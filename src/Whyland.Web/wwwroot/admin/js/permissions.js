//#region Permissions Page Scripts
/**
 * Admin "Permissions" list page (sections 6.1 / 6.2 of the UI/UX doc):
 * table of dynamic Permission entities, create/edit in a modal, group
 * filter by Key prefix, delete confirmation with protection for system
 * (IsSystem) permissions.
 * Built on top of the shared AdminCrud module (admin-crud.js).
 */

//#region Form Helpers
/**
 * Populates the form fields from a Permission object (null = create mode).
 * Declared at module level so both the AdminCrud config and the inline
 * onclick edit handler share the same logic.
 */
function fillPermissionForm(permission) {
  permission = permission || { name: "", key: "", isSystem: false };
  $("#Form_Name").val(permission.name);
  $("#Form_Key").val(permission.key);

  // IsSystem is read-only while editing (6.2); a switch in create mode.
  if (permission.isSystem) {
    $("#PermissionIsSystemEdit").removeClass("d-none");
    $("#PermissionIsSystemCreate").addClass("d-none");
  } else {
    $("#PermissionIsSystemEdit").addClass("d-none");
    $("#PermissionIsSystemCreate").removeClass("d-none");
    $("#Form_IsSystem").prop("checked", false);
  }
}
//#endregion

//#region CRUD Page Configuration
var page = AdminCrud.createPage({
  table: {
    selector: "#kt_permissions_table",
    options: {
      columnDefs: [{ orderable: false, targets: [-1] }],
    },
  },
  search: "#permissionSearch",
  filters: [
    // Group filter: matches the Key prefix (e.g. "Blog.Publish" -> "Blog").
    {
      el: "#permissionGroupFilter",
      test: function ($row, val) {
        var key = $row.find('td [dir="ltr"]').first().text().trim();
        return key.split(".")[0] === val;
      },
    },
    {
      el: "#permissionTypeFilter",
      test: AdminCrud.Filters.dataEquals("type"),
    },
  ],
  modal: {
    id: "kt_modal_permission",
    titleEl: "#modal-permission-title",
    submitBtn: "kt_permission_submit",
  },
  deleteModal: {
    id: "kt_modal_delete_permission",
    nameEl: "#delete-permission-name",
  },
  texts: {
    createTitle: "افزودن دسترسی جدید",
    editTitle: "ویرایش دسترسی",
    created: "دسترسی با موفقیت ایجاد شد.",
    updated: "تغییرات با موفقیت ذخیره شد.",
    deleted: "دسترسی با موفقیت حذف شد.",
  },

  // Populate the form fields from a Permission object (null = create mode).
  fillForm: fillPermissionForm,

  // Collect the form into the Permission data object sent to the server.
  getData: function () {
    return {
      name: $("#Form_Name").val().trim(),
      key: $("#Form_Key").val().trim(),
      // getData runs in create mode too; in edit mode IsSystem is fixed
      // (read-only field, see 6.2) and taken from the row being edited.
      isSystem: Boolean(editState.isSystem),
    };
  },

  validate: function () {
    var errors = [];
    var $name = $("#Form_Name");
    var $key = $("#Form_Key");
    var name = $name.val().trim();
    var key = $key.val().trim();

    if (!name) {
      errors.push("نام دسترسی الزامی است.");
      $name.addClass("is-invalid");
    } else {
      $name.removeClass("is-invalid");
    }
    // Key: unique, Group.Action format, no spaces (6.2).
    if (!key) {
      errors.push("کلید (Key) الزامی است.");
      $key.addClass("is-invalid");
    } else if (/\s/.test(key)) {
      errors.push("کلید (Key) نباید شامل فاصله باشد.");
      $key.addClass("is-invalid");
    } else if (!/^[A-Za-z][A-Za-z0-9]*\.[A-Za-z][A-Za-z0-9]*$/.test(key)) {
      errors.push("قالب کلید باید Group.Action باشد؛ مثال: Blog.Publish");
      $key.addClass("is-invalid");
    } else {
      $key.removeClass("is-invalid");
    }
    return errors;
  },
});

// The PageModel keeps state private to createPage; capture it for helpers.
var state = page.state;
var editState = { isSystem: false }; // IsSystem of the row being edited
//#endregion

//#region Inline onclick Handlers (HTML compatibility)
function openCreateModal() {
  editState.isSystem = false;
  page.openCreate();
}
function submitPermissionForm() {
  page.submit();
}
function openDeleteModal(id, name, isSystem) {
  if (isSystem) {
    // Protection for system permissions: never open the delete modal.
    toastr.warning("دسترسی‌های سیستمی قابل حذف نیستند.", "هشدار");
    return;
  }
  page.openDelete(id, name);
}
function confirmDelete() {
  page.confirmDelete();
}
//#endregion

//#region Edit From Row Data
/**
 * Edit: keeps the inline onclick signature (id, name, key, isSystem);
 * the values are passed in directly from the table row.
 */
function openEditModal(id, name, key, isSystem) {
  state.mode = "edit";
  state.editId = id;
  editState.isSystem = Boolean(isSystem);
  $("#modal-permission-title").text("ویرایش دسترسی");
  fillPermissionForm({ name: name, key: key, isSystem: Boolean(isSystem) });
  AdminCrud.Utils.showModal("kt_modal_permission");
}
//#endregion
//#endregion
