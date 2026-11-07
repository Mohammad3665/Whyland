//#region Course Sections Page Scripts
/**
 * Admin "Course Sections" page: the sections of one course, ordered by DisplayOrder and managed
 * from the course page. Rows, saves, deletes and drag & drop ordering all go through the Razor
 * Page handlers and the shared AdminCrud module.
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

/**
 * Sends a form-encoded POST and adapts the flat {isSuccess, message} envelope to the AdminCrud
 * contract, so the shared module can show the server's validation message.
 */
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

function saveSection(data, mode) {
  if (mode === "create") data.courseId = courseId();
  return postHandler(mode === "create" ? "Create" : "Update", data).done(refreshRows);
}

function deleteSection(id) {
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
        toastr.success("ترتیب سرفصل‌ها با موفقیت ذخیره شد.", "موفق");
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

function buildSectionRow(section) {
  var $row = $(
    '<tr><td class="dtr-control-col"></td>' +
      '<td tabindex="0" class="reorder text-center dt-type-numeric dtr-control">' +
      '<div class="d-flex align-items-center justify-content-center">' +
      '<i class="ki-duotone ki-burger-menu-2 fs-2 text-gray-400 cursor-move me-2">' +
      '<span class="path1"></span><span class="path2"></span><span class="path3"></span>' +
      '<span class="path4"></span><span class="path5"></span><span class="path6"></span>' +
      '<span class="path7"></span><span class="path8"></span><span class="path9"></span><span class="path10"></span>' +
      "</i>" +
      '<span class="section-order badge badge-light-secondary"></span>' +
      "</div></td>" +
      "<td></td>" +
      '<td><span class="badge badge-light-primary"></span></td>' +
      '<td class="text-end nowrap-cell"></td>' +
      "</tr>",
  );

  $row.attr("data-id", section.id);

  var cells = $row.find("td");
  $(cells[1]).find(".section-order").text(faNumber(section.displayOrder));
  $(cells[2]).append($('<span class="text-gray-800 fw-bold">').text(section.title));
  $(cells[3]).append(
    $('<span class="badge badge-light-primary">').text(
      faNumber(section.episodeCount) + " اپیزود",
    ),
  );

  var $edit = $(
    '<button type="button" class="btn btn-sm btn-icon btn-bg-light btn-active-color-primary me-1" title="ویرایش">',
  ).append('<i class="ki-duotone ki-pencil fs-2"><span class="path1"></span><span class="path2"></span></i>');
  $edit.on("click", function () {
    openEditModal(section);
  });

  var $delete = $(
    '<button type="button" class="btn btn-sm btn-icon btn-bg-light btn-active-color-danger" title="حذف">',
  ).append(
    '<i class="ki-duotone ki-trash fs-2"><span class="path1"></span><span class="path2"></span><span class="path3"></span><span class="path4"></span><span class="path5"></span></i>',
  );
  $delete.on("click", function () {
    if (page) page.openDelete(section.id, section.title);
  });

  $(cells[4]).append($edit, $delete);
  return $row[0];
}

function loadRows() {
  return $.getJSON(
    location.pathname + "?handler=Rows&courseId=" + courseId(),
  ).then(function (rows) {
    var nodes = [];
    for (var i = 0; i < rows.length; i++) nodes.push(buildSectionRow(rows[i]));
    return nodes;
  });
}

function refreshRows() {
  return loadRows().then(function (rows) {
    $("#kt_course_sections_table tbody").empty().append(rows);
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
      selector: "#kt_course_sections_table",
      options: $.extend(true, AdminCrud.rowReorderOptions(), {
        columnDefs: [
          { orderable: false, targets: [0, 1, 4] },
          { className: "all", targets: [1, 2] },
          { className: "min-tablet", targets: [3, 4] },
          { className: "dtr-control", targets: 0 },
        ],
      }),
    },
    search: "#sectionSearch",
    modal: {
      id: "kt_modal_section",
      titleEl: "#modal-section-title",
      submitBtn: "kt_modal_section_submit",
    },
    deleteModal: {
      id: "kt_modal_delete_section",
      nameEl: "#delete-section-name",
    },
    texts: {
      createTitle: "افزودن سرفصل",
      editTitle: "ویرایش سرفصل",
      created: "سرفصل با موفقیت ایجاد شد.",
      updated: "تغییرات با موفقیت ذخیره شد.",
      deleted: "سرفصل با موفقیت حذف شد.",
    },

    fillForm: function (section) {
      $("#Form_Title").val(section ? section.title : "");
      $("#Form_DisplayOrder").val(
        section ? section.displayOrder : reorder.getNextOrder(),
      );
    },

    getData: function () {
      return {
        title: $("#Form_Title").val().trim(),
        displayOrder: parseInt($("#Form_DisplayOrder").val(), 10) || 0,
      };
    },

    validate: function () {
      var errors = [];
      var hasTitle = $("#Form_Title").val().trim().length > 0;

      if (!hasTitle) {
        errors.push("عنوان سرفصل الزامی است.");
        $("#Form_Title").addClass("is-invalid");
      } else {
        $("#Form_Title").removeClass("is-invalid");
      }
      return errors;
    },

    onSubmit: saveSection,
    onDelete: deleteSection,
  });

  reorder = AdminCrud.bindRowReorder(page.table, {
    badge: ".section-order",
    orderCol: 1,
    logLabel: "Reorder course sections",
    successMsg: "ترتیب سرفصل‌ها با موفقیت ذخیره شد.",
    onSave: saveOrder,
  });
}
//#endregion

//#region Inline onclick Handlers
function openCreateModal() {
  if (page) page.openCreate();
}

function submitSectionForm() {
  if (page) page.submit();
}

function confirmDelete() {
  if (page) page.confirmDelete();
}

function openDeleteModal(id, name) {
  if (page) page.openDelete(id, name);
}

/** Edits a section from the row data; the closure keeps user text out of inline handlers. */
function openEditModal(section) {
  if (!page) return;
  page.state.mode = "edit";
  page.state.editId = section.id;
  $("#modal-section-title").text("ویرایش سرفصل");
  $("#Form_Title").val(section.title);
  $("#Form_DisplayOrder").val(
    reorder.orderOf(section.id, section.displayOrder),
  );
  AdminCrud.Utils.showModal("kt_modal_section");
}
//#endregion

//#region Boot
$(function () {
  loadRows()
    .then(function (rows) {
      $("#kt_course_sections_table tbody").append(rows);
      initPage();
      reorder.renumber();
    })
    .fail(function () {
      initPage();
      toastr.error("خطا در دریافت سرفصل‌ها.", "خطا");
    });
});
//#endregion
