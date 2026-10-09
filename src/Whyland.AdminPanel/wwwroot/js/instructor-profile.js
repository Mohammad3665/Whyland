//#region Instructor Profile Page Scripts
/**
 * Instructor profile page (section 4.3 of the UI/UX doc):
 * - The page itself is a read-only profile view (identity card + info cards).
 * - All editing happens in the fullscreen modal (Add-Course style), opened
 *   with openEditModal() and saved with saveInstructorProfile().
 *
 * Demo mode: saving validates and syncs the modal values back into the
 * view elements; real persistence is wired later (see the ?handler=
 * TODO markers in _InstructorProfileModals.cshtml).
 */

//#region Helpers
/** Strips non-digits and returns the pure digit string. */
function onlyDigits(value) {
  return (value || "").replace(/\D/g, "");
}
//#endregion

//#region Copy to Clipboard (view page: sheba / account number)
/**
 * Copies the (digit-normalized) text of the given view element to the
 * clipboard, then flashes icon feedback on the clicked button.
 */
function copyToClipboard(elementId, btn) {
  var raw = document.getElementById(elementId).textContent.trim();
  var text = raw.replace(/^IR/i, "").replace(/\s+/g, "");

  var done = function () {
    var $btn = $(btn);
    var $icon = $btn.find("i");

    // Save the original title so we can restore it later.
    var originalTitle = $btn.attr("title");
    $btn.removeAttr("title");

    // Switch the icon to its "success" state.
    $icon.removeClass("ki-copy").addClass("ki-copy-success text-success");

    // Popover confirmation floating above the button (auto-hides).
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

      // Restore the icon back to its idle state.
      $icon.removeClass("ki-copy-success text-success").addClass("ki-copy");

      // Restore the original title for the native tooltip.
      if (originalTitle) {
        $btn.attr("title", originalTitle);
      }

      // Blur the button so the hover/active color resets.
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

//#region Modal Open / Fill (from the read-only view)
/**
 * Opens the fullscreen edit modal, seeding every field from the
 * current view values (demo source of truth = the view markup).
 */
function openEditModal() {
  // Profile image: copy the view avatar into the modal preview <img>.
  // (object-fit: contain on the preview <img> means nothing is cropped.)
  var viewImg = $("#View_ProfileImage").attr("src");
  setModalPreviewImage(viewImg || "/assets/media/avatars/blank.png", !viewImg);
  $("#Form_ProfileImageFile").val("");
  $('#Form_ProfileImage_wrapper input[name="Form_ProfileImage_remove"]').val(
    "",
  );

  $("#Form_Bio").val($("#View_Bio").text().trim());
  $("#bio-char-count").text($("#Form_Bio").val().length + " / 300");
  $("#Form_Biography").val($("#View_Biography").text().trim());
  $("#Form_Biography").val($("#View_Biography").text().trim());

  $("#Form_Specialization").val($("#View_Specialization").text().trim());
  $("#Form_Website").val(extractUrl($("#View_Website").text().trim()));
  $("#Form_LinkedIn").val($("#View_LinkedIn").text().trim());
  $("#Form_Instagram").val($("#View_Instagram").text().trim());

  $("#Form_Address").val($("#View_Address").text().trim());
  // Sheba stored in the view with an "IR" prefix; feed the digits only.
  $("#Form_ShebaNumber").val(
    onlyDigits($("#View_ShebaNumber").text().replace(/^IR/i, "")),
  );
  formatSheba(document.getElementById("Form_ShebaNumber"));
  $("#Form_BankAccountNumber").val($("#View_BankAccountNumber").text().trim());

  // Note: admin-crud.js is not loaded on this view page; use Bootstrap
  // directly instead of AdminCrud.Utils.showModal.
  bootstrap.Modal.getOrCreateInstance(
    document.getElementById("kt_modal_instructor_profile"),
  ).show();
}
//#endregion

//#region URL / Field Utilities
/** Extracts an https? URL from a social-link text line, or null. */
function extractUrl(text) {
  var m = text.match(/https?:\/\/\S+/);
  return m ? m[0] : "";
}

/** Normalizes the sheba input: keeps up to 24 digits, groups by 4. */
function formatSheba(input) {
  var digits = onlyDigits(input.value).slice(0, 24);
  input.value = digits.replace(/(.{4})/g, "$1 ").trim();
}
//#endregion

//#region Profile Image (modal-side preview / remove state)
/**
 * Sets the modal preview <img> to the given URL.
 * The img uses object-fit: contain, so the image is fully visible (no crop).
 * When `blank` is true the blank placeholder is shown instead (removed state).
 */
function setModalPreviewImage(url, blank) {
  var img = $("#Form_ProfileImage_preview");
  img.attr("src", url);
  img.toggleClass("opacity-25", !!blank);
}

/** Client-side review of the picked file inside the modal preview. */
function previewModalProfileImage(input) {
  if (!input.files || !input.files[0]) return;
  var file = input.files[0];

  if (file.size > 2 * 1024 * 1024) {
    toastr.error("حجم تصویر باید حداکثر ۲ مگابایت باشد.", "خطا");
    input.value = "";
    return;
  }
  if (!/^image\/(png|jpe?g|webp)$/.test(file.type)) {
    toastr.error("فرمت تصویر باید JPG، PNG یا WebP باشد.", "خطا");
    input.value = "";
    return;
  }

  var url = URL.createObjectURL(file);
  setModalPreviewImage(url, false);
  $('#Form_ProfileImage_wrapper input[name="Form_ProfileImage_remove"]').val(
    "",
  );
  // TODO: POST the file to ?handler=UploadImage and persist the returned
  // path into InstructorProfile.ProfileImage when the data layer is wired.
}

/** Marks the profile image as removed for the current save (demo). */
function markProfileImageRemoved() {
  if (
    $(`#Form_ProfileImage_wrapper input[name="Form_ProfileImage_remove"]`).val()
  ) {
    // Already removed — restore instead (toggle behavior).
    $(`#Form_ProfileImage_wrapper input[name="Form_ProfileImage_remove"]`).val(
      "",
    );
    setModalPreviewImage($("#View_ProfileImage").attr("src"), false);
    // TODO: send along with SaveProfile (?handler=SaveProfile) so the server
    // clears InstructorProfile.ProfileImage on save.
    return;
  }

  $(`#Form_ProfileImage_wrapper input[name="Form_ProfileImage_remove"]`).val(
    "1",
  );
  setModalPreviewImage("/assets/media/avatars/blank.png", true);
  $("#Form_ProfileImageFile").val("");
}
//#endregion

//#region Save (demo)
/**
 * Validates and saves the fullscreen form (demo): syncs the values back
 * into the read-only view and closes the modal.
 */
function saveInstructorProfile() {
  var btn = $("#kt_instructor_profile_submit");
  var errors = [];

  // Website: when provided, must look like a URL.
  var website = $("#Form_Website").val().trim();
  if (website && !/^https?:\/\/\S+\.\S+/.test(website)) {
    errors.push("آدرس وب‌سایت باید با https:// یا http:// شروع شود.");
    $("#Form_Website").addClass("is-invalid");
  } else {
    $("#Form_Website").removeClass("is-invalid");
  }

  // Sheba: when provided, must be exactly 24 digits.
  var sheba = onlyDigits($("#Form_ShebaNumber").val());
  if (sheba && sheba.length !== 24) {
    errors.push("شماره شبا باید دقیقاً ۲۴ رقم باشد.");
    $("#Form_ShebaNumber").addClass("is-invalid");
  } else {
    $("#Form_ShebaNumber").removeClass("is-invalid");
  }

  // Bank account: when provided, 6–20 digits.
  var account = onlyDigits($("#Form_BankAccountNumber").val());
  if (account && (account.length < 6 || account.length > 20)) {
    errors.push("شماره حساب باید بین ۶ تا ۲۰ رقم باشد.");
    $("#Form_BankAccountNumber").addClass("is-invalid");
  } else {
    $("#Form_BankAccountNumber").removeClass("is-invalid");
  }

  if (errors.length) {
    toastr.error(errors.join("<br/>"), "خطا", { timeOut: 5000 });
    return;
  }

  // Demo persistence: show the saving indicator, sync the view, close.
  var label = btn.find(".indicator-label");
  var progress = btn.find(".indicator-progress");
  label.hide();
  progress.show();
  btn.prop("disabled", true);
  // TODO: POST the form to ?handler=SaveProfile when the data layer is connected.

  setTimeout(function () {
    btn.prop("disabled", false);
    progress.hide();
    label.show();

    // --- Sync the fullscreen form back into the read-only view ---
    $("#View_Bio").text($("#Form_Bio").val().trim());
    $("#View_Biography").text($("#Form_Biography").val().trim());
    $("#View_Specialization").text($("#Form_Specialization").val().trim());
    $("#View_Specialization_inline").text(
      $("#Form_Specialization").val().trim() || "بدون تخصص ثبت‌شده",
    );
    $("#View_Address").text($("#Form_Address").val().trim());

    var w = $("#Form_Website").val().trim();
    $("#View_Website").text(w || "—");
    $("#View_WebsiteLink")
      .attr("href", w || "#")
      .toggleClass("disabled", !w);

    $("#View_LinkedIn").text($("#Form_LinkedIn").val().trim() || "—");
    $("#View_LinkedInLink")
      .attr(
        "href",
        $("#Form_LinkedIn").val().trim()
          ? "https://" + $("#Form_LinkedIn").val().trim()
          : "#",
      )
      .toggleClass("disabled", !$("#Form_LinkedIn").val().trim());

    $("#View_Instagram").text($("#Form_Instagram").val().trim() || "—");
    $("#View_InstagramLink")
      .attr(
        "href",
        $("#Form_Instagram").val().trim()
          ? "https://" + $("#Form_Instagram").val().trim()
          : "#",
      )
      .toggleClass("disabled", !$("#Form_Instagram").val().trim());

    var shebaDigits = onlyDigits($("#Form_ShebaNumber").val());
    $("#View_ShebaNumber").text(
      shebaDigits ? "IR" + shebaDigits.replace(/(.{4})/g, "$1 ").trim() : "—",
    );

    $("#View_BankAccountNumber").text(
      $("#Form_BankAccountNumber").val().trim() || "—",
    );

    var removeFlag = $(
      '#Form_ProfileImage_wrapper input[name="Form_ProfileImage_remove"]',
    ).val();
    if (removeFlag === "1") {
      $("#View_ProfileImage").attr("src", "/assets/media/avatars/blank.png");
    } else {
      // New image chosen in this session? Copy the picked file into the view
      // via a FileReader data URL (the blob: preview URL dies on modal close).
      var fileInput = document.getElementById("Form_ProfileImageFile");
      if (fileInput && fileInput.files && fileInput.files[0]) {
        var reader = new FileReader();
        reader.onload = function (e) {
          $("#View_ProfileImage").attr("src", e.target.result);
        };
        reader.readAsDataURL(fileInput.files[0]);
      }
    }

    bootstrap.Modal.getOrCreateInstance(
      document.getElementById("kt_modal_instructor_profile"),
    ).hide();
    toastr.success("اطلاعات مدرس با موفقیت ذخیره شد.", "موفق");
    // TODO: in real mode, reload the page from the server after save.
  }, 600);
}
//#endregion

//#region Numeric Input Bindings (modal)
$(function () {
  // Group/limit digit entry while typing in the modal.
  $("#Form_ShebaNumber").on("input", function () {
    formatSheba(this);
  });
  $("#Form_BankAccountNumber").on("input", function () {
    this.value = onlyDigits(this.value).slice(0, 20);
  });
});
//#endregion
//#endregion
