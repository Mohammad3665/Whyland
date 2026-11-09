//#region Episode Attachments Page Scripts
/**
 * Admin "Episode Attachments" page: the downloadable files of one episode.
 * Rows, uploads and deletes go through the Razor Page handlers and the shared AdminCrud module.
 * The upload limits (max count per episode, max file size, allowed extensions) are rendered by
 * the server into the file input's data-* / accept attributes, so client validation always
 * matches the server-side rules.
 */

var page; // shared CRUD page controller (set in initPage)

//#region Server Connectivity (Razor Page handlers)
function csrfToken() {
  return $('input[name="__RequestVerificationToken"]').val();
}

function episodeId() {
  return parseInt($("#episodeId").val(), 10) || 0;
}

function postHandler(handler, data) {
  var dfd = $.Deferred();
  $.post({
    url: location.pathname + "?handler=" + handler,
    data: data,
    headers: { RequestVerificationToken: csrfToken() },
  })
    .done(function (res) {
      if (res && res.isSuccess) dfd.resolve(res);
      else dfd.reject({ responseJSON: res });
    })
    .fail(function (xhr) {
      dfd.reject(xhr);
    });
  return dfd.promise();
}

function uploadAttachment(file) {
  var formData = new FormData();
  formData.append("episodeId", episodeId());
  formData.append("file", file);

  var dfd = $.Deferred();
  $.ajax({
    url: location.pathname + "?handler=Upload",
    type: "POST",
    data: formData,
    processData: false,
    contentType: false,
    headers: { RequestVerificationToken: csrfToken() },
  })
    .done(function (res) {
      if (res && res.isSuccess) dfd.resolve(res);
      else dfd.reject({ responseJSON: res });
    })
    .fail(function (xhr) {
      dfd.reject(xhr);
    });
  return dfd.promise();
}

function deleteAttachment(id) {
  return postHandler("Delete", { id: id }).then(function (res) {
    // Keep the confirmation open until the table has actually dropped the row.
    return refreshRows().then(function () {
      return res;
    });
  });
}
//#endregion

//#region Row Rendering
function faNumber(value) {
  return Number(value || 0).toLocaleString("fa-IR");
}

function formatFileSize(bytes) {
  if (bytes >= 1024 * 1024) return Math.round(bytes / (1024 * 1024)) + " MB";
  if (bytes >= 1024) return Math.round(bytes / 1024) + " KB";
  return (bytes || 0) + " B";
}

function formatDate(value) {
  if (!value) return "—";
  var date = new Date(value);
  if (isNaN(date.getTime())) return "—";
  return date.toLocaleDateString("fa-IR");
}

function downloadUrl(id) {
  return "/media/attachments/" + id;
}

function buildAttachmentRow(attachment) {
  var $row = $(
    '<tr><td class="dtr-control-col"></td>' +
      "<td>" +
      '<div class="d-flex align-items-center">' +
      '<i class="ki-duotone ki-file-down fs-2x text-primary me-3">' +
      '<span class="path1"></span><span class="path2"></span></i>' +
      '<span class="text-gray-800 fw-bold attachment-name" dir="ltr"></span>' +
      "</div></td>" +
      '<td><span class="badge badge-light-primary attachment-type" dir="ltr"></span></td>' +
      '<td class="attachment-size"></td>' +
      '<td class="attachment-date"></td>' +
      '<td class="text-end nowrap-cell"></td>' +
      "</tr>",
  );

  $row.attr("data-id", attachment.id);

  var cells = $row.find("td");
  $(cells[1]).find(".attachment-name").text(attachment.fileName);
  $(cells[2]).find(".attachment-type").text(attachment.contentType);
  $(cells[3]).append(
    $('<span class="text-gray-600" dir="ltr">').text(
      formatFileSize(attachment.size),
    ),
  );
  $(cells[3]).attr("data-order", attachment.size);

  $(cells[4]).append($('<span class="text-gray-600">').text(formatDate(attachment.createdAt)));
  $(cells[4]).attr("data-order", attachment.createdAt);

  var $download = $(
    '<a class="btn btn-sm btn-icon btn-bg-light btn-active-color-primary me-1" title="دانلود" target="_blank" rel="noopener noreferrer">',
  )
    .attr("href", downloadUrl(attachment.id))
    .append(
      '<i class="ki-duotone ki-file-down fs-2"><span class="path1"></span><span class="path2"></span></i>',
    );

  var $delete = $(
    '<button type="button" class="btn btn-sm btn-icon btn-bg-light btn-active-color-danger" title="حذف">',
  ).append(
    '<i class="ki-duotone ki-trash fs-2"><span class="path1"></span><span class="path2"></span><span class="path3"></span><span class="path4"></span><span class="path5"></span></i>',
  );
  $delete.on("click", function () {
    if (page) page.openDelete(attachment.id, attachment.fileName);
  });

  $(cells[5]).append($download, $delete);
  return $row[0];
}

