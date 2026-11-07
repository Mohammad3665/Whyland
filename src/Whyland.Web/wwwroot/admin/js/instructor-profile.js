//#region Instructor Profile View Helpers
/** Copies the digit-normalized value of a view element, with icon feedback. */
function copyToClipboard(elementId, btn) {
  var raw = document.getElementById(elementId).textContent.trim();
  var text = raw.replace(/^IR/i, "").replace(/\s+/g, "");
  if (!text || text === "—") return;

  var done = function () {
    var $btn = $(btn);
    var $icon = $btn.find("i");
    var originalTitle = $btn.attr("title");
    $btn.removeAttr("title");

    $icon.removeClass("ki-copy").addClass("ki-copy-success text-success");

    var pop = bootstrap.Popover.getOrCreateInstance(btn, {
      title: "",
      content: "کپی شد",
      trigger: "manual",
      placement: "top",
      html: false,
      customClass: "copy-note-popover",
    });
    pop.show();

    setTimeout(function () {
      pop.hide();
      pop.dispose();
      $icon.removeClass("ki-copy-success text-success").addClass("ki-copy");
      if (originalTitle) $btn.attr("title", originalTitle);
      $btn.blur();
    }, 1500);
  };

  if (navigator.clipboard && navigator.clipboard.writeText) {
    navigator.clipboard.writeText(text).then(done, function () {
      fallbackCopy(text, done);
    });
  } else {
    fallbackCopy(text, done);
  }
}

/** execCommand fallback for non-secure contexts / older browsers. */
function fallbackCopy(text, done) {
  var tmp = document.createElement("textarea");
  tmp.value = text;
  tmp.style.position = "fixed";
  tmp.style.opacity = "0";
  document.body.appendChild(tmp);
  tmp.select();
  try {
    document.execCommand("copy");
    done();
  } catch (e) {
    toastr.error("کپی انجام نشد؛ لطفاً به‌صورت دستی کپی کنید.", "خطا");
  }
  document.body.removeChild(tmp);
}
//#endregion
