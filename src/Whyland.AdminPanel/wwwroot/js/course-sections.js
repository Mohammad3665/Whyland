//#region Course Sections Page Scripts
/**
 * Admin "Course Sections" list page (section 3.3 of the UI/UX doc):
 * sections table with drag & drop reordering (DataTables RowReorder),
 * create/edit modal and delete confirmation.
 * Built on top of the shared AdminCrud module (admin-crud.js).
 */

//#region CRUD Page Configuration
var page = AdminCrud.createPage({
  table: {
    selector: "#kt_course_sections_table",
    options: {
      order: [], // keep the DOM (DisplayOrder) sequence
      columnDefs: [
        { orderable: false, targets: [0, 1, 4] },

        // سه ستون اول → همیشه نمایش
        { className: "all", targets: [0, 1, 2] },

        // دو ستون آخر → فقط در موبایل مخفی شوند (در تبلت و دسکتاپ نمایش)
        { className: "min-tablet", targets: [3, 4] },
      ],
      rowReorder: {
        selector: "td.reorder", // drag handle cell
        dataSrc: 0,
        update: false, // order cells are rewritten in the row-reordered handler
      },
    },
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

  // Populate the form fields from a section object (null = create mode).
  fillForm: function (s) {
    $("#Form_Title").val(s ? s.title : "");
    $("#Form_DisplayOrder").val(s ? s.displayOrder : getNextOrder());
  },

  // Collect the form into the section data object sent to the server.
  getData: function () {
    return {
      title: $("#Form_Title").val().trim(),
      displayOrder: parseInt($("#Form_DisplayOrder").val(), 10) || 0,
    };
  },
});
//#endregion

//#region Order Helpers
/** Returns 1 + the highest order number currently visible in the table. */
function getNextOrder() {
  var max = 0;
  page.table
    .cells(0, ":eq(0)", { page: "current" })
    .nodes()
    .to$()
    .find(".section-order")
    .each(function () {
      max = Math.max(max, parseInt($(this).text(), 10) || 0);
    });
  return max + 1;
}

/** Rewrites the "ترتیب" badges 1..n according to the current row order. */
function renumberRows() {
  page.table.rows({ page: "current" }).every(function (rowIdx) {
    $(this.node())
      .find(".section-order")
      .text(rowIdx + 1);
  });
}

/**
 * Persists the new DisplayOrder values after a drag & drop.
 * TODO: wire up to a real endpoint (e.g. POST /CourseSections?handler=Reorder)
 * with [{ id, displayOrder }] once the data layer is connected.
 */
function saveOrder() {
  var rows = [];
  page.table.rows({ page: "current" }).every(function () {
    var $row = $(this.node());
    rows.push({
      id: $row.data("id"),
      displayOrder: parseInt($row.find(".section-order").text(), 10) || 0,
    });
  });
  console.log("Reorder sections:", rows);
  toastr.success("ترتیب سرفصل‌ها با موفقیت ذخیره شد.", "موفق");
}
//#endregion

//#region Drag & Drop Reordering
// After RowReorder drops a row, renumber the order column and persist.
page.table.on("row-reordered", function (e, diff, edit) {
  renumberRows();
  saveOrder();
});
//#endregion

//#region Inline onclick Handlers (HTML compatibility)
function openCreateModal() {
  page.openCreate();
}
function submitSectionForm() {
  page.submit();
}
function confirmDelete() {
  page.confirmDelete();
}
function openDeleteModal(id, name) {
  page.openDelete(id, name);
}
//#endregion

//#region Edit From Row Data
/**
 * Edit: keeps the inline onclick signature (id, title, displayOrder);
 * values are passed directly from the table row.
 */
function openEditModal(id, title, displayOrder) {
  page.state.mode = "edit";
  page.state.editId = id;
  $("#modal-section-title").text("ویرایش سرفصل");
  $("#Form_Title").val(title);
  $("#Form_DisplayOrder").val(displayOrder);
  AdminCrud.Utils.showModal("kt_modal_section");
}
//#endregion
//#endregion
