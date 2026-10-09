//#region Coupons Page Scripts
/**
 * Admin "Coupons" list page (sections 9.1–9.3 of the UI/UX doc):
 * table of Coupon entities, create/edit in a modal (9.2), deactivate/
 * activate toggle and soft-delete confirmation (9.3).
 * The derived status (Active / Scheduled / Expired / Exhausted /
 * Inactive) comes precomputed on each demo row via data-status; when the
 * data layer is wired up, reuse deriveStatus() server-side or recompute
 * it here after each save.
 * Built on top of the shared AdminCrud module (admin-crud.js).
 */

//#region Derived Status (9.1: Active / Scheduled / Expired / Exhausted / Inactive)
/**
 * Computes the derived status from the raw coupon fields.
 * Uses ISO (yyyy/mm/dd) numeric comparison — the demo stores Jalali
 * dates in ascending lexicographic/numeric compatible form.
 */
function deriveStatus(isActive, startDate, endDate, usageLimit, usedCount) {
  if (!isActive) return "Inactive";
  var today = "1403/07/15"; // demo "now" injalali; replace with server date
  if (startDate > today) return "Scheduled";
  if (endDate < today) return "Expired";
  if (usageLimit && usedCount >= usageLimit) return "Exhausted";
  return "Active";
}

var STATUS_BADGE = {
  Active: '<span class="badge badge-light-success">فعال</span>',
  Scheduled: '<span class="badge badge-light-primary">زمان‌بندی‌شده</span>',
  Expired: '<span class="badge badge-light-dark">منقضی</span>',
  Exhausted: '<span class="badge badge-light-warning">اتمام سهمیه</span>',
  Inactive: '<span class="badge badge-light-secondary">غیرفعال</span>',
};
//#endregion

//#region Form Helpers
/** Persians digits for the readonly UsedCount display. */
function faDigits(n) {
  var fa = "۰۱۲۳۴۵۶۷۸۹";
  return String(n).replace(/\d/g, function (d) {
    return fa[+d];
  });
}

/**
 * Populates the form fields from a Coupon object (null = create mode).
 * The row carries its raw fields in data-* attributes.
 */
function fillCouponForm(coupon) {
  coupon = coupon || {};
  $("#Form_Code").val(coupon.code || "");
  $("#Form_Type").val(coupon.type || "Percentage");
  $("#Form_Value").val(coupon.value || "");
  $("#Form_MinOrderAmount").val(coupon.min || "");
  $("#Form_UsageLimit").val(coupon.limit || "");

  // UsedCount is read-only (9.2) and hidden in create mode.
  var used = coupon.id ? coupon.used || 0 : 0;
  $("#Form_UsedCount").val(used);
  $("#CouponUsedCountGroup").toggleClass("d-none", !coupon.id);

  $("#Form_StartDate").val(coupon.start || "");
  $("#Form_EndDate").val(coupon.end || "");
  $("#Form_IsActive").prop("checked", coupon.active !== false);

  updateValueUnit();
}

/** Percentage ↔ Toman unit + hint swap on the Value field. */
function updateValueUnit() {
  var isPercentage = $("#Form_Type").val() === "Percentage";
  $("#Form_ValueUnit").text(isPercentage ? "%" : "تومان");
  $("#Form_ValueHint").text(
    isPercentage
      ? "درصد تخفیف (۱ تا ۹۹)."
      : "مبلغ ثابت تخفیف به تومان.",
  );
}
//#endregion

