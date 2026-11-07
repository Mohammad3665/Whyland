//#region Course FAQs Page Scripts
/**
 * Admin "Course FAQs" page: the questions attached to one course, managed from the course page.
 * Rows, saves, deletes and drag & drop ordering all go through the Razor Page handlers and the
 * shared AdminCrud module.
 */

var page; // shared CRUD page controller (set in initPage)
var reorder; // shared drag & drop ordering controller (set in initPage)

//#region Server Connectivity (Razor Page handlers)
function csrfToken() {
  return $('input[name="__RequestVerificationToken"]').val();
}

function courseId() {
  return parseInt($("#courseId").val(), 10) || 0;
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

function saveFaq(data, mode) {
  if (mode === "create") data.courseId = courseId();
  return postHandler(mode === "create" ? "Create" : "Update", data).done(refreshRows);
}

function deleteFaq(id) {
  return postHandler("Delete", { id: id }).done(refreshRows);
}

function saveOrder(rows) {
  $.ajax({
    url: location.pathname + "?handler=Reorder&courseId=" + courseId(),
    type: "POST",
    contentType: "application/json; charset=UTF-8",
    data: JSON.stringify(rows),
    headers: { RequestVerificationToken: csrfToken() },
  })
    .done(function (res) {
      if (res && res.isSuccess) {
        toastr.success("ترتیب سوالات متداول با موفقیت ذخیره شد.", "موفق");
      } else {
        toastr.error((res && res.message) || "خطا در ذخیره ترتیب.", "خطا");
        refreshRows();
      }
    })
    .fail(function () {
      toastr.error("خطا در ذخیره ترتیب.", "خطا");
      refreshRows();
    });
}
//#endregion

//#region Row Rendering
function faNumber(value) {
  return Number(value || 0).toLocaleString("fa-IR");
}

function buildFaqRow(faq) {
  var $row = $(
    '<tr><td class="dtr-control-col"></td>' +
      '<td tabindex="0" class="reorder text-center dt-type-numeric dtr-control">' +
      '<div class="d-flex align-items-center justify-content-center">' +
      '<i class="ki-duotone ki-burger-menu-2 fs-2 text-gray-400 cursor-move me-2">' +
      '<span class="path1"></span><span class="path2"></span><span class="path3"></span>' +
      '<span class="path4"></span><span class="path5"></span><span class="path6"></span>' +
      '<span class="path7"></span><span class="path8"></span><span class="path9"></span><span class="path10"></span>' +
      "</i>" +
      '<span class="course-faq-order badge badge-light-secondary"></span>' +
      "</div></td>" +
      "<td></td>" +
      '<td class="text-end nowrap-cell"></td>' +
      "</tr>",
  );

  $row.attr("data-id", faq.id);

  var cells = $row.find("td");
  $(cells[1]).find(".course-faq-order").text(faNumber(faq.displayOrder));
  $(cells[2]).append($('<span class="text-gray-800 fw-bold">').text(faq.title));

  var $details = $(
    '<button type="button" class="btn btn-sm btn-icon btn-bg-light btn-active-color-info me-1" title="جزئیات">',
  ).append('<i class="ki-duotone ki-eye fs-2"><span class="path1"></span><span class="path2"></span></i>');
  $details.on("click", function () {
    openDetailsModal(faq);
  });

  var $edit = $(
    '<button type="button" class="btn btn-sm btn-icon btn-bg-light btn-active-color-primary me-1" title="ویرایش">',
  ).append('<i class="ki-duotone ki-pencil fs-2"><span class="path1"></span><span class="path2"></span></i>');
  $edit.on("click", function () {
    openEditModal(faq);
  });

  var $delete = $(
    '<button type="button" class="btn btn-sm btn-icon btn-bg-light btn-active-color-danger" title="حذف">',
  ).append(
    '<i class="ki-duotone ki-trash fs-2"><span class="path1"></span><span class="path2"></span><span class="path3"></span><span class="path4"></span><span class="path5"></span></i>',
  );
  $delete.on("click", function () {
    if (page) page.openDelete(faq.id, faq.title);
  });

  $(cells[3]).append($details, $edit, $delete);
  return $row[0];
}

function loadRows() {
  return $.getJSON(
    location.pathname + "?handler=Rows&courseId=" + courseId(),
  ).then(function (rows) {
    var nodes = [];
    for (var i = 0; i < rows.length; i++) nodes.push(buildFaqRow(rows[i]));
    return nodes;
  });
}

function refreshRows() {
  return loadRows().then(function (rows) {
    $("#kt_course_faqs_table tbody").empty().append(rows);
    page.table.clear();
    page.table.rows.add(rows).draw();
    reorder.renumber();
  });
}
//#endregion

//#region CRUD Page Configuration
function initPage() {
  page = AdminCrud.createPage({
    table: {
      selector: "#kt_course_faqs_table",
      options: $.extend(true, AdminCrud.rowReorderOptions(), {
        columnDefs: [
          { orderable: false, targets: [0, 1, 3] },
          { className: "all", targets: 1 },
          { className: "min-tablet", targets: [3] },
          { className: "dtr-control", targets: 0 },
        ],
      }),
    },
    search: "#courseFaqSearch",
    modal: {
      id: "kt_modal_course_faq",
      titleEl: "#modal-course-faq-title",
      submitBtn: "kt_modal_course_faq_submit",
    },
    deleteModal: {
      id: "kt_modal_delete_course_faq",
      nameEl: "#delete-course-faq-name",
    },
    texts: {
      createTitle: "افزودن سوال متداول",
      editTitle: "ویرایش سوال متداول",
      created: "سوال متداول با موفقیت ایجاد شد.",
      updated: "تغییرات با موفقیت ذخیره شد.",
      deleted: "سوال متداول با موفقیت حذف شد.",
    },

    fillForm: function (faq) {
      faq = faq || { title: "", description: "", displayOrder: 0 };
      $("#Form_Title").val(faq.title);
      $("#Form_Description").val(faq.description);
      $("#Form_DisplayOrder").val(
        faq.displayOrder > 0 ? faq.displayOrder : reorder.getNextOrder(),
      );
    },

    getData: function () {
      return {
        title: $("#Form_Title").val().trim(),
        description: $("#Form_Description").val().trim(),
        displayOrder: parseInt($("#Form_DisplayOrder").val(), 10) || 0,
      };
    },

    validate: function () {
      var errors = [];
      var hasTitle = $("#Form_Title").val().trim().length > 0;
      var hasDescription = $("#Form_Description").val().trim().length > 0;

      if (!hasTitle) {
        errors.push("عنوان سوال الزامی است.");
        $("#Form_Title").addClass("is-invalid");
      } else {
        $("#Form_Title").removeClass("is-invalid");
      }
      if (!hasDescription) {
        errors.push("پاسخ (توضیحات) الزامی است.");
        $("#Form_Description").addClass("is-invalid");
      } else {
        $("#Form_Description").removeClass("is-invalid");
      }
      return errors;
    },

    onSubmit: saveFaq,
    onDelete: deleteFaq,
  });

  reorder = AdminCrud.bindRowReorder(page.table, {
    badge: ".course-faq-order",
    orderCol: 1,
    logLabel: "Reorder course FAQs",
    successMsg: "ترتیب سوالات متداول با موفقیت ذخیره شد.",
    onSave: saveOrder,
  });
}
//#endregion

//#region Details / Create / Edit Handlers
function openDetailsModal(faq) {
  $("#details-course-faq-title").text(faq.title);
  $("#details-course-faq-description").text(faq.description);
  AdminCrud.Utils.showModal("kt_modal_course_faq_details");
}

function openCreateModal() {
  if (page) page.openCreate();
}

function submitCourseFaqForm() {
  if (page) page.submit();
}

function confirmDelete() {
  if (page) page.confirmDelete();
}

function openDeleteModal(id, name) {
  if (page) page.openDelete(id, name);
}

/** Edits a FAQ from the row data; the closure keeps user text out of inline handlers. */
function openEditModal(faq) {
  if (!page) return;
  page.state.mode = "edit";
  page.state.editId = faq.id;
  $("#modal-course-faq-title").text("ویرایش سوال متداول");
  $("#Form_Title").val(faq.title);
  $("#Form_Description").val(faq.description);
  $("#Form_DisplayOrder").val(
    reorder.orderOf(faq.id, faq.displayOrder),
  );
  AdminCrud.Utils.showModal("kt_modal_course_faq");
}
//#endregion

//#region Boot
$(function () {
  loadRows()
    .then(function (rows) {
      $("#kt_course_faqs_table tbody").append(rows);
      initPage();
      reorder.renumber();
    })
    .fail(function () {
      initPage();
      toastr.error("خطا در دریافت سوالات.", "خطا");
    });
});
//#endregion
