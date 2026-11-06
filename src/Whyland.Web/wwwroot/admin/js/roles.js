//#region Roles Page Scripts
/**
 * Admin "Roles" list (sections 5.1 – 5.3 of the UI/UX doc): the dynamic Role entities,
 * rendered by a server-side DataTable and wired to the Razor Page handlers:
 *   GET  ?handler=Rows           → one page of roles (paging, sorting and filters resolved server-side)
 *   GET  ?handler=Fields         → the filterable fields with the operators each accepts
 *   GET  ?handler=Granted        → the permission ids a role already holds
 *   POST ?handler=Create / Update / Delete / SavePermissions → form-encoded mutations
 *
 * The table plumbing comes from common/admin-table.js, exactly like the users page, so the two
 * filter boxes cannot drift apart. What is specific to a role is the protection rule: the roles the
 * seeder created and the code refers to by name (IsSystem) are shown but never edited — their
 * grants, their names and their existence belong to the seed row. The server refuses the same
 * writes whatever this page renders, so the rule here is only about not offering a button that
 * cannot work.
 */

var page; // shared CRUD controller (AdminCrud)
var picker; // the generic filter picker (AdminTable.filterPicker)
var roleCache = {}; // id → RoleListItemDto, so the edit form reads the same row the table shows

/**
 * Reads a row for the edit modal. The table already carries every editable field of a role
 * (title, name, isSystem), so no second round-trip is needed — and a role never has hidden
 * details the list does not show.
 */
function loadRole(id) {
  var dfd = $.Deferred();
  var role = roleCache[id];

  if (role) dfd.resolve(role);
  else dfd.reject({ responseJSON: { message: "نقش در جدول پیدا نشد." } });

  return dfd.promise();
}

//#region Server Connectivity
/** Saves the create/edit modal, then reloads the table keeping its page. */
function saveRole(data, mode) {
  return AdminTable.postHandler(
    mode === "create" ? "Create" : "Update",
    data,
  ).done(afterMutation);
}

/** Soft-deletes one role; the server also detaches it from its users and revokes its grants. */
function deleteRole(id) {
  return AdminTable.postHandler("Delete", { id: id }).done(afterMutation);
}

/** A mutation re-queries the table, which refills the row cache as the rows render. */
function afterMutation() {
  // Drop the cache first: a row can disappear from this page after an edit (renumbered, filtered
  // out), and the details modal reads straight from the cache, so a stale entry would linger.
  roleCache = {};
  if (page) page.table.ajax.reload(null, false);
}
//#endregion

//#region Row Rendering
/**
 * Fills one <tr>: the title with its system shield, the raw name, the type badge, the two counts
 * and the row actions. Handlers bind with closures and every value goes through .text(), so role
 * data never reaches an inline onclick or the HTML.
 */
