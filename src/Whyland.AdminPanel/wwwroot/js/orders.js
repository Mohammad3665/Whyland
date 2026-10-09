//#region Orders Page Scripts
/**
 * Admin "Orders" list page (section 7.1 of the UI/UX doc):
 * read-only DataTable of Order entities with status filter, search and
 * an order-details view modal. No create/edit — orders are created by
 * the storefront, so the page only ships «مشاهده جزئیات».
 * Built on top of the shared AdminCrud module (admin-crud.js).
 */

//#region Table
var page = AdminCrud.createPage({
  table: {
    selector: "#kt_orders_table",
    options: {
      pageLength: 10,
    },
  },
  search: "#orderSearch",
  filters: [
    {
      el: "#orderStatusFilter",
      test: AdminCrud.Filters.dataEquals("status"),
    },
  ],
  // No create/edit/delete modals on this page: order details only (7.1).
  modal: {
    id: "kt_modal_order_details",
    titleEl: "#details-order-number",
    submitBtn: "kt_modal_order_details_close_btn",
  },
  deleteModal: {
    id: "kt_modal_order_details",
    nameEl: "#details-order-number",
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

//#region Details Modal (7.2 layout, opened from the 7.1 actions cell)
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

/** Status badge matching the table's badge classes. */
function statusBadge(label, type) {
  return '<span class="badge badge-light-' + type + '">' + label + "</span>";
}

var STATUS_BADGES = {
  "پرداخت شده": "success",
  "در انتظار پرداخت": "warning",
  "لغو شده": "dark",
  ناموفق: "danger",
};

/**
 * Opens the order-details modal. Kept inline-onclick friendly; all data
 * is passed from the table row (demo pattern used by the other pages):
 *
 * openDetailsModal(
 *   orderNo, userName, status, createdAt, paidAt, expiresAt,
 *   couponCode, couponAmount, totalAmount,
 *   paymentMethod, paymentStatus, trackingNumber, paymentAmount,
 *   itemsString // "name|qty|unit|total^..." — ^ separated
 * )
 */
function openDetailsModal(
  orderNo,
  userName,
  status,
  createdAt,
  paidAt,
  expiresAt,
  couponCode,
  couponAmount,
  totalAmount,
  paymentMethod,
  paymentStatus,
  trackingNumber,
  paymentAmount,
  itemsString,
) {
  //#region Order info
  $("#details-order-number").text(orderNo);
  $("#details-user-name").text(userName);
  $("#details-status").html(
    statusBadge(status, STATUS_BADGES[status] || "secondary"),
  );
  $("#details-created-at").text(faDigits(createdAt));

  // PaidAt only exists for paid orders; expire date only for Pending.
  if (paidAt) {
    $("#details-paid-at").text(faDigits(paidAt));
  } else {
    $("#details-paid-at").html('<span class="text-muted">—</span>');
  }

  if (status === "در انتظار پرداخت" && expiresAt) {
    $("#details-expires-wrapper").removeClass("d-none");
    $("#details-expires-at").text(faDigits(expiresAt));
  } else {
    $("#details-expires-wrapper").addClass("d-none");
  }
  //#endregion

  //#region Order items
  var $body = $("#details-items-body").empty();
  var items = (itemsString || "")
    .split("^")
    .filter(function (row) {
      return row.trim().length > 0;
    })
    .map(function (row) {
      var parts = row.split("|");
      return {
        name: parts[0] || "",
        qty: parseInt(parts[1], 10) || 1,
        unit: parseInt(parts[2], 10) || 0,
        total: parseInt(parts[3], 10) || 0,
      };
    });

  if (!items.length) {
    $body.html(
      '<tr><td colspan="4" class="text-center text-muted py-4">آیتمی ثبت نشده است</td></tr>',
    );
  } else {
    items.forEach(function (item) {
      $body.append(
        $("<tr>")
          .append(
            $("<td>", {
              class: "text-start ps-0 text-gray-800 fw-semibold",
            }).text(item.name),
          )
          .append($("<td>", { class: "text-center" }).text(faDigits(item.qty)))
          .append(
            $("<td>", {
              class: "text-end text-gray-700",
              dir: "ltr",
            }).text(formatToman(item.unit).replace(" تومان", "")),
          )
          .append(
            $("<td>", {
              class: "text-end pe-0 text-gray-800 fw-bold",
              dir: "ltr",
            }).text(formatToman(item.total).replace(" تومان", "")),
          ),
      );
    });
  }
  //#endregion

  //#region Discount block
  if (couponCode) {
    $("#details-coupon-code").html(
      '<span class="badge badge-light-success" dir="ltr">' +
        couponCode +
        "</span>",
    );
    $("#details-coupon-amount").text(formatToman(couponAmount));
  } else {
    $("#details-coupon-code").html(
      '<span class="text-muted">بدون کد تخفیف</span>',
    );
    $("#details-coupon-amount").html('<span class="text-muted">—</span>');
  }
  //#endregion

  //#region Payment block
  if (paymentMethod) {
    $("#details-payment-method").text(paymentMethod);
    $("#details-payment-status").html(
      statusBadge(
        paymentStatus,
        paymentStatus === "موفق" ? "success" : "danger",
      ),
    );
    $("#details-payment-amount").text(formatToman(paymentAmount));
    $("#details-tracking-number").text(trackingNumber || "—");
  } else {
    // Pending / Cancelled / Failed orders may have no payment attempt.
    $("#details-payment-method").html('<span class="text-muted">—</span>');
    $("#details-payment-status").html('<span class="text-muted">—</span>');
    $("#details-payment-amount").html('<span class="text-muted">—</span>');
    $("#details-tracking-number").html('<span class="text-muted">—</span>');
  }
  //#endregion

  //#region Total
  $("#details-total-amount").text(formatToman(totalAmount));
  //#endregion

  AdminCrud.Utils.showModal("kt_modal_order_details");
}
//#endregion
//#endregion
