// ===== DataTable =====
var table = $("#kt_courses_table").DataTable({
  responsive: true,
  order: [],
  pageLength: 25,
  columnDefs: [
    { orderable: false, targets: [0, -1] },
    // دکمه‌های عملیات هیچ‌وقت نشکنند و همیشه کنار هم بمانند
    { targets: -1, className: "text-nowrap text-end" },
    // اولویت نمایش ستون‌ها: عدد کمتر = دیرتر به سطر جزئیات می‌رود
    { responsivePriority: 1, targets: 0 }, // تصویر
    { responsivePriority: 2, targets: 1 }, // نام
    { responsivePriority: 3, targets: -1 }, // عملیات
    { responsivePriority: 4, targets: 4 }, // وضعیت نمایش
    { responsivePriority: 5, targets: 3 }, // قیمت
    { responsivePriority: 6, targets: 2 }, // نام لاتین
    { responsivePriority: 7, targets: 5 }, // ویژه
    { responsivePriority: 8, targets: 6 }, // شگفت‌انگیز
    { responsivePriority: 9, targets: 7 }, // تاریخ ایجاد
  ],
  language: {
    search: "جستجو:",
    lengthMenu: "_MENU_ مورد",
    info: "نمایش _START_ تا _END_ از _TOTAL_ مورد",
    infoEmpty: "هیچ موردی یافت نشد",
    infoFiltered: "(فیلتر شده از _MAX_ مورد)",
    zeroRecords: "موردی یافت نشد",
    emptyTable: "داده‌ای موجود نیست",
    loadingRecords: "در حال بارگذاری...",
    processing: "در حال پردازش...",
    paginate: { first: "اول", last: "آخر", next: "بعدی", previous: "قبلی" },
  },
});

var currentMode = "create"; // "create" | "edit"
var deleteId = null;
var descriptionEditor = null;
var slugTouched = false;

// ===== Search =====
$("#courseSearch").on("keyup", function () {
  table.search(this.value).draw();
});

// ===== Filters (status + category + instructor) =====
$.fn.dataTable.ext.search.push(function (settings, data, dataIndex) {
  if (settings.nTable.id !== "kt_courses_table") return true;

  var row = $(table.row(dataIndex).node());
  var status = $("#statusFilter").val();
  var categoryId = $("#categoryFilter").val();
  var instructorId = $("#instructorFilter").val();

  if (status && row.data("status") !== status) return false;
  if (
    categoryId &&
    String(row.data("categories") || "")
      .split(",")
      .indexOf(categoryId) === -1
  )
    return false;
  if (
    instructorId &&
    String(row.data("instructors") || "")
      .split(",")
      .indexOf(instructorId) === -1
  )
    return false;
  return true;
});

$("#statusFilter, #categoryFilter, #instructorFilter").on(
  "change",
  function () {
    table.draw();
  },
);

// ===== HTML Editor (TinyMCE) =====
function initDescriptionEditor() {
  if (typeof tinymce === "undefined") return;

  var ver = parseInt(tinymce.majorVersion, 10) || 5;
  var isNew = ver >= 6;

  var options = {
    selector: "#Form_Description",
    readonly: 0,
    directionality: "rtl",
    height: 520,
    min_height: 420,
    resize: true,
    branding: false,
    menubar: "edit view insert format table tools",
    plugins: isNew
      ? "advlist autolink lists link image media table charmap anchor searchreplace visualblocks " +
        "code codesample fullscreen preview insertdatetime emoticons directionality wordcount help"
      : "advlist autolink lists link image media table charmap anchor hr paste searchreplace visualblocks " +
        "code codesample fullscreen preview insertdatetime emoticons directionality wordcount help",
    toolbar:
      "undo redo | " +
      (isNew ? "blocks fontsize" : "formatselect fontsizeselect") +
      " | bold italic underline strikethrough | forecolor backcolor | " +
      "alignright aligncenter alignleft alignjustify | ltr rtl | bullist numlist outdent indent | " +
      "link unlink anchor | image media table | blockquote hr codesample | charmap emoticons | " +
      "subscript superscript removeformat | searchreplace visualblocks code preview fullscreen",
    toolbar_mode: "wrap",
    block_formats:
      "پاراگراف=p; عنوان ۱=h1; عنوان ۲=h2; عنوان ۳=h3; عنوان ۴=h4; عنوان ۵=h5; عنوان ۶=h6; نقل‌قول=blockquote; متن قالب‌بندی‌شده=pre",
    fontsize_formats: "12px 14px 16px 18px 20px 24px 28px 32px 40px",
    link_default_target: "_blank",
    link_assume_external_targets: "https",
    link_title: false,
    relative_urls: false,
    remove_script_host: false,
    convert_urls: false,
    table_default_attributes: { class: "table table-bordered" },
    table_default_styles: { width: "100%" },
    image_caption: true,
    image_advtab: true,
    image_title: true,
    automatic_uploads: false,
    paste_data_images: true,
    file_picker_types: "image",
    file_picker_callback: function (cb) {
      var input = document.createElement("input");
      input.type = "file";
      input.accept = "image/*";
      input.onchange = function () {
        var file = this.files[0];
        var reader = new FileReader();
        reader.onload = function () {
          cb(reader.result, { title: file.name, alt: file.name });
        };
        reader.readAsDataURL(file);
      };
      input.click();
    },
    content_style:
      "body { font-family: Vazirmatn, IRANSans, Tahoma, sans-serif; font-size: 15px; line-height: 1.9; direction: rtl; padding: 12px; }" +
      " img { max-width: 100%; height: auto; }" +
      " table { border-collapse: collapse; } table td, table th { border: 1px solid #ddd; padding: 6px 10px; }",
    setup: function (editor) {
      editor.on("init", function () {
        descriptionEditor = editor;
        try {
          if (editor.mode && editor.mode.set) editor.mode.set("design");
          else if (editor.setMode) editor.setMode("design");
        } catch (e) {}
        if (pendingDescription !== null) editor.setContent(pendingDescription);
      });
    },
  };

  if (isNew) {
    options.license_key = "gpl";
    options.promotion = false;
    options.font_size_formats = options.fontsize_formats;
  }

  if (typeof KTThemeMode !== "undefined" && KTThemeMode.getMode() === "dark") {
    options["skin"] = "oxide-dark";
    options["content_css"] = "dark";
  }

  tinymce.init(options);
}

