//#region Courses Page Scripts
/**
 * Admin "Courses" list page: DataTable, filters and the full-screen
 * create/edit modal (including the TinyMCE rich-text description editor).
 * Built on top of the shared AdminCrud module (admin-crud.js).
 */

//#region Imports
var U = AdminCrud.Utils;
var F = AdminCrud.Filters;
//#endregion//#region Description Editor (shared generic module)
/**
 * The TinyMCE wrapper for the "Full description" field lives in the
 * shared HtmlEditor module (js/common/html-editor.js) so other pages
 * (Blog Posts, ...) reuse the same generic code instead of duplicating
 * it. This page only creates the instance and drives its lifecycle.
 */
var HtmlEditor_ = window.HtmlEditor;
var descriptionHtmlEditor = HtmlEditor_.create({
  selector: "#Form_Description",
  height: 520,
  minHeight: 420,
});
// Aliases kept for the existing call sites below.
function initDescriptionEditor() {
  descriptionHtmlEditor.init();
}
function setDescription(html) {
  descriptionHtmlEditor.set(html);
}
function getDescription() {
  return descriptionHtmlEditor.get();
}
var slugTouched = false; // True once the user edits the slug manually
//#endregion

//#region Pricing / Image Field Helpers
/** Enables or disables the discount value field and updates its unit label. */
function setDiscountState(type) {
  var $v = $("#Form_DiscountValue");
  if (!type) {
    $v.val("").prop("disabled", true);
    $("#discountUnit").text("");
  } else {
    $v.prop("disabled", false);
    $("#discountUnit").text(type === "Percent" ? "(درصد)" : "(تومان)");
  }
}

/** Renders the main-image picker: shows the given url or resets to empty. */
function setMainImage(url) {
  var wrapper = document.getElementById("Form_MainImage_wrapper");
  var $w = $(wrapper);
  $("#Form_MainImage").val("");
  $w.find('input[name="MainImage_remove"]').val("");
  if (url) {
    $w.removeClass("image-input-empty").addClass("image-input-changed");
    $w.find(".image-input-wrapper").css(
      "background-image",
      "url('" + url + "')",
    );
  } else {
    $w.removeClass("image-input-changed").addClass("image-input-empty");
    $w.find(".image-input-wrapper").css("background-image", "none");
  }
}
//#endregion

//#region CRUD Page Configuration
var page = AdminCrud.createPage({
  table: {
    selector: "#kt_courses_table",
    options: {
      columnDefs: [
        { orderable: false, targets: [0, -1] },
        { targets: -1, className: "text-nowrap text-end" },
        // Column responsiveness priorities (order shown on small screens).
        { responsivePriority: 1, targets: 0 },
        { responsivePriority: 2, targets: 1 },
        { responsivePriority: 3, targets: -1 },
        { responsivePriority: 4, targets: 4 },
        { responsivePriority: 5, targets: 3 },
        { responsivePriority: 6, targets: 2 },
        { responsivePriority: 7, targets: 5 },
        { responsivePriority: 8, targets: 6 },
        { responsivePriority: 9, targets: 7 },
      ],
    },
  },
  search: "#courseSearch",
  filters: [
    { el: "#statusFilter", test: F.dataEquals("status") },
    { el: "#categoryFilter", test: F.dataContains("categories") },
    { el: "#instructorFilter", test: F.dataContains("instructors") },
  ],
  modal: {
    id: "kt_modal_course",
    titleEl: "#modal-course-title",
    submitBtn: "kt_course_submit",
  },
  deleteModal: { id: "kt_modal_delete_course", nameEl: "#delete-course-name" },
  texts: {
    createTitle: "افزودن دوره جدید",
    editTitle: "ویرایش دوره",
    created: "دوره با موفقیت ایجاد شد.",
    updated: "تغییرات با موفقیت ذخیره شد.",
    deleted: "دوره با موفقیت حذف شد.",
  },

  // Create/destroy TinyMCE with the modal so its content stays fresh.
  onModalShown: function () {
    if (!descriptionHtmlEditor.ready()) descriptionHtmlEditor.init();
  },
  onModalHidden: function () {
    descriptionHtmlEditor.destroy();
  },

  loadItem: function (id) {
    // TODO: fetch the item from the server instead of the local sample data.
    return coursesData[id];
  },

  // Populate all course form fields from a course object.
  fillForm: function (c) {
    c = c || {
      name: "",
      latinName: "",
      slug: "",
      shortDescription: "",
      description: "",
      price: null,
      discountType: "",
      discountValue: null,
      isSaleActive: true,
      displayStatus: "Draft",
      isFeatured: false,
      isAmazing: false,
      introductionVideo: "",
      categoryIds: [],
      instructorIds: [],
      image: null,
    };
    $("#Form_Name").val(c.name);
    $("#Form_LatinName").val(c.latinName);
    $("#Form_Slug").val(c.slug);
    slugTouched = !!c.slug;
    $("#Form_ShortDescription").val(c.shortDescription);
    setDescription(c.description);
    $("#Form_Price").val(U.toFa(c.price));
    $("#Form_DiscountType").val(c.discountType || "");
    setDiscountState(c.discountType || "");
    $("#Form_DiscountValue").val(U.toFa(c.discountValue));
    $("#Form_IsSaleActive").prop("checked", c.isSaleActive);
    $("#Form_DisplayStatus").val(c.displayStatus);
    $("#Form_IsFeatured").prop("checked", c.isFeatured);
    $("#Form_IsAmazing").prop("checked", c.isAmazing);
    $("#Form_IntroductionVideo").val(c.introductionVideo);
    $("#Form_CategoryIds").val(c.categoryIds).trigger("change");
    $("#Form_InstructorIds").val(c.instructorIds).trigger("change");
    setMainImage(c.image);
    $("#kt_course_form .is-invalid").removeClass("is-invalid");
  },

  // Client-side validation; returns the list of error messages.
  validate: function () {
    var errors = [];
    if (U.setInvalid($("#Form_Name"), !$("#Form_Name").val().trim()))
      errors.push("نام دوره را وارد کنید.");
    if (U.setInvalid($("#Form_LatinName"), !$("#Form_LatinName").val().trim()))
      errors.push("نام لاتین را وارد کنید.");
    if (U.setInvalid($("#Form_Slug"), !$("#Form_Slug").val().trim()))
      errors.push("اسلاگ را وارد کنید.");

    var type = $("#Form_DiscountType").val();
    if (type) {
      var dv = U.parseNumber($("#Form_DiscountValue").val());
      var price = U.parseNumber($("#Form_Price").val());
      var bad =
        !dv ||
        (type === "Percent" && dv > 100) ||
        (type === "Fixed" && price !== null && dv > price);
      if (U.setInvalid($("#Form_DiscountValue"), bad))
        errors.push("مقدار تخفیف معتبر نیست.");
    } else {
      U.setInvalid($("#Form_DiscountValue"), false);
    }
    return errors;
  },

  // Collect the form into the course data object sent to the server.
  getData: function () {
    return {
      name: $("#Form_Name").val().trim(),
      latinName: $("#Form_LatinName").val().trim(),
      slug: $("#Form_Slug").val().trim(),
      shortDescription: $("#Form_ShortDescription").val().trim(),
      description: getDescription(),
      price: U.parseNumber($("#Form_Price").val()),
      discountType: $("#Form_DiscountType").val() || null,
      discountValue: U.parseNumber($("#Form_DiscountValue").val()),
      isSaleActive: $("#Form_IsSaleActive").is(":checked"),
      displayStatus: $("#Form_DisplayStatus").val(),
      isFeatured: $("#Form_IsFeatured").is(":checked"),
      isAmazing: $("#Form_IsAmazing").is(":checked"),
      introductionVideo: $("#Form_IntroductionVideo").val().trim(),
      categoryIds: $("#Form_CategoryIds").val() || [],
      instructorIds: $("#Form_InstructorIds").val() || [],
    };
  },
});
//#endregion

