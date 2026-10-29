//#region Users Page Scripts
/**
 * Admin "Users" catalog (sections 4.1 / 4.2 of the UI/UX doc): the authentication
 * accounts, rendered by a server-side DataTable and wired to the Razor Page handlers:
 *   GET  ?handler=Rows     → one page of rows (paging, sorting and filters resolved server-side)
 *   GET  ?handler=Fields   → the filterable fields with the operators each accepts
 *   GET  ?handler=UserOptions → the accounts as id/name pairs, for the author filters
 *   GET  ?handler=Details  → the full account for the details modal
 *   POST ?handler=Create / Update / ToggleStatus → form-encoded mutations
 *
 * Nothing here is field-specific: the picker is built from ?handler=Fields and every
 * condition — the value being typed as well as the committed chips — travels as a generic
 * Filters[i].Property/Operator/Value triple, so the backend never grows a query per field.
 * The table keeps only the account's own columns; roles, password state, profile data and
 * the security counters live in the details modal. Accounts are never deleted: disabling
 * is the supported state change. POSTs carry the token from @Html.AntiForgeryToken().
 */

var page; // shared CRUD controller (AdminCrud)
var fields = []; // ?handler=Fields metadata: [{ property, label, kind, operators, options }]
var conditions = []; // committed chips: [{ property, label, operator, operatorLabel, value }]
var detailCache = {}; // id → UserDetailsDto, shared by the details modal and the edit form
var detailsCurrentId = null; // the account the details modal shows (its «ویرایش» button)
var liveTimer = null; // debounce for the value box
var userDirectories = {}; // author field → [{ id, name }], the accounts holding that permission

//#region Field Metadata (generic)
/** Operator symbols/labels for the picker, keyed by the FilterOperator names. */
var OPERATOR_LABELS = {
  Equals: "مساوی با",
  NotEquals: "نامساوی با",
  Contains: "شامل",
  StartsWith: "شروع با",
  EndsWith: "ختم به",
  GreaterThan: "بزرگتر از",
  GreaterThanOrEqual: "بزرگتر یا مساوی با",
  LessThan: "کوچکتر از",
  LessThanOrEqual: "کوچکتر یا مساوی با",
  In: "یکی از",
};

/** Persian labels for the values a bool or an enum field can be filtered on. */
var VALUE_LABELS = {
  true: "فعال",
  false: "غیرفعال",
  Male: "مرد",
  Female: "زن",
};

/** Reads a value label, falling back to the raw option so nothing renders empty. */
function valueLabel(value) {
  if (value === "" || value === null || value === undefined) return "";
  return Object.prototype.hasOwnProperty.call(VALUE_LABELS, value)
    ? VALUE_LABELS[value]
    : String(value);
}

/** The field currently chosen in «فیلتر روی», or null before the metadata arrives. */
function currentField() {
  var name = $("#filterField").val();
  for (var i = 0; i < fields.length; i++) {
    if (fields[i].property === name) return fields[i];
  }
  return null;
}

/**
 * Fetches the accounts that hold the permission behind one author field (the server decides
 * which: user.create for CreatedBy, user.edit for UpdatedBy) and caches it per field, so the
 * picker lists only the people who could have done it and its chip can name them.
 */
function loadUserOptions(field) {
  if (userDirectories[field])
    return $.Deferred().resolve(userDirectories[field]).promise();

  return $.getJSON(handlerUrl("UserOptions"), { field: field }).then(
    function (res) {
      userDirectories[field] = (res && res.data) || [];
      return userDirectories[field];
    },
  );
}

/** Appends every allowed account to the given picker. */
function fillUserOptions($select, field) {
  loadUserOptions(field)
    .done(function (list) {
      if (!list.length) {
        $select.append(
          $("<option>").val("").text("کاربری این دسترسی را ندارد"),
        );
        return;
      }

      list.forEach(function (user) {
        $select.append($("<option>").val(user.id).text(user.name));
      });
    })
    .fail(function () {
      toastr.error("خطا در دریافت لیست کاربران برای انتخاب.", "خطا");
    });
}

