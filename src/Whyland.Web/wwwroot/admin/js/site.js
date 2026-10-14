//#region Site-wide Scripts
/**
 * Global scripts shared by every page of the admin panel.
 * Loaded from the layout, so it must stay dependency-free.
 */
(function () {
  "use strict";

  //#region Sidebar Theme Sync
  /**
   * Keeps the sidebar layout in sync with the active color theme:
   * dark theme uses the "dark-sidebar" layout, light uses "light-sidebar".
   */
  function applyLayout() {
    var theme = document.documentElement.getAttribute("data-bs-theme");
    document.body.setAttribute(
      "data-kt-app-layout",
      theme === "dark" ? "dark-sidebar" : "light-sidebar",
    );
  }

  // Apply once when the page loads...
  document.addEventListener("DOMContentLoaded", applyLayout);

  // ...and re-apply whenever the theme attribute changes at runtime
  // (e.g. the user switches between light/dark from the topbar menu).
  new MutationObserver(applyLayout).observe(document.documentElement, {
    attributes: true,
    attributeFilter: ["data-bs-theme"],
  });
  //#endregion
})();
//#endregion
