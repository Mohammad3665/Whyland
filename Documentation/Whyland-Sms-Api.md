# Whyland SMS service — API contract v1

Phase 5 of the development guide (s11). The service is a standalone HTTP API with **no Whyland
business logic**; the main application reaches it only through `ISmsService`
(`Whyland.Application`) → `WhylandSmsService` (`Whyland.Infrastructure`) → `Whyland.Sms.Provider`.

- Project: `src/Whyland.Sms` (default dev URL `http://localhost:5090`)
- Client package: `src/Whyland.Sms.Provider` (Refit, no dependency on the service project)
- Storage: **in-memory only**. Messages, requests and templates live for the process lifetime and
  are lost on restart — by design, so the service needs no database or migrations.

---

## 1. Versioning

Every route is prefixed with `/api/v1`. An incompatible change to a payload or a status meaning
bumps the prefix (`/api/v2`) instead of editing v1; v1 keeps working for older consumers.

## 2. Authentication

| Item | Value |
| --- | --- |
| Scheme | `SmsApiKey` |
| Policy | `SmsApi` (applied to the whole `/api/v1/sms` group) |
| Header | `X-Sms-Api-Key: <shared key>` |
| Service setting | `SmsService:ApiKey` (env `SmsService__ApiKey`) |
| Client setting | `SmsProvider:ApiKey` |

The key is compared in constant time. An unconfigured key on the service rejects **every** request
(fail closed). A wrong or missing key returns `401` with `success = false`. The key never appears
in logs or in the OpenAPI document.

## 3. Endpoints

| Method | Route | Purpose |
| --- | --- | --- |
| `POST` | `/api/v1/sms/send` | Send one message (raw text **or** template) |
| `GET` | `/api/v1/sms/messages` | Stored messages, newest first (`?recipient=`, `?status=`, `?templateCode=`, `?limit=`, default 100) |
| `GET` | `/api/v1/sms/requests` | Send requests, newest first (`?recipient=`, `?status=`, `?limit=`) |
| `GET` | `/api/v1/sms/requests/{requestId}` | One request with its messages |
| `GET` | `/api/v1/sms/{messageId}` | One message |
| `GET` | `/api/v1/sms/templates` | All templates, active and inactive |
| `GET` | `/api/v1/sms/templates/{code}` | One template |
| `POST` | `/api/v1/sms/templates` | Create a template (`409` if the code exists) |
| `PUT` | `/api/v1/sms/templates/{code}` | Replace a template |
| `DELETE` | `/api/v1/sms/templates/{code}` | Remove a template (`204`) |
| `GET` | `/openapi/v1.json` | OpenAPI document (anonymous) |

## 4. `POST /api/v1/sms/send`

Exactly one content mode: `message` **or** `templateCode` + `parameters`. Both or neither →
`Sms.AmbiguousContent`.

```json
{
  "to": "09120000000",
  "message": "کد ورود شما: 123456"
}
```

```json
{
  "to": "+989120000000",
  "templateCode": "SMS-OTP-001",
  "parameters": { "code": "123456", "expirationMinutes": 5 }
}
```

Response `200` (accepted and stored):

```json
{
  "success": true,
  "messageId": "sms_20261024153012_000001",
  "status": "Sent",
  "errorCode": null,
  "errorMessage": null
}
```

Failure uses the provider shape too, with an HTTP status that matches the cause:

```json
{
  "success": false,
  "messageId": null,
  "status": "Failed",
  "errorCode": "Sms.TemplateParameterMissing",
  "errorMessage": "Template 'SMS-OTP-001' requires parameter 'code'."
}
```

Status semantics: `Pending` (recorded, not dispatched), `Sent` (recorded and dispatched),
`Failed` (rejected). `messageId` is only returned on success.

### Recipient normalization

`to` accepts `0912…`, `+98 912…`, `0098 912…`, `912…` and spaces/dashes; it is normalized to
`09xxxxxxxxx`. Anything that is not an 11-digit `09…` number after normalization is
`Sms.InvalidRecipient`.