var pendingDescription = null;

function setDescription(html) {
  pendingDescription = html || "";
  if (descriptionEditor) descriptionEditor.setContent(pendingDescription);
  else $("#Form_Description").val(pendingDescription);
}

function getDescription() {
  return descriptionEditor
    ? descriptionEditor.getContent()
    : $("#Form_Description").val();
}

// TinyMCE داخل المان مخفی درست مقداردهی نمی‌شود؛ پس بعد از نمایش مودال ساخته و بعد از بستن حذف می‌شود
var courseModalEl = document.getElementById("kt_modal_course");

courseModalEl.addEventListener("shown.bs.modal", function () {
  if (!tinymce.get("Form_Description")) initDescriptionEditor();
});

courseModalEl.addEventListener("hidden.bs.modal", function () {
  var ed = tinymce.get("Form_Description");
  if (ed) ed.remove();
  descriptionEditor = null;
});

// ===== Helpers =====
function slugify(text) {
  return text
    .toString()
    .toLowerCase()
    .trim()
    .replace(/[^a-z0-9\u0600-\u06FF\s-]/g, "")
    .replace(/[\s_]+/g, "-")
    .replace(/-+/g, "-")
    .replace(/^-|-$/g, "");
}

function formatNumber(el) {
  var digits = el.value.replace(/[^\d]/g, "");
  el.value = digits ? Number(digits).toLocaleString("en-US") : "";
}

function toFa(n) {
  return n === null || n === undefined ? "" : Number(n).toLocaleString("en-US");
}

function parseNumber(val) {
  var digits = (val || "").replace(/[^\d]/g, "");
  return digits ? Number(digits) : null;
}

function setDiscountState(type) {
  var $value = $("#Form_DiscountValue");
  if (!type) {
    $value.val("").prop("disabled", true);
    $("#discountUnit").text("");
  } else {
    $value.prop("disabled", false);
    $("#discountUnit").text(type === "Percent" ? "(درصد)" : "(تومان)");
  }
}

