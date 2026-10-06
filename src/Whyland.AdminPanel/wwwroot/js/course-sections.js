//#region Course Sections Page Scripts
/**
 * Admin "Course Sections" list page (section 3.3 of the UI/UX doc):
 * sections table with drag & drop reordering (DataTables RowReorder),
 * create/edit modal and delete confirmation.
 * Built on top of the shared AdminCrud module (admin-crud.js).
 */

var reorder; // shared drag & drop ordering controller (set below)

//#region CRUD Page Configuration
var page = AdminCrud.createPage({
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

  // Populate the form fields from a section object (null = create mode).
  fillForm: function (s) {
    $("#Form_Title").val(s ? s.title : "");
    $("#Form_DisplayOrder").val(s ? s.displayOrder : reorder.getNextOrder());
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

//#region Drag & Drop Reordering
// Renumbers the «ترتیب» badges and persists the new order after a drop.
// TODO: pass onSave here (POST [{ id, displayOrder }] to ?handler=Reorder)
// once the data layer is connected.
reorder = AdminCrud.bindRowReorder(page.table, {
  badge: ".section-order",
  orderCol: 1,
  logLabel: "Reorder sections",
  successMsg: "ترتیب سرفصل‌ها با موفقیت ذخیره شد.",
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