## 5. Templates

A template body uses `{{placeholder}}` tokens and declares each one. Saving a template whose
content uses an undeclared token is rejected (`400 Sms.InvalidTemplate`), so a curator cannot
publish a template that silently drops a value.

```json
{
  "code": "SMS-OTP-001",
  "name": "تأیید شماره تلفن",
  "content": "کد: {{code}} اعتبار: {{expirationMinutes}} دقیقه",
  "description": "OTP login code",
  "isActive": true,
  "parameters": [
    { "name": "code", "type": "string", "required": true, "defaultValue": null, "description": "OTP" },
    { "name": "expirationMinutes", "type": "integer", "required": false, "defaultValue": "5", "description": "Validity" }
  ]
}
```

- `code`: 3–40 characters of `[A-Za-z0-9-]`.
- `type`: `string` or `integer`; a value of the wrong type is `Sms.TemplateParameterInvalid`.
- An optional parameter falls back to `defaultValue` when omitted.
- Unknown or misspelled parameters → `Sms.UnknownTemplateParameter`.
- `SMS-OTP-001` is seeded at start-up so a fresh service can send OTPs immediately. Edits are
  in-memory and revert on restart.

## 6. Error codes

| Code | HTTP | Meaning |
| --- | --- | --- |
| `Sms.Unauthorized` | 401 | Missing/invalid API key |
| `Sms.InvalidRecipient` | 400 | `to` is not a valid mobile number |
| `Sms.MissingContent` | 400 | No `message` and no `templateCode` |
| `Sms.AmbiguousContent` | 400 | Both modes supplied |
| `Sms.EmptyMessage` | 400 | Rendered text is empty |
| `Sms.MessageTooLong` | 400 | Rendered text exceeds `SmsService:MaxMessageLength` |
| `Sms.TemplateNotFound` | 404 | Unknown template code |
| `Sms.TemplateDisabled` | 400 | Template exists but `isActive = false` |
| `Sms.TemplateParameterMissing` | 400 | Required parameter omitted |
| `Sms.TemplateParameterInvalid` | 400 | Parameter type mismatch |
| `Sms.UnknownTemplateParameter` | 400 | Parameter not declared by the template |
| `Sms.InvalidTemplate` | 400 / 409 | Bad definition, or code already taken |
| `Sms.MessageNotFound` / `Sms.RequestNotFound` | 404 | Unknown identifier |

Codes produced by the **client package** (never returned by the service) are namespaced
`Sms.Provider.*`: `ServiceError`, `UnexpectedResponse`, `Transport`, `Unauthorized`.

## 7. Service settings (`SmsService` section)

| Key | Default | Purpose |
| --- | --- | --- |
| `ApiKey` | empty → rejects all calls | Shared secret |
| `MaxStoredMessages` | 5000 | Ring buffer size; oldest entries are evicted |
| `MaxMessageLength` | 700 | Rejected before storage |
| `PanelUsername` | empty → panel is closed | Dashboard sign-in user |
| `PanelPassword` | empty → panel is closed | Dashboard sign-in secret |

Both panel values must be non-empty before the dashboard accepts a login; the pair is compared in
constant time and lives in user-secrets (`dotnet user-secrets set SmsService:PanelPassword … --project
src/Whyland.Sms`) or in the `SmsService__Panel*` environment variables, never in a committed file.

## 8. Consuming from the main application

`src/Whyland.Web/appsettings.json`:

```json
{
  "Sms": { "Provider": "Stub" },
  "SmsProvider": {
    "BaseUrl": "http://localhost:5090",
    "ApiKey": "",
    "TimeoutSeconds": 15
  }
}
```

- `Sms:Provider = "WhylandSms"` wires `WhylandSmsService` (HTTP); anything else keeps the
  development `StubSmsService`, which logs the OTP instead of sending it.
- The key belongs in user-secrets or the environment, e.g.
  `dotnet user-secrets set SmsProvider:ApiKey <key>` — never in a committed file.
