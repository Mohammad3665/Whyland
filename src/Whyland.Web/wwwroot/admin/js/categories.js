//#region Categories Page Scripts
/**
 * Admin "Categories" list page (section 7.1 of the SRS): the course category
 * tree with drag & drop reordering (DataTables RowReorder), the generic advanced
 * filter picker (same engine as the users list), create/edit modal, details modal
 * and delete confirmation. Built on top of the shared AdminCrud module
 * (admin-crud.js) and wired to the Razor Page handlers:
 *   GET  ?handler=Rows        → JSON rows (tree order, with parent / depth / audit columns),
 *                               honouring Filters[i].Property|Operator|Value
 *   GET  ?handler=Fields      → the filterable columns and their operators
 *   GET  ?handler=UserOptions → the accounts as id/name pairs, for the author filters
 *   POST ?handler=Create   → form-encoded create
 *   POST ?handler=Update   → form-encoded update
 *   POST ?handler=Delete   → form-encoded delete (id)
 *   POST ?handler=Reorder  → JSON [{ id, displayOrder }]
 * POSTs carry the antiforgery token rendered by @Html.AntiForgeryToken().
 */

var page; // shared CRUD page controller (set in initPage below)
var reorder; // shared drag & drop ordering controller (set below)

var fields = []; // ?handler=Fields metadata: [{ property, label, kind, operators, options }]
var conditions = []; // committed chips: [{ property, label, operator, operatorLabel, value, raw }]
var liveTimer = null; // debounce for the value box
var userDirectories = {}; // author field → [{ id, name }], the accounts holding that permission

/**
 * The rows of the last ?handler=Rows load: rowsList keeps the table order and
 * rowsById indexes it. The parent picker, the details modal and the depth rules
 * are all built out of this cache, so the page needs no second endpoint.
 */
var rowsList = [];
var rowsById = {};

/** The ceilings the server enforces, mirrored here only to warn before the round-trip. */
var MAX_DEPTH = 3;
var MAX_CHILDREN = 3;

//#region Server Connectivity (Razor Page handlers)
/**
 * Reads the antiforgery token rendered by @Html.AntiForgeryToken().
 */
function csrfToken() {
  return $('input[name="__RequestVerificationToken"]').val();
}

/**
 * Sends a form-encoded POST to one page handler and adapts the flat
 * {isSuccess, message} envelope to the AdminCrud contract: resolves only
 * when isSuccess is true; otherwise rejects with {responseJSON} so the
 * shared module can surface the server message instead of a generic one.
 */
