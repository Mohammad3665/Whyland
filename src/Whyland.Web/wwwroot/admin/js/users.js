//#region Users Page Scripts
/**
 * Admin "Users" catalog (sections 4.1 / 4.2 of the UI/UX doc): the authentication
 * accounts, rendered by a server-side DataTable and wired to the Razor Page handlers:
 *   GET  ?handler=Rows        → one page of rows (paging, sorting and filters resolved server-side)
 *   GET  ?handler=Fields      → the filterable fields with the operators each accepts
 *   GET  ?handler=UserOptions → the accounts as id/name pairs, for the author filters
 *   GET  ?handler=Details     → the full account for the details modal
 *   POST ?handler=Create / Update / ToggleStatus → form-encoded mutations
 *
 * Nothing here is field-specific and nothing here is generic either: the table plumbing, the
 * filter picker and the server round-trip come from common/admin-table.js. What stays in this
 * file is what belongs to an account — its avatar, its details modal, the author pickers that
 * turn «created by» into a list of people, and the rules of the create/edit form. Accounts are
 * never deleted: disabling is the supported state change.
 */

var page; // shared CRUD controller (AdminCrud)
var detailCache = {}; // id → UserDetailsDto, shared by the details modal and the edit form
var detailsCurrentId = null; // the account the details modal shows (its «ویرایش» button)
var picker; // the generic filter picker (AdminTable.filterPicker)

//#region Filter picker
/**
 * Builds the picker from ?handler=Fields. Both lists this page filters on — the accounts behind an
 * audit column and the roles of the «نقش» column — are rendered by the shared picker, so the page
 * adds no kind of its own and only answers the two option endpoints they read.
 */
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

//#region Server Connectivity
/** Saves the create/edit modal, then reloads the table keeping its page. */
function saveUser(data, mode) {
  return AdminTable.postHandler(
    mode === "create" ? "Create" : "Update",
    data,
  ).done(afterMutation);
}

/** Flips one account through ?handler=ToggleStatus, then reloads the table. */
function toggleUser(id) {
  return AdminTable.postHandler("ToggleStatus", { id: id }).done(afterMutation);
}

/** A mutation invalidates the cached details, so no modal can show stale data. */
function afterMutation() {
  detailCache = {};
  AdminTable.resetOptionDirectories(); // the author and role pickers list rows a mutation changes
  if (page) page.table.ajax.reload(null, false);
}

/** Reads one account from ?handler=Details, caching it for the modal and the edit form. */
function fetchDetails(id) {
  var dfd = $.Deferred();
  if (detailCache[id]) {
    dfd.resolve(detailCache[id]);
    return dfd.promise();
  }

  $.getJSON(AdminTable.handlerUrl("Details"), { id: id })
    .done(function (res) {
      if (res && res.isSuccess && res.data) {
        detailCache[id] = res.data;
        dfd.resolve(res.data);
      } else {
        dfd.reject({ responseJSON: res });
      }
    })
    .fail(function (xhr) {
      dfd.reject(xhr);
    });

  return dfd.promise();
}
//#endregion

//#region Row Rendering
/** Initials shown in the avatar, e.g. "علی محمدی" → "ع‌م". */
function initials(user) {
  return (
    ((user.firstName || "").charAt(0) || "") +
    "‌" +
    ((user.lastName || "").charAt(0) || "")
  );
}

/**
 * Metronic soft colours for the initial avatars. The account id picks the colour, so a row
 * keeps it across paging, search and reloads instead of every avatar staying blue.
 */
var AVATAR_COLORS = [
  "bg-light-primary text-primary",
  "bg-light-success text-success",
  "bg-light-danger text-danger",
  "bg-light-warning text-warning",
  "bg-light-info text-info",
  "bg-light-secondary",
];

function avatarColor(id) {
  var text = String(id || "");
  var sum = 0;
  for (var i = 0; i < text.length; i++) sum += text.charCodeAt(i);
  return AVATAR_COLORS[sum % AVATAR_COLORS.length];
}

/**
 * The avatar of one account: its profile picture as soon as it has one, otherwise the
 * coloured initials. Shared by the table row and the details modal so both agree.
 */
function avatarNode(user) {
  if (user.avatar) {
    return $("<img>")
      .attr("src", user.avatar)
      .attr("alt", user.firstName + " " + user.lastName);
  }

  return $('<span class="symbol-label fw-bold">')
    .addClass(avatarColor(user.id))
    .text(initials(user));
}