- `docker compose` runs both containers and passes `WHYLAND_SMS_API_KEY` to each.

### Result contract

`ISmsService.SendAsync` returns `SendSmsResult`: `Success`, `MessageId`, `Status`, `ErrorCode`,
`ErrorMessage`, and `DeliveryUnknown`.

`DeliveryUnknown = true` means no HTTP answer arrived (timeout, refused connection). The message
**may** still have been accepted, so a caller must not claim the user was never contacted —
it can only say delivery is unconfirmed.

### Login flow behaviour

`RequestLoginCodeCommandHandler` persists the OTP hash first, then sends. Any unsuccessful result
(confirmed rejection **or** unconfirmed delivery) marks the stored row used so it can never be
verified, and returns `Auth.SmsSendFailed`. The resend cooldown is measured against the newest row
whether or not it is usable, so a failed send cannot be used to bypass the throttle.

## 9. Operational notes

- Nothing is persisted: restarting the service clears messages, requests and template edits.
- Only the seeded templates are recreated on start-up; templates curated through the dashboard are
  lost with the process, exactly like the traffic.
- OTP text must never be logged by the web app; the provider package logs recipients and status
  only, never message bodies.

## 10. Dashboard panel (same host, different auth)

The MVC dashboard ships inside `src/Whyland.Sms` and reads `ISmsStore` **in process** — it never
calls its own HTTP API, so it works even with the API key unset and cannot loop.

| Route | Purpose |
| --- | --- |
| `/Account/Login`, `/Account/Logout` | Cookie sign-in (scheme `SmsPanel`, 8-hour sliding session) |
| `/` (`Dashboard/Index`) | Counters over the newest 200 rows, latest messages, latest rejected calls |
| `/Messages`, `/Messages/Details/{id}` | Stored message text per recipient/status/limit |
| `/Requests`, `/Requests/Details/{id}` | Every inbound call, including rejected ones, with the raw JSON body |
| `/Templates`, `/Templates/Create`, `/Templates/Edit?code=…`, `/Templates/Preview`, `/Templates/Delete` | Template curation with a rendered preview |

The two auth paths are deliberately separate:

- `X-Sms-Api-Key` authenticates `/api/v1/sms/*` only. It does **not** open a single panel page.
- The panel cookie authenticates the MVC routes only. It does **not** authorize an API call.

This matters because stored rows contain OTP text: the panel is a human surface guarded by a
password, while the API is a machine surface guarded by a key.

Behaviour worth knowing when reading the tables:

- A rejected send creates a **request** row and no message row, so failures are counted from the
  request log; the message table only ever holds accepted text.
- The `X-Sms-Api-Key` header is never stored, and the request payload is stored only in memory —
  it is not written to any log or database.
- Every write form posts an antiforgery token, and validation runs through the same
  `SmsTemplateEditor` the API uses, so the panel cannot save a template the API would reject.
- Feedback after create/update/delete travels as `?saved=…` on `/Templates`, which keeps the browser
  behaviour correct after a POST-redirect-GET and survives a refresh.

### Stylesheet

The panel reuses the public template's design tokens (`#ff5a1f`, the semantic status colors, the
`rounded-2xl` cards, Vazirmatn, RTL). Tailwind v4 sources live in `src/Whyland.Sms/tailwind/input.css`
and the built artifact `src/Whyland.Sms/wwwroot/panel/style.css` is committed, so the container image
needs no Node toolchain. To regenerate after changing markup:

```bash
cd src/Whyland.Sms
npm install --prefer-offline   # package.json is git-ignored; see below
npm run css:build
```

`src/Whyland.Sms/package.json` and its lock file are listed in `.gitignore` on purpose — the built
CSS is the deliverable. Recreate the manifest with the same two scripts if it is missing, or borrow
the CLI from the web project (`src/Whyland.Web/node_modules/.bin/tailwindcss`) after installing there.
Tailwind only emits utilities it finds in a scanned source, so `input.css` points `@source` at
`../Views`; without those lines the stylesheet silently loses classes.