/** Turns a comma-separated list of account ids into the names the admin picked. */
function userNames(field, value) {
  var directory = userDirectories[field];
  if (!directory) return value;

  var byId = {};
  directory.forEach(function (user) {
    byId[user.id] = user.name;
  });

  return value
    .split(",")
    .map(function (id) {
      return byId[id] || id;
    })
    .join("، ");
}

/** Builds the picker (field list, operators, value control) from ?handler=Fields. */
function loadFields() {
  return $.getJSON(handlerUrl("Fields")).then(function (res) {
    fields = (res && res.data) || [];

    var $field = $("#filterField").empty();
    fields.forEach(function (f) {
      $field.append($("<option>").val(f.property).text(f.label));
    });

    // Start on the first text field, so the value box is a working search box right away.
    var firstText = fields.filter(function (f) {
      return f.kind === "string";
    })[0];
    if (firstText) $field.val(firstText.property);

    renderOperators();
    renderValueControl();
  });
}

/** Refills the operator select with the operators the chosen field accepts. */
function renderOperators() {
  var field = currentField();
  var $op = $("#filterOperator").empty();
  if (!field) return;

  field.operators.forEach(function (op) {
    var label = OPERATOR_LABELS[op];
    $op.append(
      $("<option>")
        .val(op)
        .text(label ? label + " (" + op + ")" : op),
    );
  });
}

/**
 * Rebuilds the value control from the chosen field kind: an option list for a bool or an
 * enum, a list of the existing accounts for an audit-author field, a date/number box for
 * those kinds, plain text otherwise. Every control keeps the same id, so reading the current
 * value never depends on the kind.
 */
function renderValueControl() {
  var field = currentField();
  var $wrap = $("#filterValueWrap").empty();
  if (!field) return;

  var operator = $("#filterOperator").val();
  var options = field.options || [];

  // «created by» / «last edited by» name an account, so they are filtered by picking one.
  if (field.kind === "user") {
    var $users = $(
      '<select id="filterValue" class="form-select form-select-solid w-250px">',
    );
    if (operator === "In") $users.attr("multiple", true);
    $users.append($("<option>").val("").text("انتخاب کاربر..."));
    $wrap.append($users);
    fillUserOptions($users, field.property);
    bindValueInput();
    return;
  }

  if (options.length && operator !== "In") {
    var $select = $(
      '<select id="filterValue" class="form-select form-select-solid w-200px">',
    );
    $select.append($("<option>").val("").text("انتخاب..."));
    options.forEach(function (option) {
      $select.append($("<option>").val(option).text(valueLabel(option)));
    });
    $wrap.append($select);
  } else {
    var $input = $(
      '<input type="text" id="filterValue" class="form-control form-control-solid w-200px">',
    );
    if (field.kind === "date") $input.attr("type", "date").attr("dir", "ltr");
    else if (field.kind === "number")
      $input.attr("type", "number").attr("dir", "ltr");
    else if (field.kind === "guid") $input.attr("dir", "ltr");

    $input.attr(
      "placeholder",
      operator === "In"
        ? "چند مقدار با ویرگول"
        : field.kind === "string"
          ? "جستجو..."
          : "مقدار",
    );
    $wrap.append($input);
  }

  bindValueInput();
}

/** Live search: the value box filters while it is typed, debounced. */
function bindValueInput() {
  $("#filterValue")
    .off("input.change.users")
    .on("input.change.users", function () {
      clearTimeout(liveTimer);
      liveTimer = setTimeout(function () {
        if (page) page.table.draw();
      }, 350);
    });
}
//#endregion

//#region Conditions (chips + live search)
/**
 * The conditions sent with the next request: the committed chips plus the value currently
 * typed in the picker. The typed value is not committed, so it never duplicates a chip and
 * disappears on reload instead of lingering as an invisible filter.
 */
function activeConditions() {
  var all = conditions.slice();
  var field = currentField();
  var value = readValue();

  if (field && value) {
    all.push({
      property: field.property,
      operator: $("#filterOperator").val(),
      value: value,
    });
  }
  return all;
}

/** Reads the value control, joining a multi-select into the comma form the In operator wants. */
function readValue() {
  var raw = $("#filterValue").val();
  if (raw === null || raw === undefined) return "";
  return (Array.isArray(raw) ? raw : [raw])
    .map(function (x) {
      return String(x).trim();
    })
    .filter(function (x) {
      return x !== "";
    })
    .join(",");
}