function fillRoleRow(row, role) {
  roleCache[role.id] = role;
  var $cells = $("td", row);

  var $title = $cells.eq(1).addClass("align-middle");
  if (role.isSystem) {
    $title.html(
      "<div class='d-flex align-items-center'>" +
        "<i class='ki-duotone ki-shield-cross fs-2 text-primary me-2' title='نقش سیستمی'>" +
        "<span class='path1'></span><span class='path2'></span><span class='path3'></span>" +
        "<span class='path4'></span><span class='path5'></span><span class='path6'></span>" +
        "<span class='path7'></span><span class='path8'></span><span class='path9'></span><span class='path10'></span>" +
        "</i><span class='text-gray-800 fw-bold role-title'></span></div>",
    );
    $title.find(".role-title").text(role.title);
  } else {
    $title.append($("<span class='text-gray-800 fw-bold'>").text(role.title));
  }

  $cells.eq(2).append($("<span class='text-gray-600' dir='ltr'>").text(role.name));

  $cells
    .eq(3)
    .append(
      AdminTable.badge(
        role.isSystem ? "primary" : "secondary",
        role.isSystem ? "سیستمی" : "سفارشی",
      ),
    );

  // Only the number of accounts is shown: the list itself belongs to the users page, where a role
  // is one of the filters.
  $cells
    .eq(4)
    .append($("<span class='text-gray-800 fw-bold'>").text(AdminTable.faDigits(role.userCount)));

  var $grants = $(
    '<a href="#" class="badge badge-light-info role-permission-badge"></a>',
  ).text(AdminTable.faDigits(role.permissionCount) + " دسترسی");
  $grants.on("click", function (e) {
    e.preventDefault();
    openPermissionsModal(role.id);
  });
  $cells.eq(5).append($grants);

  $cells
    .eq(6)
    .append(
      $("<span class='text-gray-600 fs-7' dir='ltr'>").text(
        AdminTable.faDate(role.createdAt),
      ),
    );

  var $actions = $cells.eq(7).addClass("text-end nowrap-cell");

  // Details first: it is the only action every row offers, system roles included, because reading
  // a role is never a write the server would refuse.
  var $details = $(
    '<button type="button" class="btn btn-sm btn-icon btn-bg-light btn-active-color-primary me-1" title="جزئیات نقش">',
  ).append(
    '<i class="ki-duotone ki-eye fs-2"><span class="path1"></span><span class="path2"></span><span class="path3"></span></i>',
  );
  $details.on("click", function () {
    openRoleDetails(role.id);
  });
  $actions.append($details);

  var $permissions = $(
    '<button type="button" class="btn btn-sm btn-icon btn-bg-light btn-active-color-primary me-1" title="مدیریت دسترسی‌ها">',
  ).append(
    '<i class="ki-duotone ki-security-user fs-2"><span class="path1"></span><span class="path2"></span><span class="path3"></span><span class="path4"></span><span class="path5"></span></i>',
  );
  $permissions.on("click", function () {
    openPermissionsModal(role.id);
  });
  $actions.append($permissions);

  // A system role is not editable and never deletable, so the buttons that would do it are not
  // rendered at all rather than rendered and refused.
  if (!role.isSystem) {
    var $edit = $(
      '<button type="button" class="btn btn-sm btn-icon btn-bg-light btn-active-color-primary me-1" title="ویرایش">',
    ).append(
      '<i class="ki-duotone ki-pencil fs-2"><span class="path1"></span><span class="path2"></span></i>',
    );
    $edit.on("click", function () {
      if (page) page.openEdit(role.id);
    });

    var $delete = $(
      '<button type="button" class="btn btn-sm btn-icon btn-bg-light btn-active-color-danger" title="حذف">',
    ).append(
      '<i class="ki-duotone ki-trash fs-2"><span class="path1"></span><span class="path2"></span><span class="path3"></span><span class="path4"></span><span class="path5"></span></i>',
    );
    $delete.on("click", function () {
      page.openDelete(role.id, role.title);
    });

    $actions.append($edit, $delete);
  }
}
//#endregion

//#region Details Modal
var detailsCurrentId = null; // the role the open modal shows, for its own footer buttons

/** Writes one label of the details tables; an empty value shows a dash instead. */
function setRoleDetail(selector, value) {
  $(selector).text(value === null || value === undefined || value === "" ? "—" : value);
}

/**
 * Opens the modal with every field of the role, including the audit columns the table has no room
 * for. The listing already carries them all, so — like the category page — opening it is a lookup
 * in the cached row and costs no request.
 */
function openRoleDetails(id) {
  var role = roleCache[id];
  if (!role) return;

  detailsCurrentId = id;

  $("#rd-title").text(role.title);
  $("#rd-type").text(role.isSystem ? "نقش سیستمی" : "نقش سفارشی");

  setRoleDetail("#rd-id", AdminTable.faDigits(role.id));
  setRoleDetail("#rd-name", role.name);
  setRoleDetail("#rd-role-title", role.title);
  setRoleDetail(
    "#rd-kind",
    role.isSystem
      ? "سیستمی — ساخته و نگهداری‌شده توسط سید"
      : "سفارشی — قابل ویرایش و حذف",
  );

  setRoleDetail("#rd-users", AdminTable.faDigits(role.userCount) + " کاربر");
  setRoleDetail(
    "#rd-permissions",
    AdminTable.faDigits(role.permissionCount) + " دسترسی",
  );

  setRoleDetail("#rd-created", AdminTable.faDateTime(role.createdAt));
  // The name the row already carries, never the guid: an operator cannot read an author off a key.
  setRoleDetail("#rd-created-by", role.createdByName);
  setRoleDetail("#rd-updated", AdminTable.faDateTime(role.updatedAt));
  setRoleDetail("#rd-updated-by", role.updatedByName);

  // The edit button would only ever be refused by the commands, so it is not offered here either.
  $("#rd_edit_button").toggle(!role.isSystem);

  AdminCrud.Utils.showModal("kt_modal_role_details");
}

