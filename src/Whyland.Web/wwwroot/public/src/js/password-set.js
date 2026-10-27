/* ============================================================
   Set password — phone binding code
   Only runs on the variant of the page that asks for a phone
   number (an account that has none). The code itself is posted
   with the form; here we only request it and cool the button down.
   ============================================================ */
(function () {
  "use strict";

  document.addEventListener("DOMContentLoaded", function () {
    const sendBtn = document.getElementById("send-binding-code");
    const phoneInput = document.getElementById("bind-phone");
    const form = document.getElementById("set-password-form");
    const timerEl = document.getElementById("binding-code-timer");

    if (!sendBtn || !phoneInput || !form) return;

    // Matched with Auth:OtpResendCooldownSeconds on the server.
    const COOLDOWN_SECONDS = 60;
    let interval = null;

    function notify(type, message) {
      if (window.showToast) window.showToast(type, message);
    }

    function toPersianDigits(value) {
      const persian = ["۰", "۱", "۲", "۳", "۴", "۵", "۶", "۷", "۸", "۹"];
      return String(value).replace(/\d/g, (d) => persian[d]);
    }

    function startCooldown() {
      let remaining = COOLDOWN_SECONDS;

      sendBtn.disabled = true;
      sendBtn.classList.add("cursor-not-allowed", "opacity-60");
      if (timerEl) {
        timerEl.textContent = `ارسال مجدد کد پس از ${toPersianDigits(remaining)} ثانیه`;
      }

      clearInterval(interval);
      interval = setInterval(function () {
        remaining--;

        if (remaining <= 0) {
          clearInterval(interval);
          sendBtn.disabled = false;
          sendBtn.classList.remove("cursor-not-allowed", "opacity-60");
          if (timerEl) timerEl.textContent = "";
          return;
        }

        if (timerEl) {
          timerEl.textContent = `ارسال مجدد کد پس از ${toPersianDigits(remaining)} ثانیه`;
        }
      }, 1000);
    }

    sendBtn.addEventListener("click", async function () {
      const phone = phoneInput.value.trim();
      if (!phone) {
        notify("error", "ابتدا شماره موبایل را وارد کنید.");
        return;
      }

      // The antiforgery token is rendered inside the form by the form tag helper.
      const token = form.querySelector('input[name="__RequestVerificationToken"]');

      try {
        const res = await fetch("/Password/SendPhoneCode", {
          method: "POST",
          headers: {
            "Content-Type": "application/json",
            RequestVerificationToken: token ? token.value : "",
          },
          credentials: "same-origin",
          body: JSON.stringify({ phoneNumber: phone }),
        });

        const body = await res.json().catch(() => null);

        if (!res.ok || !body || !body.success) {
          notify("error", (body && body.error) || "خطا در ارسال کد.");
          return;
        }

        notify("success", "کد تأیید ارسال شد.");
        startCooldown();
      } catch (err) {
        notify("error", err.message);
      }
    });
  });
})();