/** Commits the picker state as a chip and re-queries the table. */
function addFilterCondition() {
  var field = currentField();
  var operator = $("#filterOperator").val();
  var value = readValue();

  if (!field || !operator) return;
  if (!value) {
    toastr.error("مقدار فیلتر را وارد کنید.", "خطا");
    return;
  }

  conditions.push({
    property: field.property,
    label: field.label,
    operator: operator,
    operatorLabel: OPERATOR_LABELS[operator] || operator,
    value:
      field.kind === "user"
        ? userNames(field.property, value)
        : valueLabel(value) || value,
  });

  $("#filterValue").val("");
  renderChips();
  if (page) page.table.draw();
}

/** Drops every chip and the typed value, then re-queries. */
function clearFilterConditions() {
  conditions = [];
  $("#filterValue").val("");
  renderChips();
  if (page) page.table.draw();
}

/** Renders the active chips, each with its own remove handler. */
function renderChips() {
  var $chips = $("#filterChips").empty();

  conditions.forEach(function (condition, index) {
    var $chip = $(
      '<span class="badge badge-light-primary d-inline-flex align-items-center fs-7 py-2 px-3">',
    );
    $chip.append(
      $("<span>")
        .text(
          condition.label +
            " " +
            condition.operatorLabel +
            " " +
            condition.value,
        )
        .attr("dir", "auto"),
    );

    var $remove = $(
      '<button type="button" class="btn btn-sm btn-icon btn-transparent-dark ms-1" title="حذف این فیلتر">',
    ).append(
      '<i class="ki-duotone ki-cross fs-4"><span class="path1"></span><span class="path2"></span></i>',
    );
    $remove.on("click", function () {
      conditions.splice(index, 1);
      renderChips();
      if (page) page.table.draw();
    });

    $chip.append($remove);
    $chips.append($chip);
  });
}
//#endregion

//#region Server Connectivity (Razor Page handlers)
/** The page URL of a handler, e.g. "/Admin/Users?handler=Rows". */
function handlerUrl(name) {
  return location.pathname + "?handler=" + name;
}

/** Reads the antiforgery token rendered by @Html.AntiForgeryToken(). */
function csrfToken() {
  return $('input[name="__RequestVerificationToken"]').val();
}

/** Pulls the server-side message out of a failed response. */
function serverMessage(xhr, fallback) {
  return (xhr && xhr.responseJSON && xhr.responseJSON.message) || fallback;
}

/**
 * Sends a form-encoded POST to one handler and adapts the {isSuccess, message} envelope to
 * the AdminCrud contract: resolves only when isSuccess is true, otherwise rejects with
 * {responseJSON} so the shared module surfaces the server message. Arrays are serialized
 * traditionally (roleIds=1&roleIds=2) — jQuery's default "roleIds[]" key would not bind.
 */
