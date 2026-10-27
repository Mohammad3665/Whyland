# Authentication setup

Everything below runs without any credentials except where a section says otherwise.

## Sign-in methods

| Method | Route | Needs configuration |
| --- | --- | --- |
| Phone + OTP | `POST /Account/SendCode`, `POST /Account/VerifyCode`, `POST /Account/CompleteRegistration` | SMS provider (`SmsProvider:*`) |
| Password | `POST /Account/LoginWithPassword` — the number, never an e-mail address | — |
| Google | `GET /Account/ExternalChallenge` → `GET /Account/ExternalCallback` | Google OAuth client |
| Recovery (forgot) | `/Password/Forgot` → code by SMS → same page completes it (anonymous) | SMS provider + `SMS-OTP-001` template |
| Set first password | `/Password/SetPassword` (signed in) | SMS provider + `SMS-OTP-001` template once the account still has no phone |
| Change password | `/Password/Change` (signed in) | — |

Tunables live under the `Auth` section (`src/Whyland.Web/appsettings.json`): OTP length/TTL/attempts,
resend cooldown, `PasswordMinLength`, `PasswordMaxFailedAttempts`, `PasswordLockoutMinutes` and the
SMS template code.

**Phone numbers, not e-mail addresses, are the identifiers.** Signing in with a password and
recovering a forgotten password both look the account up by `Users.PhoneNumber`; an e-mail address is
only a hint carried on the row and is never proof of anything.

## Three password responsibilities, three pages

They are deliberately separate and never substitute for one another:

| Page | Who | Proves | Touches the phone number? |
| --- | --- | --- | --- |
| `Forgot` | anonymous, cannot sign in | the six-digit code texted to the account's number | no |
| `SetPassword` | signed in, **no password yet** | the SMS code for the number being bound | binds it, once |
| `Change` | signed in, **already has a password** | the current password | never |

`GET /Password/SetPassword` and `GET /Password/Change` redirect to each other according to the stored
account state (`GetPasswordPageStateQuery`), so opening the wrong one is not an error — and the
commands re-check the same state server-side, so a hand-crafted POST cannot use one flow as the other
(`Auth.AlreadyHasPassword` / `Auth.PasswordNotSet`).

`/Password/Forgot` is one page in two stages: it asks for the number, texts a code of the
`PasswordReset` purpose, and stores the new password as soon as that code is proved. There is no
reset link and no token table any more, so nothing that could be replayed ever leaves the server. The
three OTP purposes (`Login`, `PhoneBinding`, `PasswordReset`) are separate rows, so a code can only
ever be spent on the flow that issued it.

## The seeded super admin

The first start creates one administrator, and every part of it comes from the `Admin` configuration
section — changing the number is a settings edit, never a code edit:

```json
"Admin": {
  "PhoneNumber": "0912000000",
  "Email": "admin@whyland.local"
}
```

* `Admin:PhoneNumber` is what you type on the password tab of the login page. The seeder normalizes
  it exactly like the login flow does, so the two always match.
* The account is recognized by the `SuperAdmin` **role**, not by a username (the `User` entity has no
  username column). Rotate the number in configuration and the next start moves the existing account
  to it; if the new number already belongs to someone else the account keeps its current one and a
  warning is logged.
* `Admin:Password` (default `Whyland@1234`) is hashed **only while the account is being created** — a
  restart never overwrites a password. Rotate it on `/Password/Change`.
* Leave `Admin:PhoneNumber` empty and no administrator is created; a warning says why.
* Override either value without touching a committed file:
  `dotnet user-secrets set "Admin:PhoneNumber" "091xxxxxxxxx" --project src/Whyland.Web`, or
  `Admin__PhoneNumber` in the container environment.

## Where the phone number comes from

The phone number exists for one purpose: delivering the recovery code when a password is forgotten.
So it is collected only when a password is created, and an account that never sets one is never
asked for it.

* Phone + OTP sign-in binds the number as it always did.
* Google sign-in provisions a **phone-less** account and signs the user straight in — no number, no
  OTP round trip, no intermediate step.
