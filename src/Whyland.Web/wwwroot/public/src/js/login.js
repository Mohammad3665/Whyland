//#region Login / OTP Page
/* ============================================================
   Login / OTP Page — Whyland (wired to the real backend)
   Handles two independent concerns:
   1. Method Tabs    — switch between "OTP" and "Password" panels
   2. Login Flow     — mobile → OTP code → (new user) profile
                        completion, entirely on ONE page: every
                        step talks to /Account/* JSON endpoints
                        via fetch, so the URL NEVER changes.
   Steps are toggled client-side exactly like the raw template.
   ============================================================ */
(function () {
  "use strict";

  //#region DOM Ready
  document.addEventListener("DOMContentLoaded", function () {
    //#region Method Tabs
    const methodTabs = document.querySelectorAll(".method-tab");
    const methodPanels = {
      otp: document.getElementById("method-otp"),
      password: document.getElementById("method-password"),
    };

    function setActiveTab(activeTab) {
      const method = activeTab.getAttribute("data-method");
      methodTabs.forEach(function (t) {
        const active = t === activeTab;
        t.setAttribute("aria-selected", String(active));
        t.classList.toggle("bg-white", active);
        t.classList.toggle("text-primary", active);
        t.classList.toggle("shadow-sm", active);
        t.classList.toggle("text-dark/60", !active);
        t.classList.toggle("hover:text-dark", !active);
      });
      Object.keys(methodPanels).forEach(function (key) {
        if (methodPanels[key]) {
          methodPanels[key].classList.toggle("hidden", key !== method);
        }
      });
    }

    const initialTab = document.querySelector(
      '.method-tab[aria-selected="true"]',
    );
    if (initialTab) setActiveTab(initialTab);
    methodTabs.forEach(function (tab) {
      tab.addEventListener("click", function () {
        setActiveTab(tab);
      });
    });
    //#endregion

    //#region Flow — DOM References
    const step1 = document.getElementById("otp-step-1");
    const step2 = document.getElementById("otp-step-2");
    const step3 = document.getElementById("otp-step-3");

    const mobileForm = document.getElementById("otp-mobile-form");
    const mobileInput = document.getElementById("mobile-number");
    const displayMobile = document.getElementById("display-mobile");

    const otpInputs = document.querySelectorAll("[data-otp]");
    const timerEl = document.getElementById("otp-timer");
    const resendBtn = document.getElementById("resend-otp");
    const backBtn = document.getElementById("back-to-step-1");

    const otpCodeForm = document.getElementById("otp-code-form");
    const profileForm = document.getElementById("profile-form");

    const passwordForm = document.getElementById("password-login-form");
    const passwordPhone = document.getElementById("login-phone");
    const passwordSecret = document.getElementById("password-login-secret");

    // Flow state shared across steps (kept in memory — never in the URL).
    const state = { phone: "", ticket: "" };

    // Timer state
    let timerInterval = null;
    let remaining = 120; // 2 minutes
    //#endregion

    //#region Helpers
    function toPersianDigits(num) {
      const persian = ["۰", "۱", "۲", "۳", "۴", "۵", "۶", "۷", "۸", "۹"];
      return String(num).replace(/\d/g, (d) => persian[d]);
    }

    function formatTime(seconds) {
      const m = Math.floor(seconds / 60)
        .toString()
        .padStart(2, "0");
      const s = (seconds % 60).toString().padStart(2, "0");
      return toPersianDigits(m) + ":" + toPersianDigits(s);
    }

    /**
     * POST JSON to a same-origin endpoint and return the parsed body.
     * The Origin header sent automatically by the browser is what the
     * server uses for its same-origin (CSRF) check.
     * @param {string} url
     * @param {object} data
     * @returns {Promise<object>}
     */
    async function postJson(url, data) {
      const res = await fetch(url, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        credentials: "same-origin",
        body: JSON.stringify(data),
      });
      const body = await res.json().catch(() => null);
      if (!res.ok || !body) {
        throw new Error(
          (body && body.error) || "خطایی رخ داد؛ لطفاً دوباره تلاش کنید.",
        );
      }
      return body;
    }

    /** Shows an error through the toast component (loaded before this script). */
    function notifyError(message) {
      if (window.showToast) window.showToast("error", message);
    }

    function showStep(target) {
      [step1, step2, step3].forEach((s) => {
        if (s) s.classList.toggle("hidden", s !== target);
      });
    }
    //#endregion

    //#region OTP Countdown Timer
    function startTimer() {
      remaining = 120;
      if (timerEl) timerEl.textContent = formatTime(remaining);
      if (resendBtn) {
        resendBtn.disabled = true;
        resendBtn.classList.add("text-dark/40", "cursor-not-allowed");
        resendBtn.classList.remove(
          "text-primary",
          "cursor-pointer",
          "hover:underline",
        );
      }

      clearInterval(timerInterval);
      timerInterval = setInterval(() => {
        remaining--;
        if (remaining <= 0) {
          clearInterval(timerInterval);
          if (timerEl) timerEl.textContent = toPersianDigits("00:00");
          if (resendBtn) {
            resendBtn.disabled = false;
            resendBtn.classList.remove("text-dark/40", "cursor-not-allowed");
            resendBtn.classList.add(
              "text-primary",
              "cursor-pointer",
              "hover:underline",
            );
          }
        } else if (timerEl) {
          timerEl.textContent = formatTime(remaining);
        }
      }, 1000);
    }
    //#endregion

    //#region Step 1 — Mobile Submission (POST /Account/SendCode)
    if (mobileForm) {
      mobileForm.addEventListener("submit", async function (e) {
        e.preventDefault(); // stay on this page — no URL change

        const phone = ((mobileInput && mobileInput.value) || "").trim();
        if (!phone) {
          notifyError("شماره موبایل را وارد کنید.");
          return;
        }

        try {
          const result = await postJson("/Account/SendCode", {
            phoneNumber: phone,
          });
          if (!result.success) {
            notifyError(result.error || "خطا در ارسال کد.");
            return;
          }

          // Keep the SERVER-normalized phone for every later step.
          state.phone = result.phoneNumber || phone;

          if (displayMobile) {
            displayMobile.textContent = toPersianDigits(state.phone);
          }
          showStep(step2);
          startTimer();
          if (otpInputs && otpInputs.length) otpInputs[0].focus();
        } catch (err) {
          notifyError(err.message);
        }
      });
    }
    //#endregion

    //#region Step 2 — OTP Inputs (auto-focus between boxes)
    otpInputs.forEach((input, idx) => {
      input.addEventListener("input", (e) => {
        e.target.value = e.target.value.replace(/[^0-9]/g, "");
        if (e.target.value && idx < otpInputs.length - 1) {
          otpInputs[idx + 1].focus();
        }
      });
      input.addEventListener("keydown", (e) => {
        if (e.key === "Backspace" && !e.target.value && idx > 0) {
          otpInputs[idx - 1].focus();
        }
      });
    });
    //#endregion

    //#region Step 2 — Resend (POST /Account/SendCode again)
    if (resendBtn) {
      resendBtn.addEventListener("click", async function () {
        if (resendBtn.disabled) return;
        try {
          const result = await postJson("/Account/SendCode", {
            phoneNumber: state.phone,
          });
          if (!result.success) {
            notifyError(result.error || "خطا در ارسال کد.");
            return;
          }
          otpInputs.forEach((i) => (i.value = ""));
          otpInputs[0].focus();
          startTimer();
        } catch (err) {
          notifyError(err.message);
        }
      });
    }

    // Back to Step 1 (change phone) — still the same URL.
    if (backBtn) {
      backBtn.addEventListener("click", function () {
        clearInterval(timerInterval);
        state.phone = "";
        if (mobileInput) mobileInput.value = "";
        otpInputs.forEach((i) => (i.value = ""));
        showStep(step1);
      });
    }
    //#endregion

    //#region Step 2 → Step 3 — Verify Code (POST /Account/VerifyCode)
    if (otpCodeForm) {
      otpCodeForm.addEventListener("submit", async function (e) {
        e.preventDefault(); // stay on this page — no URL change

        const code = Array.from(otpInputs)
          .map((i) => i.value)
          .join("");
        if (code.length !== 6) {
          notifyError("کد ۶ رقمی را کامل وارد کنید.");
          return;
        }

        try {
          const result = await postJson("/Account/VerifyCode", {
            phoneNumber: state.phone,
            code: code,
          });

          if (!result.success) {
            notifyError(result.error || "کد نامعتبر است.");
            return;
          }

          clearInterval(timerInterval);

          if (result.requiresRegistration) {
            // New account → carry the ticket to step 3, still on this page.
            state.ticket = result.ticket;
            if (step3) {
              const ticketField = step3.querySelector('input[name="ticket"]');
              if (ticketField) ticketField.value = state.ticket;
            }
            showStep(step3);
            const firstNameInput =
              document.getElementById("profile-first-name");
            if (firstNameInput) firstNameInput.focus();
          } else {
            // Existing account → the ONLY navigation in the flow.
            window.location.href = result.redirectUrl || "/";
          }
        } catch (err) {
          notifyError(err.message);
        }
      });
    }
    //#endregion

    //#region Step 3 — Profile Completion (POST /Account/CompleteRegistration)
    if (profileForm) {
      profileForm.addEventListener("submit", async function (e) {
        e.preventDefault(); // stay on this page — no URL change

        const firstNameInput = document.getElementById("profile-first-name");
        const lastNameInput = document.getElementById("profile-last-name");
        const firstName = (
          (firstNameInput && firstNameInput.value) ||
          ""
        ).trim();
        const lastName = ((lastNameInput && lastNameInput.value) || "").trim();
        if (!firstName) {
          notifyError("نام را وارد کنید.");
          return;
        }
        if (!lastName) {
          notifyError("نام خانوادگی را وارد کنید.");
          return;
        }

        const passwordField = profileForm.querySelector(
          'input[name="password"]',
        );
        const confirmPasswordField = profileForm.querySelector(
          'input[name="confirm_password"]',
        );
        const password = (passwordField && passwordField.value) || "";
        const confirm = (confirmPasswordField && confirmPasswordField.value) || "";
        if (password && password !== confirm) {
          notifyError("رمز عبور و تایید آن یکسان نیستند.");
          return;
        }

        try {
          const result = await postJson("/Account/CompleteRegistration", {
            ticket: state.ticket,
            firstName: firstName,
            lastName: lastName,
            password: password || null,
          });

          if (!result.success) {
            if (result.restart) {
              // Stale ticket → restart at step 1, same page/URL.
              state.ticket = "";
              state.phone = "";
              if (mobileInput) mobileInput.value = "";
              showStep(step1);
              return;
            }
            notifyError(result.error || "خطا در ثبت‌نام.");
            return;
          }

          // Account created + signed in → final navigation to the index.
          window.location.href = result.redirectUrl || "/";
        } catch (err) {
          notifyError(err.message);
        }
      });
    }
    //#endregion

    //#region Password tab — POST /Account/LoginWithPassword
    if (passwordForm) {
      passwordForm.addEventListener("submit", async function (e) {
        e.preventDefault(); // stay on this page — no URL change

        const phone = ((passwordPhone && passwordPhone.value) || "").trim();
        const secret = (passwordSecret && passwordSecret.value) || "";
        if (!phone || !secret) {
          notifyError("شماره موبایل و رمز عبور را وارد کنید.");
          return;
        }

        try {
          const result = await postJson("/Account/LoginWithPassword", {
            phoneNumber: phone,
            password: secret,
          });
          if (!result.success) {
            notifyError(result.error || "ورود ناموفق بود.");
            return;
          }
          window.location.href = result.redirectUrl || "/";
        } catch (err) {
          notifyError(err.message);
        }
      });
    }
    //#endregion
  });
  //#endregion
})();
//#endregion