/**
 * Fills one <tr> of the server-side table: avatar + full name, e-mail, phone, the status
 * badge and the row actions. Handlers bind with closures and every value goes through
 * .text(), so account data never reaches an inline onclick or the HTML.
 */
function fillUserRow(row, user) {
  var $cells = $("td", row);

  $cells
    .eq(1)
    .html(
      "<div class='d-flex align-items-center'>" +
        "<div class='symbol symbol-circle symbol-40px me-3 user-avatar'></div>" +
        "<span class='text-gray-800 fw-bold user-name'></span></div>",
    );
  $cells.eq(1).find(".user-avatar").append(avatarNode(user));
  $cells
    .eq(1)
    .find(".user-name")
    .text(user.firstName + " " + user.lastName);

  $cells
    .eq(2)
    .append(
      $('<span class="text-gray-600" dir="ltr">').text(user.email || "—"),
    );
  $cells
    .eq(3)
    .append(
      $('<span class="text-gray-600" dir="ltr">').text(user.phoneNumber || "—"),
    );

  $cells
    .eq(4)
    .append(
      AdminTable.badge(
        user.isActive ? "success" : "danger",
        user.isActive ? "فعال" : "غیرفعال",
      ),
    );

  var $actions = $cells.eq(5).addClass("text-end nowrap-cell");

  var $details = $(
    '<button type="button" class="btn btn-sm btn-icon btn-bg-light btn-active-color-primary me-1" title="جزئیات کاربر">',
  ).append(
    '<i class="ki-duotone ki-eye fs-2"><span class="path1"></span><span class="path2"></span><span class="path3"></span></i>',
  );
  $details.on("click", function () {
    openUserDetails(user.id);
  });

  var $edit = $(
    '<button type="button" class="btn btn-sm btn-icon btn-bg-light btn-active-color-primary me-1" title="ویرایش">',
  ).append(
    '<i class="ki-duotone ki-pencil fs-2"><span class="path1"></span><span class="path2"></span></i>',
  );
  $edit.on("click", function () {
    if (page) page.openEdit(user.id);
  });

  var $toggle = $(
    '<button type="button" class="btn btn-sm btn-icon btn-bg-light btn-active-color-' +
      (user.isActive ? "warning" : "success") +
      '" title="تغییر وضعیت">',
  ).append(
    user.isActive
      ? '<i class="ki-duotone ki-shield-cross fs-2"><span class="path1"></span><span class="path2"></span><span class="path3"></span></i>'
      : '<i class="ki-duotone ki-shield-tick fs-2"><span class="path1"></span><span class="path2"></span><span class="path3"></span></i>',
  );
  $toggle.on("click", function () {
    openToggleStatusModal(user);
  });

  $actions.append($details, $edit, $toggle);
}
//#endregion

//#region Details Modal
/**
 * Opens the details modal with everything the table no longer shows: roles, the password
 * state, profile data, linked providers and the login-security counters.
 */
function openUserDetails(id) {
  fetchDetails(id)
    .done(function (user) {
      detailsCurrentId = user.id;
      fillDetailsModal(user);
      AdminCrud.Utils.showModal("kt_modal_user_details");
    })
    .fail(function (xhr) {
      toastr.error(
        AdminTable.serverMessage(xhr, "خطا در دریافت جزئیات کاربر."),
        "خطا",
      );
    });
}

/** Writes one label; an empty value shows a dash (or the given word) instead. */
function setDetail(selector, value, isHtml, fallback) {
  var $el = $(selector).empty();
  if (
    value === null ||
    value === undefined ||
    value === "" ||
    value === false
  ) {
    $el.text(fallback || "—");
    return;
  }
  if (isHtml) $el.append(value);
  else $el.text(String(value));
}

