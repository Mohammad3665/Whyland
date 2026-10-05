var U = AdminCrud.Utils;
var F = AdminCrud.Filters;

// ----- TinyMCE (مخصوص این صفحه) -----
var descriptionEditor = null;
var pendingDescription = null;
var slugTouched = false;

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

// ----- صفحه -----
var page = AdminCrud.createPage({
  table: {
    selector: "#kt_courses_table",
    options: {
      columnDefs: [
        { orderable: false, targets: [0, -1] },
        { targets: -1, className: "text-nowrap text-end" },
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

  onModalShown: function () {
    if (!tinymce.get("Form_Description")) initDescriptionEditor();
  },
  onModalHidden: function () {
    var ed = tinymce.get("Form_Description");
    if (ed) ed.remove();
    descriptionEditor = null;
  },

  loadItem: function (id) {
    // TODO: دریافت از سرور
    return coursesData[id];
  },

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

// ----- رویدادهای فرم مخصوص دوره -----
$("#Form_LatinName").on("input", function () {
  if (!slugTouched) $("#Form_Slug").val(U.slugify(this.value));
});
$("#Form_Slug").on("input", function () {
  slugTouched = this.value.length > 0;
});
$("#Form_Price, #Form_DiscountValue").on("input", function () {
  U.formatNumberInput(this);
});
$("#Form_DiscountType").on("change", function () {
  setDiscountState(this.value);
});

// ----- سازگاری با onclick های HTML -----
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
