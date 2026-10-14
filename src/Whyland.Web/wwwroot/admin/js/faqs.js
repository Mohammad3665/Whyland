//#region FAQs Page Scripts
/**
 * Admin "FAQs" list page (sections 11.1 / 11.2 of the UI/UX doc):
 * public website frequently asked questions with drag & drop reordering
 * (DataTables RowReorder), status filter, create/edit modal and delete
 * confirmation. Built on top of the shared AdminCrud module (admin-crud.js).
 */

var reorder; // shared drag & drop ordering controller (set below)

//#region CRUD Page Configuration
var page = AdminCrud.createPage({
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
});
//#endregion

//#region Drag & Drop Reordering
// Renumbers the «ترتیب» badges and persists the new order after a drop.
// TODO: pass onSave here (POST [{ id, displayOrder }] to ?handler=Reorder)
// once the data layer is connected.
reorder = AdminCrud.bindRowReorder(page.table, {
  badge: ".faq-order",
  orderCol: 1,
  logLabel: "Reorder FAQs",
  successMsg: "ترتیب سوالات متداول با موفقیت ذخیره شد.",
});
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
  page.openCreate();
}
function submitFaqForm() {
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
 * Edit: keeps the old inline onclick signature
 * (id, title, description, displayOrder, isActive);
 * the values are passed in directly from the table row.
 */
function openEditModal(id, title, description, displayOrder, isActive) {
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
//#endregion