* `/Password/SetPassword` shows a phone + code block only when the account has no verified number yet
  (`RequiresPhoneNumber`). The code is sent by `POST /Password/SendPhoneCode` and stored as an OTP row
  with purpose `PhoneBinding`.
* Login OTPs and binding OTPs are separate rows (`OtpRequests.Purpose`), so a binding code can never
  authenticate a sign-in and a login code can never bind a number.
* A number that is already bound stays bound: neither `Change` nor `SetPassword` can swap it,
  precisely so it cannot silently become the recovery channel of a different account.

## Enabling Google sign-in

Each person running this repository uses their own OAuth client. The client secret cannot be
shipped inside a public repo — it would let anyone spend the owner's quota and Google usually
blocks such a key.

1. Open <https://console.cloud.google.com/apis/credentials> in your own project.
2. Configure the **OAuth consent screen** and then **Publish** it (app status: *In production*).
   While the screen is in *Testing*, Google refuses everyone except the addresses listed under
   *Test users* and the browser comes back with `403: access_denied`.
3. **Create credentials → OAuth client ID → type: Web application.**
4. Add the *Authorized redirect URI* that matches how you run the app (see the table below), and
   the matching *Authorized JavaScript origins*.
5. Put the two values where the app will read them (next section). Never commit them.

### Redirect URI by run mode

| Run mode | URL | Redirect URI to register |
| --- | --- | --- |
| `dotnet run --project src/Whyland.Web --launch-profile https` | `https://localhost:7010` | `https://localhost:7010/signin-google` |
| `dotnet run --project src/Whyland.Web --launch-profile http` | `http://localhost:5182` | `http://localhost:5182/signin-google` |
| `docker compose up` (`compose.yaml`) | `http://localhost:5000` | `http://localhost:5000/signin-google` |
| `docker compose -f compose.yaml -f compose.debug.yaml up` | `http://localhost:5182` | `http://localhost:5182/signin-google` |

The port must match exactly; Google compares the whole URI, not the host name alone.

### Where to put the credentials

Local runs (`dotnet run`) — user secrets, tied to `UserSecretsId` in `Whyland.Web.csproj`:

```bash
dotnet user-secrets set "Auth:GoogleClientId" "xxxxxxxx.apps.googleusercontent.com" --project src/Whyland.Web
dotnet user-secrets set "Auth:GoogleClientSecret" "GOCSPX-xxxxxxxx" --project src/Whyland.Web
dotnet user-secrets list --project src/Whyland.Web   # verify
```

Container runs (`docker compose`) — copy `.env.example` to `.env` and fill in:

```bash
WHYLAND_GOOGLE_CLIENT_ID=xxxxxxxx.apps.googleusercontent.com
WHYLAND_GOOGLE_CLIENT_SECRET=GOCSPX-xxxxxxxx
```

`.env` is git-ignored; `compose.yaml` and `compose.debug.yaml` pass both values to the container as
`Auth__GoogleClientId` / `Auth__GoogleClientSecret`.

## When the keys are absent

Nothing breaks: the Google provider is not registered at all, the login page renders no Google
button, and startup logs one line saying sign-in with Google is disabled.

## How the flow is protected

- The external principal lands in a separate 10-minute cookie (`Whyland.External`) and is dropped as
  soon as the callback has read it.
- The callback either signs in through an existing `ExternalLogins` link, or provisions a **new
  account with no phone number and no password** and signs the user in. Nothing is ever linked by
  e-mail address alone: when Google returns an address that already belongs to a local account, the
  new row is created without that address instead of merging into it, because anyone can register a
  Google address and would otherwise inherit someone else's account.
- Recovery codes are stored as bcrypt hashes exactly like login codes, are single-use, expire after
  `OtpExpiryMinutes`, allow `OtpMaxAttempts` guesses and are burned when the SMS is not delivered. Only
  an account that already has a number can be recovered, and that number was verified when the first
  password was set — so a password cannot be taken over silently.
- Neither an OTP code nor a password ever reaches the log; request logging writes the path without its
  query string.
