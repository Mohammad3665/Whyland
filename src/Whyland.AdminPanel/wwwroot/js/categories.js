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

// برای حفظ سازگاری با onclick های موجود در HTML
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

// ویرایش: امضای قبلی حفظ شده؛ داده‌ها از پارامترها می‌آیند
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