function fillDetailsModal(user) {
  // Same rule as the row: the picture when there is one, coloured initials when not.
  $("#ud-symbol").empty().append(avatarNode(user));

  $("#ud-name").text(user.firstName + " " + user.lastName);
  $("#ud-subtitle").text(
    [user.phoneNumber, user.email].filter(Boolean).join(" | ") ||
      "راه ارتباطی ثبت نشده",
  );

  setDetail(
    "#ud-status",
    AdminTable.badge(
      user.isActive ? "success" : "danger",
      user.isActive ? "فعال" : "غیرفعال",
    ),
    true,
  );
  setDetail(
    "#ud-password",
    AdminTable.badge(
      user.hasPassword ? "success" : "warning",
      user.hasPassword ? "رمز عبور تنظیم شده" : "رمز عبور تنظیم نشده",
    ),
    true,
  );

  // An account without roles is an ordinary user, never a "role-less" one.
  setDetail(
    "#ud-roles",
    (user.roles || []).length
      ? (user.roles || []).map(function (title) {
          return AdminTable.badge("primary", title);
        })
      : AdminTable.badge("secondary", "کاربر عادی"),
    true,
  );

  setDetail(
    "#ud-providers",
    (user.providers || []).length
      ? (user.providers || []).map(function (provider) {
          return AdminTable.badge("info", provider);
        })
      : "ورود با موبایل",
    true,
  );

  setDetail("#ud-id", user.id);
  setDetail("#ud-email", user.email);
  setDetail("#ud-phone", user.phoneNumber);
  setDetail("#ud-birthdate", AdminTable.faDate(user.birthDate));
  setDetail("#ud-gender", AdminTable.valueLabel(user.gender));

  setDetail("#ud-last-login", AdminTable.faDateTime(user.lastLoginAt), false, "هرگز");
  setDetail("#ud-failed", user.failedLoginAttempts || 0);
  setDetail("#ud-lockout", AdminTable.faDateTime(user.lockoutUntil));
  setDetail("#ud-created", AdminTable.faDateTime(user.createdAt));
  setDetail("#ud-updated", AdminTable.faDateTime(user.updatedAt));
}

/** Closes the details modal and opens the same account in the edit form. */
function openUserEditFromDetails() {
  if (!detailsCurrentId) return;
  AdminCrud.Utils.hideModal("kt_modal_user_details");
  if (page) page.openEdit(detailsCurrentId);
}
//#endregion

//#region CRUD Page Configuration
function initPage() {
  page = AdminCrud.createPage({
    table: {
      selector: "#kt_users_table",
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
          errorMessage: "خطا در دریافت لیست کاربران.",
        }),
        columns: [
          {
            data: null,
            defaultContent: "",
            name: "",
            orderable: false,
            className: "dtr-control",
          },
          {
            data: null,
            defaultContent: "",
            name: "firstName",
            className: "min-tablet",
          },
          { data: null, defaultContent: "", name: "email" },
          {
            data: null,
            defaultContent: "",
            name: "phoneNumber",
            className: "min-tablet",
          },
          {
            data: null,
            defaultContent: "",
            name: "isActive",
            className: "min-tablet",
          },
          {
            data: null,
            defaultContent: "",
            name: "",
            orderable: false,
            className: "min-tablet",
          },
        ],
        createdRow: fillUserRow,
      },
    },
    modal: {
      id: "kt_modal_user",
      titleEl: "#modal-user-title",
      submitBtn: "kt_modal_user_submit",
    },
    texts: {
      createTitle: "افزودن کاربر",
      editTitle: "ویرایش کاربر",
      created: "کاربر با موفقیت ایجاد شد.",
      updated: "تغییرات با موفقیت ذخیره شد.",
    },

    // Edit reads the same DTO the details modal shows, so the roles come along as ids.
    loadItem: function (id) {
      return fetchDetails(id).fail(function (xhr) {
        toastr.error(
          AdminTable.serverMessage(xhr, "خطا در دریافت اطلاعات کاربر."),
          "خطا",
        );
      });
    },

    fillForm: function (user) {
      user = user || {
        firstName: "",
        lastName: "",
        email: "",
        phoneNumber: "",
        isActive: true,
        roleIds: [],
      };
      $("#Form_FirstName").val(user.firstName);
      $("#Form_LastName").val(user.lastName);
      $("#Form_Email").val(user.email || "");
      $("#Form_PhoneNumber").val(user.phoneNumber || "");
      $("#Form_IsActive").prop("checked", user.isActive);
      // select2 renders its own box from this element, so every value change needs a trigger.
      $("#Form_Roles")
        .val(
          (user.roleIds || []).map(function (id) {
            return String(id);
          }),
        )
        .trigger("change");
      $("#Form_Password").val("");

      // Password is create-only: credentials are changed by their own flows.
      setPasswordVisibility(page.state.mode === "create");
    },

    getData: function () {
      var roleValues = $("#Form_Roles").val();
      return {
        firstName: $("#Form_FirstName").val().trim(),
        lastName: $("#Form_LastName").val().trim(),
        email: $("#Form_Email").val().trim(),
        phoneNumber: $("#Form_PhoneNumber").val().trim(),
        password: $("#Form_Password").val(),
        isActive: $("#Form_IsActive").is(":checked"),
        roleIds: roleValues || [],
      };
    },

    validate: function () {
      var errors = [];
      var requiredFields = [
        { el: "#Form_FirstName", msg: "نام الزامی است." },
        { el: "#Form_LastName", msg: "نام خانوادگی الزامی است." },
      ];

      // Password is required only when creating a new user.
      if (page.state.mode === "create") {
        requiredFields.push({
          el: "#Form_Password",
          msg: "رمز عبور الزامی است.",
        });
      }

      requiredFields.forEach(function (f) {
        if (!$(f.el).val().trim()) {
          errors.push(f.msg);
          $(f.el).addClass("is-invalid");
        } else {
          $(f.el).removeClass("is-invalid");
        }
      });

      // Shape checks only; the uniqueness of the number and the e-mail is server-side.
      // Persian digits are read as their Latin equivalents so a valid number is not rejected
      // here on its way to the server, which normalizes it the same way before storing it.
      var phone = AdminTable.latinDigits($("#Form_PhoneNumber").val().trim());
      if (phone && !/^\+?[0-9]{10,15}$/.test(phone)) {
        errors.push("شماره موبایل معتبر وارد کنید.");
        $("#Form_PhoneNumber").addClass("is-invalid");
      } else {
        $("#Form_PhoneNumber").removeClass("is-invalid");
      }

      var email = $("#Form_Email").val().trim();
      if (email && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
        errors.push("فرمت ایمیل صحیح نیست.");
        $("#Form_Email").addClass("is-invalid");
      } else {
        $("#Form_Email").removeClass("is-invalid");
      }
      return errors;
    },

    onSubmit: saveUser,
  });
}
//#endregion