function postHandler(handler, data) {
  var dfd = $.Deferred();
  $.ajax({
    url: handlerUrl(handler),
    type: "POST",
    data: $.param(data, true),
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

/** Saves the create/edit modal, then reloads the table keeping its page. */
function saveUser(data, mode) {
  return postHandler(mode === "create" ? "Create" : "Update", data).done(
    afterMutation,
  );
}

/** Flips one account through ?handler=ToggleStatus, then reloads the table. */
function toggleUser(id) {
  return postHandler("ToggleStatus", { id: id }).done(afterMutation);
}

/** A mutation invalidates the cached details, so no modal can show stale data. */
function afterMutation() {
  detailCache = {};
  userDirectories = {}; // the author pickers list accounts too, so they refresh with them
  if (page) page.table.ajax.reload(null, false);
}

/** Reads one account from ?handler=Details, caching it for the modal and the edit form. */
function fetchDetails(id) {
  var dfd = $.Deferred();
  if (detailCache[id]) {
    dfd.resolve(detailCache[id]);
    return dfd.promise();
  }

  $.getJSON(handlerUrl("Details"), { id: id })
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
/** Formats a UTC timestamp as a Persian date. */
function faDate(value) {
  if (!value) return "";
  var date = new Date(value);
  return isNaN(date.getTime()) ? "" : date.toLocaleDateString("fa-IR");
}

/** Formats a UTC timestamp as a Persian date with time. */
function faDateTime(value) {
  if (!value) return "";
  var date = new Date(value);
  if (isNaN(date.getTime())) return "";
  return (
    date.toLocaleDateString("fa-IR") +
    " - " +
    date.toLocaleTimeString("fa-IR", { hour: "2-digit", minute: "2-digit" })
  );
}

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

function badge(modifier, text) {
  return $('<span class="badge d-inline-block me-1 mb-1">')
    .addClass("badge-light-" + modifier)
    .text(text);
}

/** Rewrites Persian (۰-۹) and Arabic-Indic (٠-٩) digits as Latin ones. */
function latinDigits(value) {
  return String(value === null || value === undefined ? "" : value)
    .replace(/[۰-۹]/g, function (d) {
      return String("۰۱۲۳۴۵۶۷۸۹".indexOf(d));
    })
    .replace(/[٠-٩]/g, function (d) {
      return String("٠١٢٣٤٥٦٧٨٩".indexOf(d));
    });
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
      badge(
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
      toastr.error(serverMessage(xhr, "خطا در دریافت جزئیات کاربر."), "خطا");
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
    badge(
      user.isActive ? "success" : "danger",
      user.isActive ? "فعال" : "غیرفعال",
    ),
    true,
  );
  setDetail(
    "#ud-password",
    badge(
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
          return badge("primary", title);
        })
      : badge("secondary", "کاربر عادی"),
    true,
  );

  setDetail(
    "#ud-providers",
    (user.providers || []).length
      ? (user.providers || []).map(function (provider) {
          return badge("info", provider);
        })
      : "ورود با موبایل",
    true,
  );

  setDetail("#ud-id", user.id);
  setDetail("#ud-email", user.email);
  setDetail("#ud-phone", user.phoneNumber);
  setDetail("#ud-birthdate", faDate(user.birthDate));
  setDetail("#ud-gender", valueLabel(user.gender));

  setDetail("#ud-last-login", faDateTime(user.lastLoginAt), false, "هرگز");
  setDetail("#ud-failed", user.failedLoginAttempts || 0);
  setDetail("#ud-lockout", faDateTime(user.lockoutUntil));
  setDetail("#ud-created", faDateTime(user.createdAt));
  setDetail("#ud-updated", faDateTime(user.updatedAt));
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
        ajax: {
          url: handlerUrl("Rows"),
          type: "GET",
          data: function (d) {
            // DataTables 2 hands over the clicked column as { column, dir, name }; the name is
            // the entity property each column declares, which is what the server resolves.
            var order = (d.order || [])[0];
            if (order) {
              var name =
                order.name ||
                ((d.columns || [])[order.column] || {}).name ||
                "";
              d.sortColumn = name;
              d.sortColumnDirection = order.dir || "";
            }
            // Indexed keys are what the ASP.NET Core binder expects for List<FilterCondition>.
            var active = activeConditions();
            for (var i = 0; i < active.length; i++) {
              d["Filters[" + i + "].Property"] = active[i].property;
              d["Filters[" + i + "].Operator"] = active[i].operator;
              d["Filters[" + i + "].Value"] = active[i].value;
            }
          },
          error: function (xhr) {
            toastr.error(
              serverMessage(xhr, "خطا در دریافت لیست کاربران."),
              "خطا",
            );
          },
        },
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
        toastr.error(serverMessage(xhr, "خطا در دریافت اطلاعات کاربر."), "خطا");
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
      var phone = latinDigits($("#Form_PhoneNumber").val().trim());
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
      toastr.error(serverMessage(xhr, "خطا در تغییر وضعیت."), "خطا");
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
//#endregion

//#region Boot
// The picker has to know the fields before the first request is built, so the table is
// created once ?handler=Fields has answered; a failed read still leaves a usable table.
$(function () {
  $("#filterField").on("change", function () {
    renderOperators();
    renderValueControl();
  });
  $("#filterOperator").on("change", renderValueControl);

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