//#region Roles Page Scripts
/**
 * Admin "Roles" list page (sections 5.1 / 5.2 of the UI/UX doc):
 * table of dynamic Role entities, create/edit in a modal, delete
 * confirmation with protection for system (IsSystem) roles.
 * Built on top of the shared AdminCrud module (admin-crud.js).
 */

//#region Form Helpers
/**
 * Populates the form fields from a Role object (null = create mode).
 * Declared at module level so both the AdminCrud config and the
 * inline onclick edit handler share the same logic.
 */
function fillRoleForm(role) {
  role = role || { title: "", name: "", isSystem: false };
  $("#Form_Title").val(role.title);
  $("#Form_Name").val(role.name);

  // IsSystem is read-only while editing (5.2); a switch in create mode.
  if (role.isSystem) {
    $("#RoleIsSystemEdit").removeClass("d-none");
    $("#RoleIsSystemCreate").addClass("d-none");
  } else {
    $("#RoleIsSystemEdit").addClass("d-none");
    $("#RoleIsSystemCreate").removeClass("d-none");
    $("#Form_IsSystem").prop("checked", false);
  }
}
//#endregion

//#region CRUD Page Configuration
var page = AdminCrud.createPage({
  table: {
    selector: "#kt_roles_table",
    options: {
      columnDefs: [{ orderable: false, targets: [-1] }],
    },
  },
  search: "#roleSearch",
  filters: [
    {
      el: "#roleTypeFilter",
      test: AdminCrud.Filters.dataEquals("type"),
    },
  ],
  modal: {
    id: "kt_modal_role",
    titleEl: "#modal-role-title",
    submitBtn: "kt_role_submit",
  },
  deleteModal: {
    id: "kt_modal_delete_role",
    nameEl: "#delete-role-name",
  },
  texts: {
    createTitle: "افزودن نقش جدید",
    editTitle: "ویرایش نقش",
    created: "نقش با موفقیت ایجاد شد.",
    updated: "تغییرات با موفقیت ذخیره شد.",
    deleted: "نقش با موفقیت حذف شد.",
  },

  // Populate the form fields from a Role object (null = create mode).
  fillForm: fillRoleForm,

  // Collect the form into the Role data object sent to the server.
  getData: function () {
    return {
      title: $("#Form_Title").val().trim(),
      name: $("#Form_Name").val().trim(),
      // getData runs in create mode too; in edit mode IsSystem is fixed
      // (read-only field, see 5.2) and taken from the row being edited.
      isSystem: Boolean(editState.isSystem),
    };
  },

  validate: function () {
    var errors = [];
    var $title = $("#Form_Title");
    var $name = $("#Form_Name");
    var title = $title.val().trim();
    var name = $name.val().trim();

    if (!title) {
      errors.push("عنوان نقش الزامی است.");
      $title.addClass("is-invalid");
    } else {
      $title.removeClass("is-invalid");
    }
    if (!name) {
      errors.push("نام (Name) نقش الزامی است.");
      $name.addClass("is-invalid");
    } else if (/\s/.test(name)) {
      errors.push("نام (Name) نباید شامل فاصله باشد.");
      $name.addClass("is-invalid");
    } else {
      $name.removeClass("is-invalid");
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
function submitRoleForm() {
  page.submit();
}
function openDeleteModal(id, name, isSystem) {
  if (isSystem) {
    // Protection for system roles (5.4): never open the delete modal.
    toastr.warning("نقش‌های سیستمی قابل حذف نیستند.", "هشدار");
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
 * Edit: keeps the inline onclick signature (id, title, name, isSystem);
 * the values are passed in directly from the table row.
 */
function openEditModal(id, title, name, isSystem) {
  state.mode = "edit";
  state.editId = id;
  editState.isSystem = Boolean(isSystem);
  $("#modal-role-title").text("ویرایش نقش");
  fillRoleForm({ title: title, name: name, isSystem: Boolean(isSystem) });
  AdminCrud.Utils.showModal("kt_modal_role");
}
//#endregion

//#region Role Permissions (section 5.3)
/**
 * Permission matrix editor for one role. The permission catalog is
 * server-rendered in _RoleModals.cshtml ([data-perm] checkboxes, grouped
 * under [data-group] masters); only the *assignment* state is handled
 * here, in demo mode. TODO: replace demoPermissionSets / the simulated
 * save with GET ?handler=Permissions&id=... and a POST back-end once the
 * data layer is wired up.
 */

// The permission modal's live state (not shared with the CRUD controller).
var permissionsState = { roleId: null, roleName: "" };

// Demo assignment sets per role id (mirror of the demo rows in the table).
var demoPermissionSets = {};

/** Assignment counts shown in the «دسترسی‌ها» column (demo). */
function refreshPermissionBadges() {
  $(".role-permission-badge").each(function () {
    var roleId = $(this).data("role-id");
    var count = (demoPermissionSets[roleId] || []).length;
    $(this).text(faDigits(count) + " دسترسی");
  });
}

/** Digits in the demo strings are Persian; render counts the same way. */
function faDigits(n) {
  var fa = "۰۱۲۳۴۵۶۷۸۹";
  return String(n).replace(/\d/g, function (d) {
    return fa[+d];
  });
}

//#region Group master toggles + indeterminate state
function setGroupMasters() {
  $(".perm-group").each(function () {
    var group = $(this).data("group");
    var $items = $('.perm-item[data-group="' + group + '"]');
    var total = $items.length;
    var checked = $items.filter(":checked").length;

    // Master display convention (per spec):
    //  - checkmark (checked) as soon as ANY child is selected = "click to select all"
    //  - dash (indeterminate) only when ALL children are selected = "click to deselect all"
    //  - empty box when nothing is selected
    this.checked = checked > 0;
    this.indeterminate = total > 0 && checked === total;
  });
}

// Master checkbox click: when the whole group is already selected it
// clears every child (the dash means "deselect all"); in any other state
// it selects all children (the checkmark means "select all").
// The decision is taken from the CHILDREN's state, not from the master's
// own post-click value — browsers flip an indeterminate box to checked,
// which would break the "deselect all" click without this.
$(document).on("change", ".perm-group", function () {
  var group = $(this).data("group");
  var $items = $('.perm-item[data-group="' + group + '"]');
  var allChecked = $items.length > 0 && $items.filter(":checked").length === $items.length;
  $items
    .prop("checked", !allChecked)
    .trigger("change");
});

// Any child change refreshes every master (indeterminate included).
$(document).on("change", ".perm-item", function () {
  setGroupMasters();
});
//#endregion

//#region Open / Save
/**
 * Opens the permissions modal for a role. Kept inline-onclick friendly:
 * openPermissionsModal(id, name[, permKeysArray]).
 */
function openPermissionsModal(id, name, permKeys) {
  permissionsState.roleId = id;
  permissionsState.roleName = name;
  $("#permissions-role-name").text(name);

  // resolve current assignment state (demo)
  if (!permKeys && !demoPermissionSets[id]) {
    var badge = $('.role-permission-badge[data-role-id="' + id + '"]');
    // For demo rows whose count came from markup, seed from the digits.
    var m = (badge.text() || "").match(/\d|[\u06F0-\u06F9]/g);
    demoPermissionSets[id] = seedDemoSet(id, m ? countDigits(m) : 0);
  }

  var assigned = permKeys || demoPermissionSets[id] || [];
  demoPermissionSets[id] = assigned.slice();

  $(".perm-item").each(function () {
    this.checked = assigned.indexOf($(this).data("perm")) !== -1;
  });
  setGroupMasters();

  AdminCrud.Utils.showModal("kt_modal_role_permissions");
}

/** Collects the checked permission keys into an array. */
function collectPermissions() {
  var out = [];
  $(".perm-item:checked").each(function () {
    out.push($(this).data("perm"));
  });
  return out;
}

/** Persists the assignment (demo: simulated save). */
function saveRolePermissions() {
  var btn = document.getElementById("kt_role_permissions_submit");
  btn.setAttribute("data-kt-indicator", "on");
  btn.disabled = true;

  var assigned = collectPermissions();
  demoPermissionSets[permissionsState.roleId] = assigned;

  // Demo: simulate a round-trip; TODO POST to ?handler=SavePermissions
  setTimeout(function () {
    btn.removeAttribute("data-kt-indicator");
    btn.disabled = false;
    AdminCrud.Utils.hideModal("kt_modal_role_permissions");
    toastr.success("دسترسی‌های نقش با موفقیت ذخیره شد.", "موفق");
    refreshPermissionBadges();
  }, 700);
}
//#endregion

//#region Demo seeding helpers
/** Latin digits (incl. Persian ones) to a count. */
function countDigits(chars) {
  var fa = "۰۱۲۳۴۵۶۷۸۹";
  return chars.reduce(function (sum, c) {
    var idx = fa.indexOf(c);
    return sum + (idx >= 0 ? idx : /^[\u06F0-\u06F9]$/.test(c) ? 0 : parseInt(c, 10) || 0);
  }, 0);
}

/** Builds a deterministic demo key set of exactly `n` permissions. */
function seedDemoSet(roleId, n) {
  var all = [];
  $(".perm-item").each(function () {
    all.push($(this).data("perm"));
  });
  // Stable pseudo-random offset from the role id so each role gets a
  // different, deterministic subset.
  var offset = 0;
  for (var i = 0; i < roleId.length; i++) offset = (offset + roleId.charCodeAt(i)) % 997;
  var set = [];
  for (var k = 0; k < n && all.length; k++) {
    set.push(all[(offset + k * 3) % all.length]);
  }
  return Array.from(new Set(set));
}
//#endregion
//#endregion
//#endregion