//#region Password Visibility (create-only field)
/** Shows or hides the password field depending on create vs edit mode. */
function setPasswordVisibility(visible) {
  $("#user-password-row").toggle(visible);
  $("#Form_Password").prop("required", visible);
}
//#endregion

//#region Toggle Active Status
var toggleTarget = null; // { id, name, makeActive }

/**
 * Opens the toggle-status confirmation. makeActive is the state the row gets after
 * confirmation; the server flips the flag itself, so it only drives the wording.
 */
function openToggleStatusModal(user) {
  toggleTarget = {
    id: user.id,
    name: user.firstName + " " + user.lastName,
    makeActive: !user.isActive,
  };
  $("#toggle-status-user-name").text(toggleTarget.name);
  $("#toggle-status-user-text").text(
    toggleTarget.makeActive ? "فعال شود؟" : "غیرفعال شود؟",
  );
  AdminCrud.Utils.showModal("kt_modal_toggle_user_status");
}

function confirmToggleStatus() {
  if (!toggleTarget) return;

  var target = toggleTarget;
  toggleTarget = null;
  AdminCrud.Utils.hideModal("kt_modal_toggle_user_status");

  toggleUser(target.id)
    .done(function () {
      toastr.success(
        "وضعیت کاربر «" + target.name + "» با موفقیت تغییر کرد.",
        "موفق",
      );
    })
    .fail(function (xhr) {
      toastr.error(AdminTable.serverMessage(xhr, "خطا در تغییر وضعیت."), "خطا");
    });
}
//#endregion

//#region Inline onclick Handlers (HTML compatibility)
function openCreateModal() {
  if (page) page.openCreate();
}
function submitUserForm() {
  if (page) page.submit();
}
function addFilterCondition() {
  if (picker) picker.add();
}
function clearFilterConditions() {
  if (picker) picker.clear();
}
//#endregion

//#region Boot
// The picker has to know the fields before the first request is built, so the table is
// created once ?handler=Fields has answered; a failed read still leaves a usable table.
$(function () {
  loadFields()
    .then(initPage)
    .fail(function () {
      initPage();
      toastr.error("خطا در دریافت فیلدهای قابل فیلتر.", "خطا");
    });
});
//#endregion

//#region Roles Select2 (inside the user modal)
// The roles select is a multi-select, so Select2 puts the search box inline by default.
// Initialising on "shown.bs.modal" makes Select2 measure the modal after it is laid out,
// so the dropdown aligns with the field instead of sticking to the top-left corner.
$("#kt_modal_user").on("shown.bs.modal", function () {
  $("#Form_Roles").select2({
    dropdownParent: $("#kt_modal_user .modal-content"),
    placeholder: "انتخاب نقش‌ها",
    allowClear: true,
    width: "100%",
    dropdownAutoWidth: false,
    dropdownPosition: "below",
    dir: "rtl",
  });
});
//#endregion
