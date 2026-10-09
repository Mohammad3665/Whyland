//#region Payments Page Scripts
/**
 * Admin "Payments" list page (sections 8.1 / 8.2 of the UI/UX doc):
 * read-only DataTable of Payment entities with status + payment-method
 * filters and a payment-details modal. No create/edit — payments are
 * created by the payment flow, so the page only ships «مشاهده جزئیات».
 * Built on top of the shared AdminCrud module (admin-crud.js).
 */

//#region Table
var page = AdminCrud.createPage({
  table: {
    selector: "#kt_payments_table",
    options: {
      columnDefs: [{ orderable: false, targets: [-1] }],
    },
  },
  search: "#paymentSearch",
  filters: [
    {
      el: "#paymentStatusFilter",
      test: AdminCrud.Filters.dataEquals("status"),
    },
    // Method values contain Persian characters; match the data-method attr.
    {
      el: "#paymentMethodFilter",
      test: AdminCrud.Filters.dataEquals("method"),
    },
  ],
  // View-only page: the detail modal doubles as modal/deleteModal stubs
  // so the shared controller keeps working without CRUD endpoints.
  modal: {
    id: "kt_modal_payment_details",
    titleEl: "#payment-details-tracking",
    submitBtn: "payment_details_back_btn",
  },
  deleteModal: {
    id: "kt_modal_payment_details",
    nameEl: "#payment-details-tracking",
  },
  texts: {
    createTitle: "",
    editTitle: "",
    created: "",
    updated: "",
    deleted: "",
  },
  fillForm: function () {},
  getData: function () {
    return {};
  },
});
//#endregion

//#region Details Modal (8.2, opened from the 8.1 actions cell)
/** Persian digits for money/date display, matching the table's style. */
function faDigits(text) {
  var fa = "۰۱۲۳۴۵۶۷۸۹";
  return String(text).replace(/\d/g, function (d) {
    return fa[+d];
  });
}

/** Formats a raw integer with thousands separators + تومان. */
function formatToman(amount) {
  if (!amount) return "—";
  return faDigits(Number(amount).toLocaleString("en-US")) + " تومان";
}

/** Status badge matching the table's badge classes ("label|type"). */
function statusBadge(labelType) {
  var parts = (labelType || "").split("|");
  if (!parts[0]) return '<span class="text-muted">—</span>';
  return '<span class="badge badge-light-' + (parts[1] || "secondary") + '">' + parts[0] + "</span>";
}

/**
 * Opens the payment-details modal. Kept inline-onclick friendly; all
 * data is passed from the table row (demo pattern used by other pages):
 *
 * openDetailsModal(
 *   orderNo, userName, method, status("موفق"|...), amount,
 *   trackingNo, createdAt, paidAt,
 *   orderStatusBadge("پرداخت شده|success"|...), orderAmount, orderCreatedAt
 * )
 */
function openDetailsModal(
  orderNo,
  userName,
  method,
  status,
  amount,
  trackingNo,
  createdAt,
  paidAt,
  orderStatusBadge,
  orderAmount,
  orderCreatedAt,
) {
  //#region Payment info
  $("#payment-details-order-no").text(orderNo);
  $("#payment-details-user").text(userName);
  $("#payment-details-method").text(method);
  $("#payment-details-amount").text(formatToman(amount));

  var badges = {
    "موفق": "success",
    "در انتظار": "warning",
    "ناموفق": "danger",
    "لغو شده": "dark",
  };
  $("#payment-details-status").html(
    '<span class="badge badge-light-' + (badges[status] || "secondary") + '">' + status + "</span>",
  );

  $("#payment-details-tracking").text(trackingNo || "");
  if (trackingNo) {
    $("#payment-details-tracking-no").text(trackingNo);
  } else {
    $("#payment-details-tracking-no").html('<span class="text-muted">—</span>');
  }

  $("#payment-details-created").text(faDigits(createdAt));

  if (paidAt) {
    $("#payment-details-paid").text(faDigits(paidAt));
  } else {
    $("#payment-details-paid").html('<span class="text-muted">—</span>');
  }
  //#endregion

  //#region Linked order
  $("#payment-details-order-status").html(statusBadge(orderStatusBadge));
  $("#payment-details-order-amount").text(orderAmount || "—");
  $("#payment-details-order-created").text(faDigits(orderCreatedAt));
  //#endregion

  AdminCrud.Utils.showModal("kt_modal_payment_details");
}
//#endregion
//#endregion