//#region Course-specific Form Events
// Auto-generate the slug from the Latin name until the user edits it manually.
$("#Form_LatinName").on("input", function () {
  if (!slugTouched) $("#Form_Slug").val(U.slugify(this.value));
});
$("#Form_Slug").on("input", function () {
  slugTouched = this.value.length > 0;
});
// Pretty-print numbers while typing price and discount fields.
$("#Form_Price, #Form_DiscountValue").on("input", function () {
  U.formatNumberInput(this);
});
$("#Form_DiscountType").on("change", function () {
  setDiscountState(this.value);
});
//#endregion

//#region Inline onclick Handlers (HTML compatibility)
// The markup calls these directly via onclick="..." attributes.
function openCreateModal() {
  page.openCreate();
}
function openEditModal(id) {
  page.openEdit(id);
}
function submitCourseForm() {
  page.submit();
}
function openDeleteModal(id, name) {
  page.openDelete(id, name);
}
function confirmDelete() {
  page.confirmDelete();
}
//#endregion

//#region Sample Data (temporary, until a real API exists)
var coursesData = {
  1: {
    name: "دوره جامع ASP.NET Core",
    latinName: "Complete ASP.NET Core",
    slug: "complete-aspnet-core",
    shortDescription: "",
    description: "",
    price: 1500000,
    discountType: "",
    discountValue: null,
    isSaleActive: true,
    displayStatus: "Published",
    isFeatured: true,
    isAmazing: false,
    introductionVideo: "",
    categoryIds: ["1", "2", "3"],
    instructorIds: ["1"],
    image: "/assets/media/stock/600x400/img-1.jpg",
  },
  2: {
    name: "آموزش مقدماتی جاوااسکریپت",
    latinName: "JavaScript Basics",
    slug: "javascript-basics",
    shortDescription: "",
    description: "",
    price: null,
    discountType: "",
    discountValue: null,
    isSaleActive: true,
    displayStatus: "Published",
    isFeatured: false,
    isAmazing: true,
    introductionVideo: "",
    categoryIds: ["1", "2"],
    instructorIds: ["2"],
    image: "/assets/media/stock/600x400/img-2.jpg",
  },
  3: {
    name: "فتوشاپ از صفر تا صد",
    latinName: "Photoshop A to Z",
    slug: "photoshop-a-to-z",
    shortDescription: "",
    description: "",
    price: 850000,
    discountType: "Percent",
    discountValue: 10,
    isSaleActive: true,
    displayStatus: "Draft",
    isFeatured: false,
    isAmazing: false,
    introductionVideo: "",
    categoryIds: ["5"],
    instructorIds: ["3"],
    image: null,
  },
  4: {
    name: "کارآفرینی و استارتاپ",
    latinName: "Entrepreneurship",
    slug: "entrepreneurship",
    shortDescription: "",
    description: "",
    price: 2400000,
    discountType: "",
    discountValue: null,
    isSaleActive: false,
    displayStatus: "Hidden",
    isFeatured: true,
    isAmazing: false,
    introductionVideo: "",
    categoryIds: ["6"],
    instructorIds: ["1", "3"],
    image: "/assets/media/stock/600x400/img-3.jpg",
  },
};
//#endregion
//#endregion
