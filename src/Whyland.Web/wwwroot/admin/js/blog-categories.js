//#region Blog Categories Page Scripts
/**
 * Admin "Blog Categories" list page (section 37.1 of the SRS):
 * hierarchical blog category table with slug uniqueness, create/edit
 * modal, delete confirmation (soft delete / in-use guard) and the
 * standard search + status filter.
 * Built on top of the shared AdminCrud module (admin-crud.js).
 */

//#region CRUD Page Configuration
var page = AdminCrud.createPage({
  table: { selector: "#kt_blog_categories_table" },
  search: "#blogCategorySearch",
  filters: [
    { el: "#statusFilter", test: AdminCrud.Filters.dataEquals("active") },
  ],
  modal: {
    id: "kt_modal_blog_category",
    titleEl: "#modal-blog-category-title",
    submitBtn: "kt_modal_blog_category_submit",
  },
  deleteModal: {
    id: "kt_modal_delete_blog_category",
    nameEl: "#delete-blog-category-name",
  },
  texts: {
    createTitle: "افزودن دسته‌بندی بلاگ",
    editTitle: "ویرایش دسته‌بندی بلاگ",
    created: "دسته‌بندی بلاگ با موفقیت ایجاد شد.",
    updated: "تغییرات با موفقیت ذخیره شد.",
    deleted: "دسته‌بندی بلاگ با موفقیت حذف شد.",
  },

  // Populate the form fields from a category object (null = create mode).
  fillForm: function (c) {
    c = c || {
      name: "",
      latinName: "",
      slug: "",
      displayOrder: 0,
      parentId: "",
      isActive: true,
    };
    $("#Form_Name").val(c.name);
    $("#Form_LatinName").val(c.latinName);
    $("#Form_Slug").val(c.slug);
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
      slug: $("#Form_Slug").val().trim(),
      displayOrder: parseInt($("#Form_DisplayOrder").val(), 10) || 0,
      parentId: $("#Form_ParentId").val() || null,
      isActive: $("#Form_IsActive").is(":checked"),
    };
  },

  // SRS 37.1 validation: required fields, unique slug (case-insensitive)
  // and no self-parenting.
  validate: function () {
    var errors = [];
    var data = this.getData();
    // getData() carries no id — the edit id lives in the controller state
    // (AdminCrud.submit assigns data.id only after validate() runs).
    var editId = page.state.editId ? Number(page.state.editId) : null;

    function require(selector, field) {
      var $el = $(selector);
      var ok = $el.val() && $el.val().trim().length > 0;
      AdminCrud.Utils.setInvalid($el, !ok);
      return ok;
    }

    if (!require("#Form_Name", "name")) {
      errors.push("نام دسته‌بندی الزامی است.");
    }
    if (!require("#Form_LatinName", "latinName")) {
      errors.push("نام لاتین الزامی است.");
    }
    if (!require("#Form_Slug", "slug")) {
      errors.push("اسلاگ (Slug) الزامی است.");
    } else if (
      // Uniqueness check over the rendered demo rows (the real check
      // happens on the server against the unique index).
      $("#kt_blog_categories_table tbody tr:not(.filter-hide) td code").filter(
        function () {
          var rowId = Number($(this).closest("tr").data("id")) || null;
          var slugThis = $(this).text().trim().toLowerCase();
          return slugThis === data.slug.toLowerCase() && rowId !== editId;
        },
      ).length > 0
    ) {
      $("#Form_Slug").addClass("is-invalid");
      errors.push("این اسلاگ قبلاً استفاده شده است.");
    }
    if (data.parentId && editId && Number(data.parentId) === editId) {
      // Self-parent guard only matters while editing (SRS 37.1).
      $("#Form_ParentId").addClass("is-invalid");
      errors.push("یک دسته‌بندی نمی‌تواند والد خودش باشد.");
    }
    return errors;
  },
});
//#endregion

//#region Inline onclick Handlers (HTML compatibility)
// The markup calls these directly via onclick="..." attributes.
function openCreateModal() {
  page.openCreate();
}
function submitBlogCategoryForm() {
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
 * Edit: keeps the inline onclick signature
 * (id, name, latinName, slug, displayOrder, parentId, isActive);
 * the values are passed in directly from the table row.
 */
function openEditModal(
  id,
  name,
  latinName,
  slug,
  displayOrder,
  parentId,
  isActive,
) {
  page.state.mode = "edit";
  page.state.editId = id;
  $("#modal-blog-category-title").text("ویرایش دسته‌بندی بلاگ");
  $("#Form_Name").val(name);
  $("#Form_LatinName").val(latinName);
  $("#Form_Slug").val(slug);
  $("#Form_DisplayOrder").val(displayOrder);
  $("#Form_ParentId")
    .val(parentId ?? "")
    .trigger("change");
  $("#Form_IsActive").prop("checked", isActive);
  AdminCrud.Utils.showModal("kt_modal_blog_category");
}

$("#Form_ParentId").select2({
  dropdownParent: $("#kt_modal_blog_category"),
  placeholder: "بدون والد (سطح اول)",
  allowClear: true,
  width: "100%",
  dropdownAutoWidth: false,
  dropdownPosition: "below",
  dir: "rtl",
});
//#endregion
//#endregion
