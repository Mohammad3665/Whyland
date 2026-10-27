/* ============================================================
   Server-rendered notifications → toast
   Views that post a form cannot call showToast() themselves, so they
   emit a hidden element per message and this script spends it:
     <div hidden data-toast="success">…</div>
   Requires admin-toast.js (window.showToast) to be loaded first.
   ============================================================ */
(function () {
  "use strict";

  document.addEventListener("DOMContentLoaded", function () {
    if (!window.showToast) return;

    document.querySelectorAll("[data-toast]").forEach(function (element) {
      var type = element.getAttribute("data-toast") || "info";
      var message = (element.textContent || "").trim();

      element.remove();
      if (message) window.showToast(type, message);
    });
  });
})();
