//#region Categories Page Scripts
/**
 * Admin "Categories" list page: hierarchical category table with
 * create/edit modal and delete confirmation.
 * Built on top of the shared AdminCrud module (admin-crud.js).
 */

//#region CRUD Page Configuration
var page = AdminCrud.createPage({
  table: { selector: "#kt_categories_table" },
  search: "#categorySearch",
  filters: [
    { el: "#statusFilter", test: AdminCrud.Filters.dataEquals("active") },
  ],
  modal: {
    id: "kt_modal_category",
    titleEl: "#modal-category-title",
    submitBtn: "kt_modal_category_submit",
  },
  deleteModal: {
    id: "kt_modal_delete_category",
    nameEl: "#delete-category-name",
  },
  texts: {
    createTitle: "افزودن دسته‌بندی",
    editTitle: "ویرایش دسته‌بندی",
    created: "دسته‌بندی با موفقیت ایجاد شد.",
    updated: "تغییرات با موفقیت ذخیره شد.",
    deleted: "دسته‌بندی با موفقیت حذف شد.",
  },

  // Populate the form fields from a category object (null = create mode).
  fillForm: function (c) {
    c = c || {
      name: "",
      latinName: "",
      displayOrder: 0,
      parentId: "",
      isActive: true,
    };
    $("#Form_Name").val(c.name);
    $("#Form_LatinName").val(c.latinName);
    $("#Form_DisplayOrder").val(c.displayOrder);
    $("#Form_ParentId")
      .val(c.parentId ?? "")
      .trigger("change");
    $("#Form_IsActive").prop("checked", c.isActive);
  },

  // Collect the form into the category data object sent to the server.
  getData: function () {
    return {
      name: $("#Form_Name").val().trim(),
      latinName: $("#Form_LatinName").val().trim(),
      displayOrder: $("#Form_DisplayOrder").val(),
      parentId: $("#Form_ParentId").val() || null,
      isActive: $("#Form_IsActive").is(":checked"),
    };
  },
});
//#endregion

//#region Inline onclick Handlers (HTML compatibility)
// The markup calls these directly via onclick="..." attributes.

function openCreateModal() {
  page.openCreate();
}
function submitCategoryForm() {
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
 * Edit: keeps the old inline onclick signature
 * (id, name, latinName, displayOrder, parentId, isActive);
 * the values are passed in directly from the table row.
 */
function openEditModal(id, name, latinName, displayOrder, parentId, isActive) {
  page.state.mode = "edit";
  page.state.editId = id;
  $("#modal-category-title").text("ویرایش دسته‌بندی");
  $("#Form_Name").val(name);
  $("#Form_LatinName").val(latinName);
  $("#Form_DisplayOrder").val(displayOrder);
  $("#Form_ParentId")
    .val(parentId ?? "")
    .trigger("change");
  $("#Form_IsActive").prop("checked", isActive);
  AdminCrud.Utils.showModal("kt_modal_category");
}
//#endregion
//#endregion