function setMainImage(url) {
  var wrapper = document.getElementById("Form_MainImage_wrapper");
  var inst =
    typeof KTImageInput !== "undefined"
      ? KTImageInput.getInstance(wrapper)
      : null;
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

// ===== Form events =====
$("#Form_LatinName").on("input", function () {
  if (!slugTouched) $("#Form_Slug").val(slugify(this.value));
});
$("#Form_Slug").on("input", function () {
  slugTouched = this.value.length > 0;
});
$("#Form_Price, #Form_DiscountValue").on("input", function () {
  formatNumber(this);
});
$("#Form_DiscountType").on("change", function () {
  setDiscountState(this.value);
});

// ===== Mock data =====
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

// ===== Modal: create / edit =====
function fillForm(c) {
  $("#Form_Name").val(c.name);
  $("#Form_LatinName").val(c.latinName);
  $("#Form_Slug").val(c.slug);
  slugTouched = !!c.slug;
  $("#Form_ShortDescription").val(c.shortDescription);
  setDescription(c.description);
  $("#Form_Price").val(toFa(c.price));
  $("#Form_DiscountType").val(c.discountType || "");
  setDiscountState(c.discountType || "");
  $("#Form_DiscountValue").val(toFa(c.discountValue));
  $("#Form_IsSaleActive").prop("checked", c.isSaleActive);
  $("#Form_DisplayStatus").val(c.displayStatus);
  $("#Form_IsFeatured").prop("checked", c.isFeatured);
  $("#Form_IsAmazing").prop("checked", c.isAmazing);
  $("#Form_IntroductionVideo").val(c.introductionVideo);
  $("#Form_CategoryIds").val(c.categoryIds).trigger("change");
  $("#Form_InstructorIds").val(c.instructorIds).trigger("change");
  setMainImage(c.image);
  $("#kt_course_form .is-invalid").removeClass("is-invalid");
}

function openCreateModal() {
  currentMode = "create";
  $("#modal-course-title").text("افزودن دوره جدید");
  $("#Form_Id").val("");
  fillForm({
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
  });
  bootstrap.Modal.getOrCreateInstance(
    document.getElementById("kt_modal_course"),
  ).show();
}

function openEditModal(id) {
  currentMode = "edit";
  $("#modal-course-title").text("ویرایش دوره");
  $("#Form_Id").val(id);
  // TODO: دریافت اطلاعات دوره از سرور با id
  fillForm(coursesData[id]);
  bootstrap.Modal.getOrCreateInstance(
    document.getElementById("kt_modal_course"),
  ).show();
}

// ===== Validation =====
function setInvalid($el, invalid) {
  $el.toggleClass("is-invalid", invalid);
  return invalid;
}

function validateForm() {
  var errors = [];
  if (setInvalid($("#Form_Name"), !$("#Form_Name").val().trim()))
    errors.push("نام دوره را وارد کنید.");
  if (setInvalid($("#Form_LatinName"), !$("#Form_LatinName").val().trim()))
    errors.push("نام لاتین را وارد کنید.");
  if (setInvalid($("#Form_Slug"), !$("#Form_Slug").val().trim()))
    errors.push("اسلاگ را وارد کنید.");

  var type = $("#Form_DiscountType").val();
  if (type) {
    var dv = parseNumber($("#Form_DiscountValue").val());
    var price = parseNumber($("#Form_Price").val());
    var bad =
      !dv ||
      (type === "Percent" && dv > 100) ||
      (type === "Fixed" && price !== null && dv > price);
    if (setInvalid($("#Form_DiscountValue"), bad))
      errors.push("مقدار تخفیف معتبر نیست.");
  } else {
    setInvalid($("#Form_DiscountValue"), false);
  }
  return errors;
}

// ===== Submit =====
function submitCourseForm() {
  var errors = validateForm();
  if (errors.length) {
    toastr.error(errors[0], "خطا");
    return;
  }

  var data = {
    id: $("#Form_Id").val() || null,
    name: $("#Form_Name").val().trim(),
    latinName: $("#Form_LatinName").val().trim(),
    slug: $("#Form_Slug").val().trim(),
    shortDescription: $("#Form_ShortDescription").val().trim(),
    description: getDescription(),
    price: parseNumber($("#Form_Price").val()), // null = رایگان
    discountType: $("#Form_DiscountType").val() || null,
    discountValue: parseNumber($("#Form_DiscountValue").val()),
    isSaleActive: $("#Form_IsSaleActive").is(":checked"),
    displayStatus: $("#Form_DisplayStatus").val(),
    isFeatured: $("#Form_IsFeatured").is(":checked"),
    isAmazing: $("#Form_IsAmazing").is(":checked"),
    introductionVideo: $("#Form_IntroductionVideo").val().trim(),
    categoryIds: $("#Form_CategoryIds").val() || [],
    instructorIds: $("#Form_InstructorIds").val() || [],
  };
  console.log(data);

  var btn = document.getElementById("kt_course_submit");
  btn.setAttribute("data-kt-indicator", "on");
  btn.disabled = true;

  setTimeout(function () {
    btn.removeAttribute("data-kt-indicator");
    btn.disabled = false;

    bootstrap.Modal.getInstance(
      document.getElementById("kt_modal_course"),
    ).hide();

    toastr.success(
      currentMode === "create"
        ? "دوره با موفقیت ایجاد شد."
        : "تغییرات با موفقیت ذخیره شد.",
      "موفق",
    );
  }, 800);
}

// ===== Delete =====
function openDeleteModal(id, name) {
  deleteId = id;
  $("#delete-course-name").text(name);
  new bootstrap.Modal(document.getElementById("kt_modal_delete_course")).show();
}

function confirmDelete() {
  bootstrap.Modal.getInstance(
    document.getElementById("kt_modal_delete_course"),
  ).hide();
  toastr.success("دوره با موفقیت حذف شد.", "موفق");
}

if (typeof KTSelect2 !== "undefined") {
  KTSelect2.init();
}