/** Closes the details modal and opens the same role in the edit form. */
function editRoleFromDetails() {
  if (!detailsCurrentId) return;
  AdminCrud.Utils.hideModal("kt_modal_role_details");
  if (page) page.openEdit(detailsCurrentId);
}

/** Closes the details modal and opens the permission tree of the same role. */
function permissionsFromDetails() {
  if (!detailsCurrentId) return;
  AdminCrud.Utils.hideModal("kt_modal_role_details");
  openPermissionsModal(detailsCurrentId);
}
//#endregion

//#region CRUD Page Configuration
function initPage() {
  page = AdminCrud.createPage({
    table: {
      selector: "#kt_roles_table",
      options: {
        serverSide: true,
        processing: true,
        searching: false, // the generic picker above is the search and filter UI
        order: [],
        pageLength: 10,
        lengthMenu: [10, 25, 50, 100],
        ajax: AdminTable.serverSideAjax({
          conditions: function () {
            return picker ? picker.conditions() : [];
          },
          errorMessage: "خطا در دریافت لیست نقش‌ها.",
        }),
        columns: [
          {
            data: null,
            defaultContent: "",
            name: "",
            orderable: false,
            className: "dtr-control",
          },
          { data: null, defaultContent: "", name: "title" },
          { data: null, defaultContent: "", name: "name" },
          { data: null, defaultContent: "", name: "isSystem" },
          { data: null, defaultContent: "", name: "userCount" },
          { data: null, defaultContent: "", name: "permissionCount" },
          { data: null, defaultContent: "", name: "createdAt" },
          {
            data: null,
            defaultContent: "",
            name: "",
            orderable: false,
            className: "min-tablet",
          },
        ],
        createdRow: fillRoleRow,
      },
    },
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

    loadItem: loadRole,

    fillForm: function (role) {
      role = role || { title: "", name: "" };
      $("#Form_Title").val(role.title);
      $("#Form_Name").val(role.name);
      $("#Form_Name").prop("readonly", false);
    },

    getData: function () {
      return {
        title: $("#Form_Title").val().trim(),
        name: $("#Form_Name").val().trim(),
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
        // The name is the identifier the code and the seeder look a role up by, so it stays one
        // word; the display name is the Title field.
        errors.push("نام (Name) نباید شامل فاصله باشد.");
        $name.addClass("is-invalid");
      } else {
        $name.removeClass("is-invalid");
      }

      return errors;
    },

    onSubmit: saveRole,
    onDelete: deleteRole,
  });
}
//#endregion

//#region Filter picker
/** Builds the picker from ?handler=Fields; a role has no exotic field kind. */
function loadFields() {
  picker = AdminTable.filterPicker({
    fieldEl: "#filterField",
    operatorEl: "#filterOperator",
    valueWrapEl: "#filterValueWrap",
    chipsEl: "#filterChips",
    onChange: function () {
      if (page) page.table.draw();
    },
  });

  return picker.load();
}
//#endregion

//#region Role Permissions (section 5.3)
/**
 * The permission tree is rendered by _RoleModals from the live catalogue, so the boxes are the
 * permissions that actually exist — a key retired from the code leaves the tree with the row that
 * held it. Only the *assignment* state is fetched here.
 */
var permissionsState = { roleId: null, roleName: "", isSystem: false };

