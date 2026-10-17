//#region FAQs Page Scripts
/**
 * Admin "FAQs" list page (sections 11.1 / 11.2 of the UI/UX doc):
 * public website frequently asked questions with drag & drop reordering
 * (DataTables RowReorder), status filter, create/edit modal and delete
 * confirmation. Built on top of the shared AdminCrud module (admin-crud.js)
 * and wired to the Razor Page handlers:
 *   GET  ?handler=Rows     → JSON rows
 *   POST ?handler=Create   → form-encoded create
 *   POST ?handler=Update   → form-encoded update
 *   POST ?handler=Delete   → form-encoded delete (id)
 *   POST ?handler=Reorder  → JSON [{ id, displayOrder }]
 * POSTs carry the antiforgery token rendered by @Html.AntiForgeryToken().
 */

var page;    // shared CRUD page controller (set in initPage below)
var reorder; // shared drag & drop ordering controller (set below)

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
function saveFaq(data, mode) {
  return postHandler(mode === "create" ? "Create" : "Update", data).done(
    refreshRows,
  );
}

/** Deletes a FAQ by id through ?handler=Delete. */
function deleteFaq(id) {
  return postHandler("Delete", { id: id }).done(refreshRows);
}

/** Persists the visual order produced by drag & drop through ?handler=Reorder. */
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
        toastr.success("ترتیب سوالات متداول با موفقیت ذخیره شد.", "موفق");
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

//#region Row Rendering (from server DTOs)
/** Formats a UTC timestamp as a localized Persian date for the table. */
function faqDate(createdAt) {
  var d = new Date(createdAt);
  return isNaN(d.getTime()) ? "" : d.toLocaleDateString("fa-IR");
}

/**
 * Builds one <tr> exactly like the original demo markup (responsive cell,
 * drag handle with order badge, title, status badge, creation date and the
 * row actions) but binds the handlers with closures so user data never
 * lands inside inline onclick strings. Returns the plain DOM node.
 */
function buildFaqRow(faq) {
  var $row = $(
    '<tr><td class="dtr-control-col"></td>' +
      '<td tabindex="0" class="reorder text-center dt-type-numeric dtr-control">' +
      '<div class="d-flex align-items-center justify-content-center">' +
      '<i class="ki-duotone ki-burger-menu-2 fs-2 text-gray-400 cursor-move me-2">' +
      '<span class="path1"></span><span class="path2"></span><span class="path3"></span>' +
      '<span class="path4"></span><span class="path5"></span><span class="path6"></span>' +
      '<span class="path7"></span><span class="path8"></span><span class="path9"></span><span class="path10"></span>' +
      "</i>" +
      '<span class="faq-order badge badge-light-secondary"></span>' +
      "</div></td>" +
      "<td></td>" +
      '<td class="text-center"></td>' +
      "<td></td>" +
      '<td class="text-end nowrap-cell"></td>' +
      "</tr>",
  );

  $row.attr("data-id", faq.id).attr("data-active", faq.isActive ? 1 : 0);

  var cells = $row.find("td");
  $(cells[1]).find(".faq-order").text(faq.displayOrder);

  $(cells[2]).append(
    $('<span class="text-gray-800 fw-bold">').text(faq.title),
  );

  $(cells[3])
    .attr("data-order", faq.isActive ? 1 : 0)
    .append(
      $(
        '<span class="badge badge-light-' +
          (faq.isActive ? "success" : "danger") +
          '">',
      ).text(faq.isActive ? "فعال" : "غیرفعال"),
    );

  $(cells[4])
    .attr("data-order", faq.createdAt)
    .append($('<span class="text-gray-600">').text(faqDate(faq.createdAt)));

  var $details = $(
    '<button type="button" class="btn btn-sm btn-icon btn-bg-light btn-active-color-info me-1" title="جزئیات">',
  ).append('<i class="ki-duotone ki-eye fs-2"><span class="path1"></span><span class="path2"></span></i>');
  $details.on("click", function () {
    openDetailsModal(faq.title, faq.description);
  });

  var $edit = $(
    '<button type="button" class="btn btn-sm btn-icon btn-bg-light btn-active-color-primary me-1" title="ویرایش">',
  ).append('<i class="ki-duotone ki-pencil fs-2"><span class="path1"></span><span class="path2"></span></i>');
  $edit.on("click", function () {
    openEditModal(
      faq.id,
      faq.title,
      faq.description,
      faq.displayOrder,
      faq.isActive,
    );
  });

  var $delete = $(
    '<button type="button" class="btn btn-sm btn-icon btn-bg-light btn-active-color-danger" title="حذف">',
  ).append(
    '<i class="ki-duotone ki-trash fs-2"><span class="path1"></span><span class="path2"></span><span class="path3"></span><span class="path4"></span><span class="path5"></span></i>',
  );
  $delete.on("click", function () {
    if (page) page.openDelete(faq.id, faq.title);
  });

  $(cells[5]).append($details, $edit, $delete);

  return $row[0];
}

/**
 * Fetches the full row set from ?handler=Rows and builds the <tr> nodes;
 * rows arrive sorted by DisplayOrder.
 */
function loadRows() {
  return $.getJSON(location.pathname + "?handler=Rows").then(function (res) {
    var data = (res && res.data) || [];
    var nodes = [];
    for (var i = 0; i < data.length; i++) nodes.push(buildFaqRow(data[i]));
    return nodes;
  });
}

/**
 * Reloads from the server and reinjects every row into the (already
 * initialised) DataTable; used after every successful save/delete.
 */
function refreshRows() {
  return loadRows().then(function (rows) {
    $("#kt_faqs_table tbody").empty().append(rows);
    page.table.clear();
    page.table.rows.add(rows).draw();
    reorder.renumber();
  });
}
//#endregion

