/* =========================================================
 * persian-digits.js — the panel's output digit interceptor
 * Dependencies: none. Loaded from the admin layout, before any page script.
 *
 * Every number the panel paints is written with Persian digits, whatever produced it: a
 * DataTables cell, the paging buttons, the «نمایش ۱ تا ۱۰ از ۵۷ مورد» line, the page-length
 * menu, a details modal, a toast. Converting where each value is rendered meant one call per
 * column per page, and the chrome DataTables writes itself could not be reached that way at
 * all. So the conversion happens once, on the way into the document: a MutationObserver
 * watches the text that is actually painted and rewrites the digits in place. A page gets it
 * without asking for it, and a table built by any code path is covered the same way.
 *
 * Only the display is touched. Input values, `value`/`data-*` attributes and everything a
 * script reads back stay Latin, which is what the server binder and the JavaScript expect.
 * ========================================================= */
(function (window, document) {
  "use strict";

  var FA_DIGITS = "۰۱۲۳۴۵۶۷۸۹";

  // Text that is code or an editable surface rather than a value shown to the operator.
  // TinyMCE lives in an iframe, so it is out of this document's reach anyway.
  var SKIP_TAGS = {
    SCRIPT: true,
    STYLE: true,
    NOSCRIPT: true,
    TEMPLATE: true,
    TEXTAREA: true,
    IFRAME: true,
  };

  /**
   * The Latin letter next to a digit run, or "" when there is none that makes the run part of
   * an identifier. The T and Z of an ISO timestamp are separators between digits, not letters
   * of a name, so «۲۰۲۶-۱۱-۰۷T۱۰:۳۰» still converts.
   */
  function glueLetter(text, index) {
    var character = text.charAt(index);
    if (!/[A-Za-z]/.test(character)) return "";

    var digitBefore = /\d/.test(text.charAt(index - 1));
    var digitAfter = index + 1 >= text.length || /\d/.test(text.charAt(index + 1));
    if ((character === "T" || character === "Z") && digitBefore && digitAfter) return "";

    return character;
  }

  /**
   * Rewrites the Latin digits of a string as Persian ones.
   *
   * A run glued to a Latin letter belongs to an identifier — a slug, a GUID, a file name —
   * and is left alone: «web2» and «3f2a9c4e-…» are names, not numbers, and digits rewritten
   * inside them would change what the operator reads rather than how a count looks.
   */
  function toPersian(value) {
    var text = value === null || value === undefined ? "" : String(value);

    return text.replace(/\d+/g, function (run, offset) {
      if (glueLetter(text, offset - 1) || glueLetter(text, offset + run.length))
        return run;

      return run.replace(/\d/g, function (digit) {
        return FA_DIGITS[+digit];
      });
    });
  }

  /** Rewrites the digits of one node and everything under it, in place. */
  function paint(node) {
    if (node.nodeType === 3) {
      var parent = node.parentElement;
      if (parent && (SKIP_TAGS[parent.nodeName] || parent.isContentEditable)) return;

      // Writing only when the text really changes is what keeps the observer from feeding
      // itself: toPersian is idempotent, so the second pass has nothing left to rewrite.
      var persian = toPersian(node.nodeValue);
      if (persian !== node.nodeValue) node.nodeValue = persian;
      return;
    }

    if (node.nodeType !== 1 || SKIP_TAGS[node.nodeName] || node.isContentEditable) return;

    for (var child = node.firstChild; child; child = child.nextSibling) paint(child);
  }

  /** Converts what is already on the page, then every text the page writes from now on. */
  function start() {
    paint(document.body);

    new MutationObserver(function (records) {
      for (var i = 0; i < records.length; i++) {
        var record = records[i];

        if (record.type === "characterData") {
          paint(record.target);
          continue;
        }

        for (var j = 0; j < record.addedNodes.length; j++) paint(record.addedNodes[j]);
      }
    }).observe(document.body, { childList: true, subtree: true, characterData: true });
  }

  start();

  // The one digit definition of the panel. AdminTable.faDigits and the page scripts read it
  // from here so a string built in JavaScript matches what the interceptor paints.
  window.PersianDigits = {
    toPersian: toPersian,
    paint: paint,
  };
})(window, document);