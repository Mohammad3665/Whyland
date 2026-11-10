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
var picker; // the generic filter picker (AdminTable.filterPicker)

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
/** Saves the create/edit modal through ?handler=Create / ?handler=Update. */
function saveCategory(data, mode) {
  return AdminTable.postHandler(
    mode === "create" ? "Create" : "Update",
    data,
  ).done(refreshRows);
}

/** Deletes a category by id through ?handler=Delete. */
function deleteCategory(id) {
  return AdminTable.postHandler("Delete", { id: id }).done(refreshRows);
}

/**
 * Persists the visual order produced by drag & drop through ?handler=Reorder.
 * The server numbers every sibling group separately, so the client sends the
 * sequence only and never a group key.
 */
function saveOrder(rows) {
  $.ajax({
    url: AdminTable.handlerUrl("Reorder"),
    type: "POST",
    contentType: "application/json; charset=UTF-8",
    data: JSON.stringify(rows),
    headers: { RequestVerificationToken: AdminTable.csrfToken() },
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

//#region Generic Filter Picker (the shared AdminTable implementation)
/**
 * Fields, operators, the value control, chips, range checks and the Select2 dressing of every box
 * all come from common/admin-table.js, so a filter row on this page behaves exactly like the one on
 * the accounts and the roles pages. The only thing the category page decides is how a change
 * reaches the table: the whole tree has to stay in the DOM for drag &amp; drop, so the rows are
 * reloaded from ?handler=Rows instead of being redrawn as a server-side page.
 */
function loadFields() {
  picker = AdminTable.filterPicker({
    fieldEl: "#filterField",
    operatorEl: "#filterOperator",
    valueWrapEl: "#filterValueWrap",
    chipsEl: "#filterChips",
    onChange: refreshRows,
  });

  return picker.load();
}

/** The chips plus the value currently typed, as the row request needs them. */
function activeConditions() {
  return picker.conditions();
}

/** The two buttons of the filter row call these from the markup. */
function addFilterCondition() {
  picker.add();
}

function clearFilterConditions() {
  picker.clear();
}
//#endregion

//#region Row Rendering (from server DTOs)

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
      $('<span class="text-gray-600">').text(AdminTable.faDate(cat.createdAt)),
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
    params["Filters[" + i + "].Value"] = active[i].value;
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
    if (isFull) note += " (پر: " + AdminTable.faDigits(MAX_CHILDREN) + " فرزند)";

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
// page and the list ends up in the top left corner instead of on the field. AdminTable.modalSelect2
// is the shared modal setup — .modal-content is the dropdown parent and the list opens above the
// box, pinned to the field's own edges.
$("#kt_modal_category").one("shown.bs.modal", function () {
  AdminTable.modalSelect2($("#Form_ParentId"), {
    dropdownParent: $("#kt_modal_category .modal-content"),
    placeholder: "بدون والد (سطح اول)",
  });
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
  setDetails("#cd-created-at", AdminTable.faDateTime(cat.createdAt));
  // The author names the row already carries, not the guids: an operator cannot read a person off a key.
  setDetails("#cd-created-by", cat.createdByName);
  setDetails("#cd-updated-at", AdminTable.faDateTime(cat.updatedAt));
  setDetails("#cd-updated-by", cat.updatedByName);
  setDetails("#cd-is-deleted", cat.isDeleted ? "حذف شده" : "خیر");
  setDetails("#cd-deleted-at", AdminTable.faDateTime(cat.deletedAt));
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