function loadRows() {
  return $.getJSON(
    location.pathname + "?handler=Rows&episodeId=" + episodeId(),
  ).then(function (rows) {
    var nodes = [];
    for (var i = 0; i < rows.length; i++) nodes.push(buildAttachmentRow(rows[i]));
    return nodes;
  });
}

function refreshRows() {
  return loadRows().then(function (rows) {
    $("#kt_episode_attachments_table tbody").empty().append(rows);
    page.table.clear();
    page.table.rows.add(rows).draw();
  });
}
//#endregion

//#region CRUD Page Configuration
function initPage() {
  page = AdminCrud.createPage({
    table: {
      selector: "#kt_episode_attachments_table",
      options: {
        columnDefs: [
          { orderable: false, targets: [0, 5] },
          { className: "all", targets: [0, 1] },
          { className: "min-tablet", targets: [2, 3, 4, 5] },
          { className: "dtr-control", targets: 0 },
        ],
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
      editTitle: "افزودن پیوست",
      created: "پیوست با موفقیت آپلود شد.",
      updated: "تغییرات با موفقیت ذخیره شد.",
      deleted: "پیوست با موفقیت حذف شد.",
    },

    // Attachments cannot be edited after upload: the form only chooses a new file.
    fillForm: function () {
      resetFileInput();
    },

    getData: function () {
      return {};
    },

    validate: function () {
      var errors = [];
      var fileInput = document.getElementById("Form_AttachmentFile");

      if (!fileInput || !fileInput.files.length) {
        errors.push("انتخاب فایل پیوست الزامی است.");
        $("#Form_AttachmentFile").addClass("is-invalid");
        return errors;
      }

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

      var dotIndex = file.name.lastIndexOf(".");
      var extension = dotIndex >= 0 ? file.name.slice(dotIndex).toLowerCase() : "";

      if (allowed.length && allowed.indexOf(extension) === -1) {
        errors.push(
          "پسوند فایل مجاز نیست. پسوندهای مجاز: " + allowed.join("، "),
        );
      }

      if (maxSize > 0 && file.size > maxSize) {
        errors.push(
          "حجم فایل بیشتر از حد مجاز است (حداکثر " +
            formatFileSize(maxSize) +
            ").",
        );
      }

      var currentCount = page.table.rows().count();
      if (maxCount > 0 && currentCount + 1 > maxCount) {
        errors.push("حداکثر تعداد پیوست برای هر اپیزود " + maxCount + " مورد است.");
      }

      return errors;
    },

    onSubmit: function () {
      var fileInput = document.getElementById("Form_AttachmentFile");
      var file = fileInput && fileInput.files.length ? fileInput.files[0] : null;
      if (!file) return $.Deferred().reject({ responseJSON: { message: "فایل انتخاب نشده است." } }).promise();

      return uploadAttachment(file).then(function (res) {
        // Resolve only after the table shows the new row, so the modal closes on fresh state.
        return refreshRows().then(function () {
          return res;
        });
      });
    },

    onDelete: deleteAttachment,
  });
}
//#endregion

//#region File Helpers
/** Clears the file input and its validation state. */
function resetFileInput() {
  var fileInput = document.getElementById("Form_AttachmentFile");
  if (fileInput) fileInput.value = "";
  $("#Form_AttachmentFile").removeClass("is-invalid");
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

//#region Inline onclick Handlers
function openCreateModal() {
  if (page) page.openCreate();
}

function submitAttachmentForm() {
  if (page) page.submit();
}

function confirmDelete() {
  if (page) page.confirmDelete();
}

function openDeleteModal(id, name) {
  if (page) page.openDelete(id, name);
}
//#endregion

//#region Boot
$(function () {
  loadRows()
    .then(function (rows) {
      $("#kt_episode_attachments_table tbody").append(rows);
      initPage();
    })
    .fail(function () {
      initPage();
      toastr.error("خطا در دریافت پیوست‌ها.", "خطا");
    });
});
//#endregion
//#endregion