function postHandler(handler, data) {
  var dfd = $.Deferred();
  $.post({
    url: location.pathname + "?handler=" + handler,
    data: data,
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

/** Saves the create/edit modal through ?handler=Create / ?handler=Update. */
function saveCategory(data, mode) {
  return postHandler(mode === "create" ? "Create" : "Update", data).done(
    refreshRows,
  );
}

/** Deletes a category by id through ?handler=Delete. */
function deleteCategory(id) {
  return postHandler("Delete", { id: id }).done(refreshRows);
}

/**
 * Persists the visual order produced by drag & drop through ?handler=Reorder.
 * The server numbers every sibling group separately, so the client sends the
 * sequence only and never a group key.
 */
function saveOrder(rows) {
  $.ajax({
    url: location.pathname + "?handler=Reorder",
    type: "POST",
    contentType: "application/json; charset=UTF-8",
    data: JSON.stringify(rows),
    headers: { RequestVerificationToken: csrfToken() },
  })
    .done(function (res) {
      if (res && res.isSuccess) {
        // The rows cached from the last load still hold the old numbers, but both
        // the edit form and the details modal read the badge instead of them, so
        // the new order is visible without refetching anything.
        toastr.success("ترتیب دسته‌بندی‌ها با موفقیت ذخیره شد.", "موفق");
      } else {
        toastr.error((res && res.message) || "خطا در ذخیره ترتیب.", "خطا");
        refreshRows(); // restore the persisted order on the table
      }
    })
    .fail(function () {
      toastr.error("خطا در ذخیره ترتیب.", "خطا");
      refreshRows();
    });
}
//#endregion

//#region Generic Filter Picker (same engine as the users list)
/**
 * The field list, the operators of every field and the option values all come from
 * ?handler=Fields, so filtering a new column costs no markup and no new query: the
 * conditions travel as Filters[i].Property|Operator|Value and the shared data-table
 * engine turns them into parameterised SQL, exactly like the users table.
 */

/** Operator labels for the picker, keyed by the FilterOperator names. */
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

/** Persian labels for the values a bool field can be filtered on. */
var VALUE_LABELS = {
  true: "فعال",
  false: "غیرفعال",
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

  return $.getJSON(location.pathname + "?handler=UserOptions", {
    field: field,
  }).then(function (res) {
    userDirectories[field] = (res && res.data) || [];
    return userDirectories[field];
  });
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

/** The other direction, for a hint the admin reads instead of types. */
function faDigits(value) {
  return String(value).replace(/\d/g, function (d) {
    return "۰۱۲۳۴۵۶۷۸۹"[Number(d)];
  });
}

/**
 * Whether a typed value is usable for the chosen field. A field that declares a range — the tree
 * level is one — cannot be searched outside it: the walk stops here, so no request asks for a
 * level that does not exist.
 */
function inRange(field, value) {
  if (!rangeHint(field)) return true;

  return value
    .split(",")
    .filter(function (x) {
      return x !== "";
    })
    .every(function (part) {
      var number = Number(part);
      if (isNaN(number) || String(number) !== part) return false;
      if (
        field.min !== null &&
        field.min !== undefined &&
        number < Number(field.min)
      )
        return false;
      if (
        field.max !== null &&
        field.max !== undefined &&
        number > Number(field.max)
      )
        return false;
      return true;
    });
}

/** The Persian wording of a field's allowed range, empty when the value is unbounded. */
function rangeHint(field) {
  if (!field || field.kind !== "number") return "";

  var hasMin = field.min !== null && field.min !== undefined;
  var hasMax = field.max !== null && field.max !== undefined;
  if (!hasMin && !hasMax) return "";
  if (hasMin && hasMax)
    return faDigits(field.min) + " تا " + faDigits(field.max);
  return hasMax
    ? "تا " + faDigits(field.max)
    : "از " + faDigits(field.min) + " به بعد";
}

/** Marks the value box the way the rest of the admin marks an unusable input. */
function markInvalid(invalid) {
  $("#filterValue").toggleClass("is-invalid", invalid);
}

/** Builds the picker (field list, operators, value control) from ?handler=Fields. */
function loadFields() {
  return $.getJSON(location.pathname + "?handler=Fields").then(function (res) {
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
 * Rebuilds the value control from the kind of the chosen field: a text box for names, a
 * number box for orders and levels, a date box for the audit timestamps, an option list for
 * a bool and a user picker for an author column — so no field needs its own markup.
 */
function renderValueControl() {
  var field = currentField();
  var $wrap = $("#filterValueWrap").empty();
  if (!field) return;

  var operator = $("#filterOperator").val();
  var options = field.options || [];

  // «created by» / «updated by» name an account, so they are filtered by picking one.
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
      // A text box with a numeric keyboard, not <input type="number">: the latter throws away
      // anything typed in Persian digits, and every numeric input of this admin accepts ۰-۹.
      $input.attr("inputmode", "numeric").attr("dir", "rtl");
    else if (field.kind === "guid") $input.attr("dir", "ltr");

    // A field that declares a range says so on the box; inRange() is what enforces it.
    var hint = rangeHint(field);
    if (hint) {
      if (field.min !== null && field.min !== undefined)
        $input.attr("min", field.min);
      if (field.max !== null && field.max !== undefined)
        $input.attr("max", field.max);
      $input.attr("title", hint);
    }

    $input.attr(
      "placeholder",
      operator === "In"
        ? "چند مقدار با ویرگول"
        : field.kind === "string"
          ? "جستجو..."
          : hint || "مقدار",
    );
    $wrap.append($input);
  }

  bindValueInput();
}

/**
 * Live search: the value box filters while it is typed, debounced. The table is not
 * server-side paged here (the whole tree has to stay in the DOM for drag &amp; drop),
 * so a change reloads the rows through the handler instead of redrawing an Ajax page.
 */
function bindValueInput() {
  $("#filterValue")
    .off("input.change.categories")
    .on("input.change.categories", function () {
      // The box goes red while the value is outside the field's range, before the request runs.
      markInvalid(!inRange(currentField(), readValue()));
      clearTimeout(liveTimer);
      liveTimer = setTimeout(refreshRows, 350);
    });
}

/**
 * The conditions sent with the next request: the committed chips plus the value currently
 * typed in the picker. The typed value is not committed, so it never duplicates a chip and
 * disappears on reload instead of lingering as an invisible filter.
 */
function activeConditions() {
  var all = conditions.slice();
  var field = currentField();
  var value = readValue();

  // An out-of-range value is not sent at all: the box says why, the query stays untouched.
  if (field && value && inRange(field, value)) {
    all.push({
      property: field.property,
      operator: $("#filterOperator").val(),
      // The typed value is the raw one: what the chip shows and what the server filters on
      // are the same string here.
      raw: value,
    });
  }
  return all;
}

/** Reads the value control, joining a multi-select into the comma form the In operator wants. */
function readValue() {
  var raw = $("#filterValue").val();
  if (raw === null || raw === undefined) return "";

  var field = currentField();
  return (Array.isArray(raw) ? raw : [raw])
    .map(function (x) {
      var text = String(x).trim();

      // A typed number or date is written in Persian digits by an admin who means 2, not by
      // anyone searching a name that happens to contain ۲ — so only the non-text kinds convert.
      return field && field.kind !== "string" ? latinDigits(text) : text;
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
  if (!inRange(field, value)) {
    toastr.error(
      "مقدار «" + field.label + "» باید " + rangeHint(field) + " باشد.",
      "خطا",
    );
    markInvalid(true);
    return;
  }

  conditions.push({
    property: field.property,
    label: field.label,
    operator: operator,
    operatorLabel: OPERATOR_LABELS[operator] || operator,
    // The chip shows a human label (an account name, «فعال») but the server must keep
    // filtering on what was really chosen, so the raw value travels next to it.
    raw: value,
    value:
      field.kind === "user"
        ? userNames(field.property, value)
        : valueLabel(value) || value,
  });

  $("#filterValue").val("");
  renderChips();
  refreshRows();
}

/** Drops every chip and the typed value, then re-queries. */
function clearFilterConditions() {
  conditions = [];
  $("#filterValue").val("");
  renderChips();
  refreshRows();
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
      refreshRows();
    });

    $chip.append($remove);
    $chips.append($chip);
  });
}
//#endregion

//#region Row Rendering (from server DTOs)
/** Formats a UTC timestamp as a localized Persian date for the table. */
function categoryDate(value) {
  var d = new Date(value);
  return isNaN(d.getTime()) ? "" : d.toLocaleDateString("fa-IR");
}

/** Formats a UTC timestamp as a localized Persian date and time for the details modal. */
function categoryDateTime(value) {
  if (!value) return "—";
  var d = new Date(value);
  return isNaN(d.getTime()) ? "—" : d.toLocaleString("fa-IR");
}

/**
 * Builds one <tr> in the shape the FAQ table uses: a responsive control cell, then
 * the «ترتیب» cell that is itself the drag handle and carries the order badge right
 * next to it; then the two names (the first indented by the level the row sits on),
 * the parent, the status, the creation date and the row actions. Handlers are bound
 * with closures so user data never lands inside inline onclick strings.
 */
function buildCategoryRow(cat) {
  var $row = $(
    '<tr><td class="dtr-control-col"></td>' +
      '<td tabindex="0" class="reorder text-center dt-type-numeric dtr-control">' +
      '<div class="d-flex align-items-center justify-content-center">' +
      '<i class="ki-duotone ki-burger-menu-2 fs-2 text-gray-400 cursor-move me-2">' +
      '<span class="path1"></span><span class="path2"></span><span class="path3"></span>' +
      '<span class="path4"></span><span class="path5"></span><span class="path6"></span>' +
      '<span class="path7"></span><span class="path8"></span><span class="path9"></span><span class="path10"></span>' +
      "</i>" +
      '<span class="cat-order badge badge-light-secondary"></span>' +
      "</div></td>" +
      "<td></td>" +
      "<td></td>" +
      "<td></td>" +
      '<td class="text-center"></td>' +
      "<td></td>" +
      '<td class="text-end nowrap-cell"></td>' +
      "</tr>",
  );

  $row.attr("data-id", cat.id).attr("data-active", cat.isActive ? 1 : 0);

  var cells = $row.find("td");
  $(cells[1]).find(".cat-order").text(cat.displayOrder);

  var $name = $(cells[2]);

  // One flex line for the indent, the arrow and the name. A .ki-duotone icon is an inline-flex
  // whose own paths are absolutely positioned, so it has no in-flow baseline to sit on and lands
  // above the text; centring the three against each other is what keeps them on one line.
  var $line = $('<span class="d-flex align-items-center">');
  if (cat.depth > 1) {
    // The indentation is what makes the branch readable in a flat table.
    $line.append(
      $("<span>").css({
        width: (cat.depth - 1) * 20 + "px",
        "flex-shrink": 0,
      }),
      $('<i class="ki-duotone ki-arrow-right fs-6 text-gray-500 me-1">').append(
        '<span class="path1"></span><span class="path2"></span>',
      ),
    );
  }
  $line.append($('<span class="text-gray-800 fw-bold">').text(cat.name));
  $name.append($line);

  $(cells[3]).append($('<span class="text-gray-600">').text(cat.latinName));

  $(cells[4])
    .attr("data-order", cat.parentName || "")
    .append(
      cat.parentName
        ? $('<span class="badge badge-light-info">').text(cat.parentName)
        : $('<span class="text-muted">').text("—"),
    );

  $(cells[5])
    .attr("data-order", cat.isActive ? 1 : 0)
    .append(
      $(
        '<span class="badge badge-light-' +
          (cat.isActive ? "success" : "danger") +
          '">',
      ).text(cat.isActive ? "فعال" : "غیرفعال"),
    );

  $(cells[6])
    .attr("data-order", cat.createdAt)
    .append(
      $('<span class="text-gray-600">').text(categoryDate(cat.createdAt)),
    );

  var $details = $(
    '<button type="button" class="btn btn-sm btn-icon btn-bg-light btn-active-color-info me-1" title="جزئیات">',
  ).append(
    '<i class="ki-duotone ki-eye fs-2"><span class="path1"></span><span class="path2"></span></i>',
  );
  $details.on("click", function () {
    openDetailsModal(cat.id);
  });

  var $edit = $(
    '<button type="button" class="btn btn-sm btn-icon btn-bg-light btn-active-color-primary me-1" title="ویرایش">',
  ).append(
    '<i class="ki-duotone ki-pencil fs-2"><span class="path1"></span><span class="path2"></span></i>',
  );
  $edit.on("click", function () {
    openEditModal(cat.id);
  });

  var $delete = $(
    '<button type="button" class="btn btn-sm btn-icon btn-bg-light btn-active-color-danger" title="حذف">',
  ).append(
    '<i class="ki-duotone ki-trash fs-2"><span class="path1"></span><span class="path2"></span><span class="path3"></span><span class="path4"></span><span class="path5"></span></i>',
  );
  $delete.on("click", function () {
    if (page) page.openDelete(cat.id, cat.name);
  });

  $(cells[7]).append($details, $edit, $delete);

  return $row[0];
}

/**
 * Fetches the row set from ?handler=Rows — carrying every active filter condition, so the
 * server resolves them with the same engine the users table uses — refills the cache the
 * parent picker and the details modal read, and builds the <tr> nodes; rows arrive in tree
 * order, so a parent is always followed by its own children.
 */
function loadRows() {
  // Indexed keys are what the ASP.NET Core binder expects for List<FilterCondition>.
  var active = activeConditions();
  var params = {};
  for (var i = 0; i < active.length; i++) {
    params["Filters[" + i + "].Property"] = active[i].property;
    params["Filters[" + i + "].Operator"] = active[i].operator;
    params["Filters[" + i + "].Value"] = active[i].raw;
  }

  return $.getJSON(location.pathname + "?handler=Rows", params).then(
    function (res) {
      var data = (res && res.data) || [];
      var nodes = [];

      rowsList = data;
      rowsById = {};
      for (var i = 0; i < data.length; i++) {
        rowsById[data[i].id] = data[i];
        nodes.push(buildCategoryRow(data[i]));
      }

      return nodes;
    },
  );
}

/**
 * Reloads from the server and reinjects every row into the (already
 * initialised) DataTable; used after every successful save/delete.
 */
function refreshRows() {
  return loadRows().then(function (rows) {
    $("#kt_categories_table tbody").empty().append(rows);
    page.table.clear();
    page.table.rows.add(rows).draw();
    reorder.renumber();
  });
}
//#endregion

//#region Hierarchy Helpers (mirror the server rules in the form)
/** The id of a row and of everything below it: none of them may become its parent. */
function branchIds(id) {
  var ids = {};
  var pending = [id];

  while (pending.length) {
    var current = pending.shift();
    if (ids[current]) continue;
    ids[current] = true;

    for (var i = 0; i < rowsList.length; i++) {
      if (rowsList[i].parentId === current) pending.push(rowsList[i].id);
    }
  }

  return Object.keys(ids).map(Number);
}

/** The rows of one sibling group; null means the root level. */
function siblingsOf(parentId) {
  var group = parentId ? Number(parentId) : null;
  return rowsList.filter(function (row) {
    return (row.parentId || null) === group;
  });
}

/**
 * The order a new row gets by default: one past the last row of the sibling
 * group it is being added to, so «افزودن» never drops a category in front of
 * its siblings.
 */
function nextOrderFor(parentId) {
  var max = 0;

  siblingsOf(parentId).forEach(function (row) {
    // After a drag & drop the badge holds the live number; the cache does not.
    var order = reorder
      ? reorder.orderOf(row.id, row.displayOrder)
      : row.displayOrder;
    max = Math.max(max, order);
  });

  return max + 1;
}

/** The level a child of the chosen parent would sit on; a root is level 1. */
function childDepthOf(parentId) {
  if (!parentId) return 1;
  var parent = rowsById[parentId];
  return parent ? parent.depth + 1 : 1;
}

/**
 * How many children one row already holds. The row being edited is skipped: it occupies a slot
 * under its current parent, so counting it would make its own parent look one child too full.
 */
function childCountOf(parentId, excludeId) {
  var skip = excludeId ? Number(excludeId) : null;
  return siblingsOf(parentId).filter(function (row) {
    return row.id !== skip;
  }).length;
}
//#endregion

//#region Parent Picker
/**
 * Rebuilds the «والد» options from the rows on the table: the tree the operator
 * sees is the tree they choose from, so no second endpoint can drift out of sync
 * with it. A row can never be hung on itself or on its own branch, a row that
 * already sits on the last allowed level cannot be chosen as a parent, and neither
 * can one that is already holding the full set of children.
 */
function fillParentOptions(excludeId) {
  var $select = $("#Form_ParentId");
  var selected = $select.val() || "";
  var blocked = excludeId ? branchIds(excludeId) : [];
  var html = ['<option value="">بدون والد (سطح اول)</option>'];

  for (var i = 0; i < rowsList.length; i++) {
    var row = rowsList[i];
    var isBlocked = blocked.indexOf(row.id) !== -1;
    var isDeep = childDepthOf(row.id) > MAX_DEPTH;
    var isFull = childCountOf(row.id, excludeId) >= MAX_CHILDREN;
    var label = "";
    for (var level = 1; level < row.depth; level++) label += "— ";
    label += row.name;

    var note = "";
    if (isDeep) note += " (سطح سوم)";
    if (isFull) note += " (پر: " + faDigits(MAX_CHILDREN) + " فرزند)";

    html.push(
      '<option value="' +
        row.id +
        '"' +
        (isBlocked || note ? " disabled" : "") +
        ">" +
        $("<div>").text(label).html() +
        (isBlocked ? "" : note) +
        "</option>",
    );
  }

  // select2 keeps its own copy of the option list, so the rebuilt markup has to be
  // pushed through a change event to reach the visible dropdown.
  $select.html(html.join(""));
  $select.val(selected).trigger("change");
}
//#endregion

//#region CRUD Page Configuration
function initPage() {
  page = AdminCrud.createPage({
    table: {
      selector: "#kt_categories_table",
      options: $.extend(true, AdminCrud.rowReorderOptions(), {
        // The generic picker above the table is the search and filter UI: it queries the
        // server, so the DataTable's own box would only search the already filtered rows.
        searching: false,
        responsive: {
          breakpoints: [
            { name: "desktop", width: 1260 },
            { name: "tablet-l", width: 960 },
            { name: "tablet", width: 930 },
            { name: "phone", width: 576 },
          ],
        },
        columnDefs: [
          { orderable: false, targets: [0, 1, 7] },
          { className: "all", targets: 1 },
          { className: "min-tablet", targets: [3, 4, 5, 6, 7] },
          { className: "dtr-control", targets: 0 },
        ],
      }),
    },
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
    fillForm: function (cat) {
      cat = cat || {
        id: null,
        name: "",
        latinName: "",
        displayOrder: 0,
        parentId: null,
        isActive: true,
      };

      fillParentOptions(cat.id);

      $("#Form_Name").val(cat.name);
      $("#Form_LatinName").val(cat.latinName);
      $("#Form_ParentId")
        .val(cat.parentId || "")
        .trigger("change");
      $("#Form_DisplayOrder").val(
        cat.id
          ? reorder.orderOf(cat.id, cat.displayOrder)
          : nextOrderFor(cat.parentId),
      );
      $("#Form_IsActive").prop("checked", cat.isActive);
    },

    // Collect the form into the category data object sent to the server.
    getData: function () {
      var parent = $("#Form_ParentId").val();
      return {
        name: $("#Form_Name").val().trim(),
        latinName: $("#Form_LatinName").val().trim(),
        displayOrder: parseInt($("#Form_DisplayOrder").val(), 10) || 0,
        parentId: parent === "" ? null : parent,
        isActive: $("#Form_IsActive").is(":checked"),
      };
    },

    validate: function () {
      var errors = [];
      var hasName = $("#Form_Name").val().trim().length > 0;
      var hasLatin = $("#Form_LatinName").val().trim().length > 0;

      if (!hasName) {
        errors.push("نام دسته‌بندی الزامی است.");
        $("#Form_Name").addClass("is-invalid");
      } else {
        $("#Form_Name").removeClass("is-invalid");
      }
      if (!hasLatin) {
        errors.push("نام لاتین الزامی است.");
        $("#Form_LatinName").addClass("is-invalid");
      } else {
        $("#Form_LatinName").removeClass("is-invalid");
      }
      return errors;
    },

    // Server handlers (saveCategory / deleteCategory refresh the rows after success).
    onSubmit: saveCategory,
    onDelete: deleteCategory,
  });

  //#region Drag & Drop Reordering
  // Renumbers the «ترتیب» badges and persists the new order after a drop.
  reorder = AdminCrud.bindRowReorder(page.table, {
    badge: ".cat-order",
    orderCol: 1,
    logLabel: "Reorder Categories",
    successMsg: "ترتیب دسته‌بندی‌ها با موفقیت ذخیره شد.",
    onSave: saveOrder,
  });
  //#endregion
}

// The parent select lives inside a bootstrap modal, so Select2 is initialised the first time
// the modal is laid out ("shown.bs.modal", once): attached to <body> it measures against the
// page and the list ends up in the top left corner instead of on the field. It is attached to
// the .modal element and not to .modal-content because the theme gives .modal-content
// "overflow: hidden", which would cut the list off as soon as it reaches the header.
$("#kt_modal_category").one("shown.bs.modal", function () {
  var $select = $("#Form_ParentId");

  $select.select2({
    dropdownParent: $("#kt_modal_category"),
    placeholder: "بدون والد (سطح اول)",
    allowClear: true,
    width: "100%",
    dropdownAutoWidth: false,
    dir: "rtl",
  });

  /**
   * Places the list above the field, with the same left edge as the field. This is a copy of
   * Select2's own AttachBody._positionDropdown (the function that writes the offsets on open,
   * on scroll and on resize) with the vertical anchor moved from the bottom of the control to
   * its top: overriding it - instead of repositioning after the fact - is what keeps the list
   * from being painted below the field for the first frame.
   */
  function positionAboveField() {
    var $parent = this.$dropdownParent;
    if ($parent.css("position") === "static") $parent = $parent.offsetParent();

    var control = this.$container.offset();
    var origin = $parent.offset();
    var height = this.$dropdown.outerHeight(false);

    this.$dropdownContainer.css({
      left: control.left - origin.left,
      top: control.top - origin.top - height - 2,
    });

    // The direction classes are what swap the rounded corners of the list and the box.
    this.$dropdown
      .removeClass("select2-dropdown--below")
      .addClass("select2-dropdown--above");
    this.$container
      .removeClass("select2-container--below")
      .addClass("select2-container--above");
  }

  var dropdown = $select.data("select2").dropdown;
  dropdown._positionDropdown = positionAboveField;
});
//#endregion

//#region Details Modal (all fields, audit and soft-delete included)
/** Writes one label/value row of the details tables; empty values show a dash. */
function setDetails(selector, value) {
  $(selector).text(
    value === null || value === undefined || value === "" ? "—" : value,
  );
}

/**
 * Fills the read-only details modal from the row cache — the listing already
 * carries every column the modal shows, so opening it costs no request. The
 * order comes from the badge because a drag & drop changes it on the table
 * before the server value reaches the browser again.
 */
function openDetailsModal(id) {
  var cat = rowsById[id];
  if (!cat) return;

  var order = reorder
    ? reorder.orderOf(id, cat.displayOrder)
    : cat.displayOrder;

  $("#cd-name").text(cat.name);
  $("#cd-latin-name").text(cat.latinName);
  setDetails("#cd-name-full", cat.name);
  setDetails("#cd-latin-name-full", cat.latinName);
  setDetails("#cd-display-order", order);
  setDetails("#cd-parent", cat.parentName);
  setDetails("#cd-depth", cat.depth);
  setDetails("#cd-status", cat.isActive ? "فعال" : "غیرفعال");
  setDetails("#cd-id", cat.id);
  setDetails("#cd-created-at", categoryDateTime(cat.createdAt));
  setDetails("#cd-created-by", cat.createdBy);
  setDetails("#cd-updated-at", categoryDateTime(cat.updatedAt));
  setDetails("#cd-updated-by", cat.updatedBy);
  setDetails("#cd-is-deleted", cat.isDeleted ? "حذف شده" : "خیر");
  setDetails("#cd-deleted-at", categoryDateTime(cat.deletedAt));
  setDetails("#cd-deleted-by", cat.deletedBy);

  // The modal has no row of its own to point back at, so the button closes over the id.
  $("#cd-edit")
    .off("click")
    .on("click", function () {
      AdminCrud.Utils.hideModal("kt_modal_category_details");
      openEditModal(id);
    });

  AdminCrud.Utils.showModal("kt_modal_category_details");
}
//#endregion

//#region Inline onclick Handlers (HTML compatibility)
// The markup calls these directly via onclick="..." attributes.

function openCreateModal() {
  if (page) page.openCreate();
}
function submitCategoryForm() {
  if (page) page.submit();
}
function confirmDelete() {
  if (page) page.confirmDelete();
}
function openDeleteModal(id, name) {
  if (page) page.openDelete(id, name);
}

/**
 * Edit: opens the modal from the row cache. The values come from the cache except
 * the display order, which comes from the badge — after a drag & drop the badge is
 * the number that was saved, and showing the old one reads as a broken reorder.
 */
function openEditModal(id) {
  var cat = rowsById[id];
  if (!cat || !page) return;

  page.state.mode = "edit";
  page.state.editId = id;
  $("#modal-category-title").text("ویرایش دسته‌بندی");

  fillParentOptions(id);

  $("#Form_Name").val(cat.name);
  $("#Form_LatinName").val(cat.latinName);
  $("#Form_ParentId")
    .val(cat.parentId || "")
    .trigger("change");
  $("#Form_DisplayOrder").val(reorder.orderOf(id, cat.displayOrder));
  $("#Form_IsActive").prop("checked", cat.isActive);

  AdminCrud.Utils.showModal("kt_modal_category");
}
//#endregion

//#region Boot
// Rows must be in the DOM before the DataTable initialises; at init the
// table registers its existing tbody rows automatically, so the rows must
// NOT be re-added with rows.add() or they end up duplicated in every page.
// The picker is read first: its field metadata decides which condition the
// very first request may already carry.
$(function () {
  $("#filterField").on("change", function () {
    renderOperators();
    renderValueControl();
  });
  $("#filterOperator").on("change", renderValueControl);

  loadFields()
    .fail(function () {
      toastr.error("خطا در دریافت فیلدهای قابل فیلتر.", "خطا");
    })
    .always(function () {
      loadRows()
        .then(function (rows) {
          $("#kt_categories_table tbody").append(rows);
          initPage();
          reorder.renumber();
        })
        .fail(function () {
          initPage(); // still initialise so the page stays usable
          toastr.error("خطا در دریافت لیست دسته‌بندی‌ها.", "خطا");
        });
    });
});
//#endregion