//#region CRUD Page Configuration
var page = AdminCrud.createPage({
  table: {
    selector: "#kt_coupons_table",
    options: {
      columnDefs: [{ orderable: false, targets: [-1] }],
    },
  },
  search: "#couponSearch",
  filters: [
    {
      el: "#couponStatusFilter",
      test: AdminCrud.Filters.dataEquals("status"),
    },
  ],
  modal: {
    id: "kt_modal_coupon",
    titleEl: "#modal-coupon-title",
    submitBtn: "kt_coupon_submit",
  },
  deleteModal: {
    id: "kt_modal_delete_coupon",
    nameEl: "#delete-coupon-name",
  },
  texts: {
    createTitle: "افزودن کد تخفیف",
    editTitle: "ویرایش کد تخفیف",
    created: "کد تخفیف با موفقیت ایجاد شد.",
    updated: "تغییرات با موفقیت ذخیره شد.",
    deleted: "کد تخفیف با موفقیت حذف شد.",
  },

  // Convert the row's data-* attributes into the shape fillCouponForm uses.
  loadItem: function (id) {
    var $row = $('tr[data-id="' + id + '"]');
    return $row.length
      ? {
          id: id,
          code: $row.data("code"),
          type: $row.data("type"),
          value: $row.data("value"),
          min: $row.data("min") || "",
          limit: $row.data("limit") || "",
          used: $row.data("used"),
          start: $row.data("start"),
          end: $row.data("end"),
          active: String($row.data("active")) === "true",
          deletable: String($row.data("deletable")) === "true",
        }
      : null;
  },

  fillForm: fillCouponForm,

  // Collect the form into the Coupon data object sent to the server.
  getData: function () {
    return {
      id: page.state.editId,
      code: $("#Form_Code").val().trim().toUpperCase(),
      type: $("#Form_Type").val(),
      value: $("#Form_Value").val().trim().replace(/[^\d.]/g, ""),
      minOrderAmount: $("#Form_MinOrderAmount").val().replace(/[^\d]/g, ""),
      usageLimit: $("#Form_UsageLimit").val().replace(/[^\d]/g, ""),
      startDate: $("#Form_StartDate").val().trim(),
      endDate: $("#Form_EndDate").val().trim(),
      isActive: $("#Form_IsActive").prop("checked"),
      // Derived status is recomputed after save in a real integration.
    };
  },

  //#region Validation (entity rules from the entities doc)
  validate: function () {
    var errors = [];
    var $code = $("#Form_Code");
    var $value = $("#Form_Value");
    var $start = $("#Form_StartDate");
    var $end = $("#Form_EndDate");

    // Code: required, no spaces (matched case-insensitively).
    var code = $code.val().trim();
    if (!code) {
      errors.push("کد تخفیف الزامی است.");
      $code.addClass("is-invalid");
    } else if (/\s/.test(code)) {
      errors.push("کد تخفیف نباید شامل فاصله باشد.");
      $code.addClass("is-invalid");
    } else {
      $code.removeClass("is-invalid");
    }

    // Value: > 0; < 100 for Percentage.
    var value = parseFloat($value.val());
    var isPercentage = $("#Form_Type").val() === "Percentage";
    if (!value || value <= 0) {
      errors.push("مقدار تخفیف باید بزرگ‌تر از صفر باشد.");
      $value.addClass("is-invalid");
    } else if (isPercentage && value >= 100) {
      errors.push("درصد تخفیف باید کمتر از ۱۰۰ باشد.");
      $value.addClass("is-invalid");
    } else {
      $value.removeClass("is-invalid");
    }

    // Dates: both required, EndDate > StartDate.
    var start = $start.val().trim();
    var end = $end.val().trim();
    if (!start) {
      errors.push("تاریخ شروع الزامی است.");
      $start.addClass("is-invalid");
    } else {
      $start.removeClass("is-invalid");
    }
    if (!end) {
      errors.push("تاریخ پایان الزامی است.");
      $end.addClass("is-invalid");
    } else if (start && end <= start) {
      errors.push("تاریخ پایان باید بعد از تاریخ شروع باشد.");
      $end.addClass("is-invalid");
    } else {
      $end.removeClass("is-invalid");
    }

    // UsageLimit (optional) must be an integer > 0.
    var limit = $("#Form_UsageLimit").val().replace(/[^\d]/g, "");
    if (limit !== "" && parseInt(limit, 10) <= 0) {
      errors.push("محدودیت استفاده باید عددی بزرگ‌تر از صفر باشد.");
      $("#Form_UsageLimit").addClass("is-invalid");
    } else {
      $("#Form_UsageLimit").removeClass("is-invalid");
    }

    return errors;
  },
  //#endregion
});

// The PageModel keeps state private to createPage; capture it for helpers.
var state = page.state;
var editState = {}; // raw fields of the row being edited
//#endregion

//#region Inline onclick Handlers (HTML compatibility)
function openCreateModal() {
  editState = {};
  page.openCreate();
}
function submitCouponForm() {
  page.submit();
}

/** Deactivate / activate toggle from the actions column. */
function toggleActive(id) {
  var $row = $('tr[data-id="' + id + '"]');
  var isActive = String($row.data("active")) === "true";
  $('[data-id="' + id + '"] td[data-active="true"] .badge')[0];
  var $badgeCell = $row.find("td[data-active]");
  var $toggle = $row.find('button[onclick*="toggleActive"]');

  // Demo: flip the row state; TODO POST ?handler=ToggleActive&id=
  $row.data("active", !isActive);
  $row.attr("data-status", !isActive ? "Active" : "Inactive");
  $badgeCell.html(STATUS_BADGE[!isActive ? "Active" : "Inactive"]);

  // Swap the toggle icon/title: disable when active, enable when not.
  $toggle
    .toggleClass("btn-active-color-warning", isActive)
    .toggleClass("btn-active-color-success", !isActive)
    .attr("title", isActive ? "فعال‌سازی" : "غیرفعال‌سازی");

  toastr.success(
    isActive ? "کد تخفیف غیرفعال شد." : "کد تخفیف فعال شد.",
    "موفق",
  );
}
//#endregion

//#region Delete (9.3: soft-delete + keep-used guard)
function openDeleteModal(id) {
  var item = page.state && page.state.deleteId === id ? null : null;
  var coupon = readRow(id);
  if (!coupon) return;

  // Never physically remove a coupon that has been used (9.3); the demo
  // backend must soft delete instead — the modal reflects that.
  if (!coupon.deletable) {
    $("#delete-coupon-note").html(
      "این کد قبلاً استفاده شده است؛ حذف به‌صورت نرم انجام می‌شود و از " +
        "سفارش‌های قبلی حذف نمی‌شود.",
    );
  } else {
    $("#delete-coupon-note").text("این کد تاکنون استفاده نشده است.");
  }

  $("#delete-coupon-name").text(coupon.code);
  state.deleteId = id;
  AdminCrud.Utils.showModal("kt_modal_delete_coupon");
}

function confirmDelete() {
  page.confirmDelete();
}
//#endregion

//#region Row Reader (demo)
/** Reads the demo row's raw fields (same shape as loadItem). */
function readRow(id) {
  var $row = $('tr[data-id="' + id + '"]');
  if (!$row.length) return null;
  return {
    id: id,
    code: $row.data("code"),
    type: $row.data("type"),
    value: $row.data("value"),
    min: $row.data("min") || "",
    limit: $row.data("limit") || "",
    used: $row.data("used"),
    start: $row.data("start"),
    end: $row.data("end"),
    active: String($row.data("active")) === "true",
    deletable: String($row.data("deletable")) === "true",
  };
}
//#endregion

//#region Edit From Row Data
/** Edit: the AdminCrud openEdit(id) uses loadItem(id) above. */
function openEditModal(id) {
  editState = readRow(id) || {};
  page.openEdit(id);
}
//#endregion

//#region Type change hook
$(document).on("change", "#Form_Type", updateValueUnit);
//#endregion
//#endregion