//#region CRUD Page Configuration
function initPage() {
  page = AdminCrud.createPage({
    table: {
      selector: "#kt_faqs_table",
      options: $.extend(true, AdminCrud.rowReorderOptions(), {
        responsive: {
          breakpoints: [
            { name: "desktop", width: 1260 },
            { name: "tablet-l", width: 960 },
            { name: "tablet", width: 930 },
            { name: "phone", width: 576 },
          ],
        },
        columnDefs: [
          { orderable: false, targets: [0, 1, 5] },
          { className: "all", targets: 1 },
          { className: "min-tablet", targets: [3, 4, 5] },
          { className: "dtr-control", targets: 0 },
        ],
      }),
    },
    search: "#faqSearch",
    filters: [
      {
        // Filter on the «وضعیت» cell (data-active on the <tr>).
        el: "#statusFilter",
        test: AdminCrud.Filters.dataEquals("active"),
      },
    ],
    modal: {
      id: "kt_modal_faq",
      titleEl: "#modal-faq-title",
      submitBtn: "kt_modal_faq_submit",
    },
    deleteModal: {
      id: "kt_modal_delete_faq",
      nameEl: "#delete-faq-name",
    },
    texts: {
      createTitle: "افزودن سوال متداول",
      editTitle: "ویرایش سوال متداول",
      created: "سوال متداول با موفقیت ایجاد شد.",
      updated: "تغییرات با موفقیت ذخیره شد.",
      deleted: "سوال متداول با موفقیت حذف شد.",
    },

    // Populate the form fields from a FAQ object (null = create mode).
    fillForm: function (faq) {
      faq = faq || {
        title: "",
        description: "",
        displayOrder: 0,
        isActive: true,
      };
      $("#Form_Title").val(faq.title);
      $("#Form_Description").val(faq.description);
      $("#Form_DisplayOrder").val(
        faq.displayOrder > 0 ? faq.displayOrder : reorder.getNextOrder(),
      );
      $("#Form_IsActive").prop("checked", faq.isActive);
    },

    // Collect the form into the FAQ data object sent to the server.
    getData: function () {
      return {
        title: $("#Form_Title").val().trim(),
        description: $("#Form_Description").val().trim(),
        displayOrder: parseInt($("#Form_DisplayOrder").val(), 10) || 0,
        isActive: $("#Form_IsActive").is(":checked"),
      };
    },

    validate: function () {
      var errors = [];
      var hasTitle = $("#Form_Title").val().trim().length > 0;
      var hasDescription = $("#Form_Description").val().trim().length > 0;

      if (!hasTitle) {
        errors.push("عنوان سوال الزامی است.");
        $("#Form_Title").addClass("is-invalid");
      } else {
        $("#Form_Title").removeClass("is-invalid");
      }
      if (!hasDescription) {
        errors.push("پاسخ (توضیحات) الزامی است.");
        $("#Form_Description").addClass("is-invalid");
      } else {
        $("#Form_Description").removeClass("is-invalid");
      }
      return errors;
    },

    // Server handlers (saveFaq / deleteFaq refresh the rows after success).
    onSubmit: saveFaq,
    onDelete: deleteFaq,
  });

  //#region Drag & Drop Reordering
  // Renumbers the «ترتیب» badges and persists the new order after a drop.
  reorder = AdminCrud.bindRowReorder(page.table, {
    badge: ".faq-order",
    orderCol: 1,
    logLabel: "Reorder FAQs",
    successMsg: "ترتیب سوالات متداول با موفقیت ذخیره شد.",
    onSave: saveOrder,
  });
  //#endregion
}
//#endregion

//#region Details Modal (read-only answer preview)
/**
 * Shows the long answer in a read-only modal; avoids truncating the
 * توضیحات column in the table.
 */
function openDetailsModal(title, description) {
  $("#details-faq-title").text(title);
  $("#details-faq-description").text(description);
  AdminCrud.Utils.showModal("kt_modal_faq_details");
}
//#endregion

//#region Inline onclick Handlers (HTML compatibility)
// The markup calls these directly via onclick="..." attributes.

function openCreateModal() {
  if (page) page.openCreate();
}
function submitFaqForm() {
  if (page) page.submit();
}
function confirmDelete() {
  if (page) page.confirmDelete();
}
function openDeleteModal(id, name) {
  if (page) page.openDelete(id, name);
}
//#endregion

//#region Edit From Row Data
/**
 * Edit: keeps the old inline onclick signature
 * (id, title, description, displayOrder, isActive);
 * the values are passed in directly from the table row.
 */
function openEditModal(id, title, description, displayOrder, isActive) {
  if (!page) return;
  page.state.mode = "edit";
  page.state.editId = id;
  $("#modal-faq-title").text("ویرایش سوال متداول");
  $("#Form_Title").val(title);
  $("#Form_Description").val(description);
  $("#Form_DisplayOrder").val(displayOrder);
  $("#Form_IsActive").prop("checked", isActive);
  AdminCrud.Utils.showModal("kt_modal_faq");
}
//#endregion

//#region Boot
// Rows must be in the DOM before the DataTable initialises; at init the
// table registers its existing tbody rows automatically, so the rows must
// NOT be re-added with rows.add() or they end up duplicated in every page.
$(function () {
  loadRows()
    .then(function (rows) {
      $("#kt_faqs_table tbody").append(rows);
      initPage();
      reorder.renumber();
    })
    .fail(function () {
      initPage(); // still initialise so the page stays usable
      toastr.error("خطا در دریافت لیست سوالات.", "خطا");
    });
});
//#endregion
