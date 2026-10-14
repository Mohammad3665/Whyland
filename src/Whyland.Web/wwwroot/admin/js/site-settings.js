//#region Site Settings Pages Scripts
/**
 * Shared script for the three «تنظیمات سایت» pages (sections 12.1 / 12.2 of
 * the UI/UX doc):
 *
 *   /SiteSettings/General/Index   تنظیمات عمومی
 *   /SiteSettings/Seo/Index       تنظیمات سئو
 *   /SiteSettings/Contact/Index   اطلاعات تماس
 *
 * Every page first renders its settings read-only (12.1); editing happens in
 * a single modal (12.2) opened with openSettingsModal() and stored with
 * saveSettings(). The group a page owns comes from the
 * [data-settings-group] attribute of #kt_settings_page.
 *
 * Image settings (logos, favicon, OG image) render as <img> in the read-only
 * view and are edited with a file picker + preview in the modal:
 * previewSettingsImage() shows the picked file, clearSettingsImage() marks
 * the image for removal.
 *
 * Data layer is not wired up yet: saveSettings() validates the form and
 * updates the read-only view (demo mode). Replace the setTimeout block with
 * a POST to ?handler=SaveGroup&group=<key> when the backend handlers land.
 */

//#region Group Configuration
/**
 * Fields of each settings group. `key` is the SiteSetting.Key (it is also
 * the suffix of the Form_* / View_* element ids), `label` is used in
 * validation messages.
 */
var SETTINGS_GROUPS = {
  general: {
    title: "تنظیمات عمومی",
    saved: "تنظیمات عمومی با موفقیت ذخیره شد.",
    fields: [
      { key: "SiteTitle", label: "عنوان سایت", required: true },
      { key: "SiteDescription", label: "توضیحات سایت" },
      { key: "LogoDark", label: "لوگو (تم روشن)", type: "image" },
      { key: "LogoLight", label: "لوگو (تم تیره)", type: "image" },
      { key: "Favicon", label: "فاوآیکون", type: "image" },
    ],
  },
  seo: {
    title: "تنظیمات سئو",
    saved: "تنظیمات سئو با موفقیت ذخیره شد.",
    fields: [
      { key: "MetaTitle", label: "عنوان متا پیش‌فرض" },
      { key: "MetaDescription", label: "توضیحات متا پیش‌فرض" },
      { key: "MetaKeywords", label: "کلمات کلیدی پیش‌فرض" },
      { key: "OgImage", label: "تصویر Open Graph", type: "image" },
    ],
  },
  contact: {
    title: "اطلاعات تماس",
    saved: "اطلاعات تماس با موفقیت ذخیره شد.",
    fields: [
      { key: "Phone", label: "تلفن", required: true, validate: isPhone },
      { key: "Email", label: "ایمیل", required: true, validate: isEmail },
      { key: "Address", label: "آدرس" },
    ],
  },
};

/** Shown by image fields that currently have no image. */
var SETTINGS_IMAGE_PLACEHOLDER = "/assets/media/svg/files/blank-image.svg";

/** Maximum accepted image size (matches the hint under each image field). */
var SETTINGS_IMAGE_MAX_BYTES = 2 * 1024 * 1024;

/**
 * State of the image fields while the modal is open:
 * key -> { src, file, removed } — `src` is what the preview shows,
 * `file` is a newly picked file and `removed` means the image was cleared.
 */
var settingsImageState = {};

/** Group handled by the current page, taken from the page wrapper element. */
function settingsGroupKey() {
  var page = document.getElementById("kt_settings_page");
  return page ? page.getAttribute("data-settings-group") : null;
}

/** Configuration of the current page's settings group. */
function settingsConfig() {
  return SETTINGS_GROUPS[settingsGroupKey()] || null;
}
//#endregion

//#region Read-only View <-> Modal
/** Raw value stored on a read-only view element ("" when unset). */
function getSettingValue(key) {
  var el = document.getElementById("View_" + key);
  return el ? el.getAttribute("data-value") || "" : "";
}

/**
 * Updates a read-only view element after a successful (demo) save:
 * images are swapped by their src, text fields by their text.
 */
function setSettingValue(key, value) {
  var el = document.getElementById("View_" + key);
  if (!el) return;

  el.setAttribute("data-value", value);

  if (el.tagName === "IMG") {
    el.classList.toggle("opacity-50", value.length === 0);
    el.src = value.length ? value : SETTINGS_IMAGE_PLACEHOLDER;
    return;
  }

  el.textContent = value.length ? value : "—";
  el.classList.toggle("text-muted", value.length === 0);
}

/** Copies the current values into the edit modal and shows it. */
function openSettingsModal() {
  var cfg = settingsConfig();
  if (!cfg) return;

  cfg.fields.forEach(function (field) {
    if (field.type === "image") {
      // Reset the picker: the modal edits the value currently on the page.
      settingsImageState[field.key] = {
        src: getSettingValue(field.key),
        file: null,
        removed: false,
      };
      resetSettingsImageInput(field.key);
      renderSettingsImagePreview(field.key);
      return;
    }

    $("#Form_" + field.key)
      .val(getSettingValue(field.key))
      .removeClass("is-invalid");
  });

  AdminCrud.Utils.showModal("kt_modal_settings_edit");
}
//#endregion