/** Ticks the boxes the role already holds, then opens the modal. */
function openPermissionsModal(id) {
  var role = roleCache[id];
  if (!role) return;

  permissionsState.roleId = id;
  permissionsState.roleName = role.title;
permissionsState.isSystem = role.isSystem === true;
  $("#permissions-role-name").text(role.title);
  applyReadOnly(permissionsState.isSystem);

  $.getJSON(AdminTable.handlerUrl("Granted"), { id: id })
    .done(function (res) {
      if (!res || !res.isSuccess) {
        toastr.error(
          AdminTable.serverMessage(
            { responseJSON: res },
            "خطا در دریافت دسترسی‌های نقش.",
          ),
          "خطا",
        );
        return;
      }
      tickPermissions(res.data || []);
      AdminCrud.Utils.showModal("kt_modal_role_permissions");
    })
    .fail(function (xhr) {
      toastr.error(
        AdminTable.serverMessage(xhr, "خطا در دریافت دسترسی‌های نقش."),
        "خطا",
      );
    });
}

/** Sets the checked state from the granted identifiers and refreshes every group master. */
function tickPermissions(grantedIds) {
  var granted = {};
  grantedIds.forEach(function (id) {
    granted[String(id)] = true;
  });

  $(".perm-item").each(function () {
    this.checked = granted[String($(this).data("perm-id"))] === true;
  });

  setGroupMasters();
}

/**
 * A system role is inspected, not edited: the tree stays visible with its ticks but every box is
 * disabled and the save button is gone, so nothing can be submitted for it.
 * The notice switches classes instead of jQuery's display toggle, because the box is a flex row in
 * the markup and an inline "display: block" would stack its icon above its text.
 */
function applyReadOnly(readOnly) {
  $(".perm-item, .perm-group").prop("disabled", readOnly);
  $("#kt_role_permissions_submit").toggle(!readOnly);
  $("#permissions-readonly-note")
    .toggleClass("d-none", !readOnly)
    .toggleClass("d-flex", readOnly);
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
  if (this.disabled) return;

  var group = $(this).data("group");
  var $items = $('.perm-item[data-group="' + group + '"]');
  var allChecked =
    $items.length > 0 && $items.filter(":checked").length === $items.length;
  $items.prop("checked", !allChecked).trigger("change");
});

// Any child change refreshes every master (indeterminate included).
$(document).on("change", ".perm-item", function () {
  setGroupMasters();
});
//#endregion

/** Collects the checked permission identifiers into an array. */
function collectPermissions() {
  var out = [];
  $(".perm-item:checked").each(function () {
    out.push($(this).data("perm-id"));
  });
  return out;
}

/**
 * Rewrites the whole grant set through ?handler=SavePermissions. The complete list travels every
 * time — not a delta — so a stale checkbox in a half-opened modal cannot decide what an account
 * may do; an empty list is a real answer and revokes everything.
 */
function saveRolePermissions() {
  if (permissionsState.roleId === null || permissionsState.isSystem) return;

  var btn = document.getElementById("kt_role_permissions_submit");
  btn.setAttribute("data-kt-indicator", "on");
  btn.disabled = true;

  function finish() {
    btn.removeAttribute("data-kt-indicator");
    btn.disabled = false;
  }

  AdminTable.postHandler("SavePermissions", {
    roleId: permissionsState.roleId,
    permissionIds: collectPermissions(),
  })
    .done(function () {
      finish();
      AdminCrud.Utils.hideModal("kt_modal_role_permissions");
      toastr.success("دسترسی‌های نقش با موفقیت ذخیره شد.", "موفق");
      afterMutation(); // the count in the row just changed
    })
    .fail(function (xhr) {
      finish();
      toastr.error(
        AdminTable.serverMessage(xhr, "خطا در ذخیره دسترسی‌ها."),
        "خطا",
      );
    });
}
//#endregion

//#region Inline onclick Handlers (HTML compatibility)
function openCreateModal() {
  if (page) page.openCreate();
}
function submitRoleForm() {
  if (page) page.submit();
}
function confirmDelete() {
  if (page) page.confirmDelete();
}
function addFilterCondition() {
  if (picker) picker.add();
}
function clearFilterConditions() {
  if (picker) picker.clear();
}
//#endregion

//#region Boot
// The picker has to know the fields before the first request is built, so the table is created
// once ?handler=Fields has answered; a failed read still leaves a usable table.
$(function () {
  loadFields()
    .then(initPage)
    .fail(function () {
      initPage();
      toastr.error("خطا در دریافت فیلدهای قابل فیلتر.", "خطا");
    });
});
//#endregion
