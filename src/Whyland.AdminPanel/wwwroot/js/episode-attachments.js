//#region Episode Attachments Page Scripts
/**
 * Admin "Episode Attachments" list page (section 3.5 of the UI/UX doc):
 * files attached to a single episode, with an upload modal that
 * enforces the configured limits (max count per episode, max file
 * size, allowed extensions). Limits come from the server-rendered
 * data-* attributes on the file input, so client validation always
 * matches the server-side rules.
 * Built on top of the shared AdminCrud module (admin-crud.js).
 */

//#region CRUD Page Configuration
var page = AdminCrud.createPage({
  table: {
    selector: "#kt_episode_attachments_table",
    options: {
      pageLength: 10,
    },
  },
  search: "#attachmentSearch",
  modal: {
    id: "kt_modal_attachment",
    titleEl: "#modal-attachment-title",
    submitBtn: "kt_modal_attachment_submit",
  },
  deleteModal: {
    id: "kt_modal_delete_attachment",
    nameEl: "#delete-attachment-name",
  },
  texts: {
    createTitle: "افزودن پیوست",
    editTitle: "ویرایش پیوست",
    created: "پیوست با موفقیت آپلود شد.",
    updated: "تغییرات با موفقیت ذخیره شد.",
    deleted: "پیوست با موفقیت حذف شد.",
  },

  // Populate the form fields from an attachment object (null = create mode).
  // Note: attachments have no edit — files cannot be modified after upload,
  // only deleted and re-uploaded. The upload form has only the file input
  // (section 3.5: file selection + limits display).
  fillForm: function () {
    resetFileInput();
  },

  // Collect the form into the attachment data object sent to the server.
  getData: function () {
    var fileInput = document.getElementById("Form_AttachmentFile");
    var file = fileInput && fileInput.files.length ? fileInput.files[0] : null;
    return {
      fileName: file ? file.name : null,
      contentType: file ? file.type : null,
      size: file ? file.size : 0,
    };
  },

  validate: function () {
    var errors = [];
    var fileInput = document.getElementById("Form_AttachmentFile");

    // A file is always required: attachments cannot exist without one.
    if (!fileInput || !fileInput.files.length) {
      errors.push("انتخاب فایل پیوست الزامی است.");
      $("#Form_AttachmentFile").addClass("is-invalid");
    } else {
      $("#Form_AttachmentFile").removeClass("is-invalid");

      var file = fileInput.files[0];
      var maxCount = parseInt(fileInput.dataset.maxCount, 10) || 0;
      var maxSize = parseInt(fileInput.dataset.maxSizeBytes, 10) || 0;
      var allowed = (fileInput.getAttribute("accept") || "")
        .split(",")
        .map(function (ext) {
          return ext.trim().toLowerCase();
        })
        .filter(Boolean);

      // Extension check: compare against the allowed list from the server.
      var dotIndex = file.name.lastIndexOf(".");
      var extension =
        dotIndex >= 0 ? file.name.slice(dotIndex).toLowerCase() : "";
      if (allowed.length && allowed.indexOf(extension) === -1) {
        errors.push(
          "پسوند فایل مجاز نیست. پسوندهای مجاز: " + allowed.join("، "),
        );
      }

      // Size check: against MaxAttachmentSizeBytes.
      if (maxSize > 0 && file.size > maxSize) {
        errors.push(
          "حجم فایل بیشتر از حد مجاز است (حداکثر " +
            formatFileSize(maxSize) +
            ").",
        );
      }

      // Max count check: current rows + 1 must not exceed the limit.
      var currentCount = page.table.rows().count();
      if (maxCount > 0 && currentCount + 1 > maxCount) {
        errors.push(
          "حداکثر تعداد پیوست برای هر اپیزود " + maxCount + " مورد است.",
        );
      }
    }
    return errors;
  },
});
//#endregion

//#region File Helpers
/** Clears the file input and its auto-filled info. */
function resetFileInput() {
  var fileInput = document.getElementById("Form_AttachmentFile");
  if (fileInput) fileInput.value = "";
  $("#Form_AttachmentFile").removeClass("is-invalid");
}

/** Formats a byte count for error messages (e.g. 20971520 → "20 MB"). */
function formatFileSize(bytes) {
  if (bytes >= 1024 * 1024) {
    return Math.round(bytes / (1024 * 1024)) + " MB";
  }
  if (bytes >= 1024) {
    return Math.round(bytes / 1024) + " KB";
  }
  return bytes + " B";
}

// Show the picked file's name/size as immediate feedback next to the input.
$("#Form_AttachmentFile").on("change", function () {
  if (this.files.length) {
    var file = this.files[0];
    $(this).removeClass("is-invalid");
    toastr.info(
      file.name + " (" + formatFileSize(file.size) + ") انتخاب شد.",
      "فایل انتخاب شد",
    );
  }
});
//#endregion

//#region Inline onclick Handlers (HTML compatibility)
function openCreateModal() {
  page.openCreate();
}
function submitAttachmentForm() {
  page.submit();
}
function confirmDelete() {
  page.confirmDelete();
}
function openDeleteModal(id, name) {
  page.openDelete(id, name);
}
//#endregion
//#endregion