//#region Image Fields (picker + preview)
/** Clears the file picker so picking the same file again still fires. */
function resetSettingsImageInput(key) {
  $("#Form_" + key + "File").val("");
}

/** Puts the modal's preview image in sync with the field state. */
function renderSettingsImagePreview(key) {
  var state = settingsImageState[key];
  if (!state) return;

  var preview = document.getElementById("Form_" + key + "_preview");
  if (!preview) return;

  preview.src = state.src.length ? state.src : SETTINGS_IMAGE_PLACEHOLDER;
  preview.classList.toggle("opacity-50", state.src.length === 0);
}

/** Validates and previews an image picked in the modal's file input. */
function previewSettingsImage(input, key) {
  if (!input.files || !input.files[0]) return;
  var file = input.files[0];

  if (!isImageFile(file)) {
    toastr.error("فقط فایل تصویری (PNG، JPG، WebP، SVG، ICO) مجاز است.", "خطا");
    input.value = "";
    return;
  }
  if (file.size > SETTINGS_IMAGE_MAX_BYTES) {
    toastr.error("حجم تصویر باید حداکثر ۲ مگابایت باشد.", "خطا");
    input.value = "";
    return;
  }

  settingsImageState[key] = {
    // Object URL only lives while the modal is open; the view gets a data
    // URL on save (same approach as the instructor profile image).
    src: URL.createObjectURL(file),
    file: file,
    removed: false,
  };
  renderSettingsImagePreview(key);
}

/** Marks an image field as empty (the image is removed on save). */
function clearSettingsImage(key) {
  settingsImageState[key] = { src: "", file: null, removed: true };
  resetSettingsImageInput(key);
  renderSettingsImagePreview(key);
}
//#endregion

//#region Save (current group)
/** Validates the modal form, then persists the group (demo mode). */
function saveSettings() {
  var cfg = settingsConfig();
  if (!cfg) return;

  var values = []; // SiteSetting payload rows
  var displays = []; // values written back into the read-only view
  var errors = [];

  cfg.fields.forEach(function (field) {
    if (field.type === "image") {
      var state = settingsImageState[field.key] || { src: "", file: null, removed: false };
      var row = { key: field.key };

      if (state.removed) {
        row.value = ""; // clear the image
      } else if (state.file) {
        row.value = null; // a new file is picked: set by the upload handler
        row.file = state.file;
      } else {
        row.value = state.src; // unchanged
      }

      values.push(row);
      displays.push({ key: field.key, value: state.src, file: state.file });
      return;
    }

    var $input = $("#Form_" + field.key);
    var value = String($input.val() || "").trim();
    $input.removeClass("is-invalid");

    if (field.required && value.length === 0) {
      errors.push("فیلد «" + field.label + "» الزامی است.");
      $input.addClass("is-invalid");
      return;
    }
    if (value.length > 0 && field.validate && !field.validate(value)) {
      errors.push("مقدار فیلد «" + field.label + "» معتبر نیست.");
      $input.addClass("is-invalid");
      return;
    }

    // Setting rows match the SiteSetting entity (key + value);
    // the group is implied by the page.
    values.push({ key: field.key, value: value });
    displays.push({ key: field.key, value: value });
  });

  if (errors.length) {
    toastr.error(errors[0], "خطا");
    return;
  }

  // Show the busy indicator while saving.
  var btn = document.getElementById("kt_settings_submit");
  btn.setAttribute("data-kt-indicator", "on");
  btn.disabled = true;

  // Demo save — TODO: POST values to ?handler=SaveGroup&group=<key>
  console.log("Save settings group " + settingsGroupKey(), values);
  setTimeout(function () {
    displays.forEach(function (item) {
      if (item.file) {
        // Newly picked image: a data URL survives the blob preview URL.
        var reader = new FileReader();
        reader.onload = function (e) {
          setSettingValue(item.key, e.target.result);
        };
        reader.readAsDataURL(item.file);
        return;
      }
      setSettingValue(item.key, item.value);
    });

    btn.removeAttribute("data-kt-indicator");
    btn.disabled = false;
    AdminCrud.Utils.hideModal("kt_modal_settings_edit");
    toastr.success(cfg.saved, "موفق");
  }, 700);
}
//#endregion

//#region Field Validators
/** Accepts the image formats offered by the pickers (ICO has no MIME type). */
function isImageFile(file) {
  if (/^image\//.test(file.type)) return true;
  return /\.(png|jpe?g|webp|svg|ico)$/i.test(file.name || "");
}

/** Accepts a simple email address. */
function isEmail(value) {
  return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value);
}

/** Accepts digits with optional spaces/dashes/parentheses (at least 8 digits). */
function isPhone(value) {
  if (!/^[\d\s()+-]+$/.test(value)) return false;
  return value.replace(/\D/g, "").length >= 8;
}
//#endregion
