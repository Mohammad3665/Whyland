//#region Courses Page Scripts
/**
 * Admin "Courses" list and CRUD form (section 7.2 of the SRS).
 *
 *   GET  ?handler=Rows        one page of rows (paging, sorting, search and filters server-side)
 *   GET  ?handler=Fields      the generic filterable fields and their operators
 *   GET  ?handler=FormOptions active categories and instructors for the form and the picker
 *   GET  ?handler=Details     the course with every property, its selected links and image URLs
 *   POST ?handler=Create/Update/Delete
 *
 * The table, filter picker and server round-trip come from common/admin-table.js and
 * common/admin-crud.js. This file keeps only what belongs to a course: its columns, its
 * category/instructor picker kinds, the details modal and the multipart form with its cover image.
 */

var page; // shared CRUD controller
var picker; // the generic filter picker
var formOptions = { categories: [], instructors: [] };
var imageRemoved = false;
var slugTouched = false;

//#region Description Editor
var descriptionEditor = HtmlEditor.create({
  selector: "#Form_Description",
  height: 520,
  minHeight: 420,
});

function setDescription(html) {
  descriptionEditor.set(html || "");
}

function getDescription() {
  return descriptionEditor.get();
}
//#endregion

//#region Form and Filter Options
/** Reads the active categories and instructors once; both the filters and the form use them. */
function loadFormOptions() {
  return $.getJSON(AdminTable.handlerUrl("FormOptions")).then(function (res) {
    if (!res || !res.isSuccess || !res.data) {
      return $.Deferred().reject({ responseJSON: res }).promise();
    }

    formOptions = {
      categories: res.data.categories || [],
      instructors: res.data.instructors || [],
    };
    fillFormCategoryOptions();
    fillFormInstructorOptions();
    return formOptions;
  });
}

/** The depth of a category in the returned tree; roots are level one. */
function categoryDepth(category, byId) {
  var depth = 1;
  var parentId = category.parentId;
  var guard = 0;

  while (parentId && byId[parentId] && guard < 10) {
    depth += 1;
    parentId = byId[parentId].parentId;
    guard += 1;
  }

  return depth;
}

/** The server returns categories in display order; the selects show that order with indentation. */
function categoryLabel(category, byId) {
  var depth = categoryDepth(category, byId);
  var prefix = "";
  for (var i = 1; i < depth; i++) prefix += "— ";
  return prefix + category.name;
}

/** Full name of one instructor option. */
function instructorName(instructor) {
  return instructor.firstName + " " + instructor.lastName;
}

/**
 * Turns ids into the labels the admin picked. A missing option (for example an inactive
 * category) falls back to the id so the details modal still shows every stored value.
 */
function namesFor(options, ids, labelOf) {
  var list = Array.isArray(ids)
    ? ids
    : String(ids || "")
        .split(",")
        .filter(Boolean);

  if (!list.length) return "—";

  var byId = {};
  options.forEach(function (option) {
    byId[String(option.id)] = labelOf(option);
  });

  return list
    .map(function (id) {
      return byId[String(id)] || String(id);
    })
    .join("، ");
}

function fillFormCategoryOptions() {
  var $select = $("#Form_CategoryIds");
  var byId = {};

  formOptions.categories.forEach(function (category) {
    byId[category.id] = category;
  });

  $select.empty();
  formOptions.categories.forEach(function (category) {
    $select.append(
      $("<option>").val(category.id).text(categoryLabel(category, byId)),
    );
  });
}

function fillFormInstructorOptions() {
  var $select = $("#Form_InstructorIds");

  $select.empty();
  formOptions.instructors.forEach(function (instructor) {
    $select.append(
      $("<option>")
        .val(instructor.id)
        .text(
          instructorName(instructor) +
            (instructor.specialization ? " — " + instructor.specialization : ""),
        ),
    );
  });
}

/**
 * The two course-specific picker kinds. They read the same option lists the form uses, so the
 * category and instructor filters stay inside the generic "فیلتر روی" picker.
 */
var courseFilterKinds = {
  courseCategory: {
    fill: function ($select) {
      if (!$select.prop("multiple")) {
        $select.append($("<option>").val("").text("انتخاب دسته‌بندی..."));
      }

      var byId = {};
      formOptions.categories.forEach(function (category) {
        byId[category.id] = category;
      });
      formOptions.categories.forEach(function (category) {
        $select.append(
          $("<option>")
            .val(category.id)
            .text(categoryLabel(category, byId)),
        );
      });
    },
    label: function (field, value) {
      return namesFor(formOptions.categories, value, function (category) {
        return category.name;
      });
    },
  },
  courseInstructor: {
    fill: function ($select) {
      if (!$select.prop("multiple")) {
        $select.append($("<option>").val("").text("انتخاب مدرس..."));
      }

      formOptions.instructors.forEach(function (instructor) {
        $select.append(
          $("<option>").val(instructor.id).text(instructorName(instructor)),
        );
      });
    },
    label: function (field, value) {
      return namesFor(formOptions.instructors, value, instructorName);
    },
  },
};
//#endregion

//#region Server Connectivity
/** The generic picker already contains the category and instructor filters. */
function courseConditions() {
  return picker ? picker.conditions() : [];
}

function reloadTable() {
  if (!page) return;
  page.table.ajax.reload(null, false);
}

/** Deletes one course and refreshes the current page. */
function deleteCourse(id) {
  return AdminTable.postHandler("Delete", { id: id }).done(reloadTable);
}

/** Sends the multipart course form; the image may be absent, replaced or explicitly removed. */
function saveCourse(data, mode) {
  var form = new FormData();

  form.append("Id", mode === "edit" ? page.state.editId : "");
  form.append("Name", data.name);
  form.append("LatinName", data.latinName);
  form.append("ShortDescription", data.shortDescription);
  form.append("Description", data.description || "");
  form.append("Slug", data.slug);
  form.append("Price", data.price === null ? "" : data.price);
  form.append("DiscountType", data.discountType || "None");
  form.append(
    "DiscountValue",
    data.discountValue === null ? "" : data.discountValue,
  );
  form.append("IsSaleActive", data.isSaleActive ? "true" : "false");
  form.append("DisplayStatus", data.displayStatus);
  form.append("IsFeatured", data.isFeatured ? "true" : "false");
  form.append("IsAmazing", data.isAmazing ? "true" : "false");
  form.append("RemoveImage", data.removeImage ? "true" : "false");

  (data.categoryIds || []).forEach(function (id) {
    form.append("CategoryIds", id);
  });
  (data.instructorIds || []).forEach(function (id) {
    form.append("InstructorIds", id);
  });

  var file = $("#Form_MainImage")[0].files[0];
  if (file) form.append("Image", file);

  var dfd = $.Deferred();
  $.ajax({
    url: AdminTable.handlerUrl(mode === "create" ? "Create" : "Update"),
    type: "POST",
    data: form,
    processData: false,
    contentType: false,
    headers: { RequestVerificationToken: AdminTable.csrfToken() },
  })
    .done(function (res) {
      if (res && res.isSuccess) dfd.resolve(res);
      else dfd.reject({ responseJSON: res });
    })
    .fail(function (xhr) {
      dfd.reject(xhr);
    });

  return dfd.promise().done(reloadTable);
}

/** Reads one course for the edit form and the details modal. */
function loadCourse(id) {
  var dfd = $.Deferred();

  $.getJSON(AdminTable.handlerUrl("Details"), { id: id })
    .done(function (res) {
      if (res && res.isSuccess && res.data) {
        dfd.resolve(res.data);
        return;
      }

      var failure = { responseJSON: res };
      toastr.error(
        AdminTable.serverMessage(failure, "خطا در دریافت اطلاعات دوره."),
        "خطا",
      );
      dfd.reject(failure);
    })
    .fail(function (xhr) {
      toastr.error(
        AdminTable.serverMessage(xhr, "خطا در دریافت اطلاعات دوره."),
        "خطا",
      );
      dfd.reject(xhr);
    });

  return dfd.promise();
}
//#endregion

//#region Details Modal
var DISCOUNT_TYPE_LABELS = {
  None: "بدون تخفیف",
  Percentage: "درصدی",
  FixedAmount: "مبلغ ثابت",
};

var STATUS_BADGES = {
  Draft: { modifier: "secondary", text: "پیش‌نویس" },
  Published: { modifier: "success", text: "منتشر شده" },
  Hidden: { modifier: "dark", text: "مخفی" },
};

function detailText(selector, value) {
  $(selector).text(
    value === null || value === undefined || value === "" ? "—" : String(value),
  );
}

function detailBoolean(selector, value) {
  $(selector).text(value ? "بله" : "خیر");
}

function detailActor(name, id) {
  if (name && id) return name + " (" + id + ")";
  return name || id || "—";
}

/** Groups a number and rewrites its digits for the Persian admin UI. */
function faNumber(value) {
  return AdminTable.faDigits(Number(value).toLocaleString("en-US"));
}

/** Reads a numeric input after normalizing Persian and Arabic-Indic digits. */
function parseNumberInput(selector) {
  return AdminCrud.Utils.parseNumber(
    AdminTable.latinDigits($(selector).val()),
  );
}

function detailPrice(price) {
  if (price === null || price === undefined) return "رایگان";
  return faNumber(price) + " تومان";
}

function detailDiscountValue(type, value) {
  if (!type || type === "None" || value === null || value === undefined) {
    return "—";
  }
  return type === "Percentage"
    ? faNumber(value) + "٪"
    : faNumber(value) + " تومان";
}

/** Renders every property of CourseDetailsDto into the read-only modal. */
function fillCourseDetails(course) {
  var imageUrl = course.mainImageUrl || course.thumbnailUrl || "";
  $("#cd-image").attr("src", imageUrl).toggleClass("d-none", !imageUrl);
  $("#cd-image-empty").toggleClass("d-none", !!imageUrl);

  detailText("#cd-name", course.name);
  detailText("#cd-latin-name", course.latinName);
  detailText("#cd-slug", course.slug);
  detailText("#cd-info-name", course.name);
  detailText("#cd-info-latin-name", course.latinName);
  detailText("#cd-info-slug", course.slug);
  detailText("#cd-main-image-url", course.mainImageUrl);
  detailText("#cd-thumbnail-url", course.thumbnailUrl);
  detailText("#cd-id", course.id);
  detailText("#cd-short-description", course.shortDescription);
  detailText("#cd-price", detailPrice(course.price));
  detailText("#cd-discount-type", DISCOUNT_TYPE_LABELS[course.discountType] || course.discountType);
  detailText(
    "#cd-discount-value",
    detailDiscountValue(course.discountType, course.discountValue),
  );

  var status = STATUS_BADGES[course.displayStatus] || STATUS_BADGES.Draft;
  $("#cd-status").empty().append(AdminTable.badge(status.modifier, status.text));
  detailBoolean("#cd-sale-active", course.isSaleActive);
  detailBoolean("#cd-featured", course.isFeatured);
  detailBoolean("#cd-amazing", course.isAmazing);

  detailText(
    "#cd-categories",
    namesFor(formOptions.categories, course.categoryIds, function (category) {
      return category.name;
    }),
  );
  detailText(
    "#cd-category-ids",
    (course.categoryIds || []).length ? course.categoryIds.join("، ") : "—",
  );
  detailText(
    "#cd-instructors",
    namesFor(formOptions.instructors, course.instructorIds, instructorName),
  );
  detailText(
    "#cd-instructor-ids",
    (course.instructorIds || []).length ? course.instructorIds.join("، ") : "—",
  );

  if (course.description) $("#cd-description").html(course.description);
  else $("#cd-description").text("—");

  detailText("#cd-created-at", AdminTable.faDateTime(course.createdAt));
  detailText("#cd-created-by", detailActor(course.createdByName, course.createdBy));
  detailText("#cd-updated-at", AdminTable.faDateTime(course.updatedAt));
  detailText("#cd-updated-by", detailActor(course.updatedByName, course.updatedBy));

  $("#cd-edit")
    .off("click.courseDetails")
    .on("click.courseDetails", function () {
      AdminCrud.Utils.hideModal("kt_modal_course_details");
      page.openEdit(course.id);
    });
}

function openCourseDetails(id) {
  loadCourse(id).done(function (course) {
    fillCourseDetails(course);
    AdminCrud.Utils.showModal("kt_modal_course_details");
  });
}
//#endregion

//#region Row Rendering
function namesCell(text) {
  var $cell = $("<div class='d-flex flex-wrap gap-1'>");
  var names = String(text || "")
    .split("،")
    .map(function (name) {
      return name.trim();
    })
    .filter(Boolean);

  if (!names.length) return $("<span class='text-muted'>").text("—");

  names.forEach(function (name) {
    $cell.append($("<span class='badge badge-light-info'>").text(name));
  });
  return $cell;
}

/** Fills one row of the seven-column server-side table. */
function fillCourseRow(row, course) {
  var $cells = $("td", row);

  // Image
  var $image = $("<div class='symbol symbol-45px symbol-2by3'>");
  if (course.thumbnailUrl || course.mainImageUrl) {
    $image.append(
      $("<img>")
        .attr("src", course.thumbnailUrl || course.mainImageUrl)
        .attr("alt", course.name),
    );
  } else {
    $image.append(
      $("<div class='symbol-label bg-light-primary text-primary'>").append(
        '<i class="ki-duotone ki-picture fs-2"><span class="path1"></span><span class="path2"></span></i>',
      ),
    );
  }
  $cells.eq(0).addClass("text-center").append($image);

  // Name only (no Latin name and no slug); long names are clamped by .course-name-text.
  $cells.eq(1).append(
    $("<span class='text-gray-800 fw-bold course-name-text'>")
      .attr("title", course.name)
      .text(course.name),
  );

  $cells.eq(2).append(namesCell(course.categoryNames));
  $cells.eq(3).append(namesCell(course.instructorNames));

  // Price
  if (course.price === null || course.price === undefined) {
    $cells.eq(4).append(AdminTable.badge("primary", "رایگان"));
  } else {
    var priceText = faNumber(course.price);
    $cells.eq(4).append($("<span class='text-gray-800 fw-bold'>").text(priceText));
    if (course.discountType && course.discountType !== "None") {
      var discountText =
        course.discountType === "Percentage"
          ? faNumber(course.discountValue) + "٪ تخفیف"
          : faNumber(course.discountValue) + " تخفیف";
      $cells.eq(4).append($("<div class='text-muted fs-8'>").text(discountText));
    }
  }

  var status = STATUS_BADGES[course.displayStatus] || STATUS_BADGES.Draft;
  $cells.eq(5).addClass("text-center").append(
    AdminTable.badge(status.modifier, status.text),
  );

  var $actions = $cells.eq(6).addClass("text-end nowrap-cell");

  var $details = $(
    '<button type="button" class="btn btn-sm btn-icon btn-bg-light btn-active-color-info me-1" title="جزئیات">',
  ).append(
    '<i class="ki-duotone ki-eye fs-2"><span class="path1"></span><span class="path2"></span><span class="path3"></span></i>',
  );
  $details.on("click", function () {
    openCourseDetails(course.id);
  });

  var $edit = $(
    '<button type="button" class="btn btn-sm btn-icon btn-bg-light btn-active-color-primary me-1" title="ویرایش">',
  ).append(
    '<i class="ki-duotone ki-pencil fs-2"><span class="path1"></span><span class="path2"></span></i>',
  );
  $edit.on("click", function () {
    page.openEdit(course.id);
  });

  var $delete = $(
    '<button type="button" class="btn btn-sm btn-icon btn-bg-light btn-active-color-danger" title="حذف">',
  ).append(
    '<i class="ki-duotone ki-trash fs-2"><span class="path1"></span><span class="path2"></span><span class="path3"></span><span class="path4"></span><span class="path5"></span></i>',
  );
  $delete.on("click", function () {
    page.openDelete(course.id, course.name);
  });

  $actions.append($details, $edit, $delete);
}
//#endregion

//#region Form Helpers
function setDiscountState(type) {
  var $value = $("#Form_DiscountValue");

  if (!type || type === "None") {
    $value.val("").prop("disabled", true);
    $("#discountUnit").text("");
    return;
  }

  $value.prop("disabled", false);
  $("#discountUnit").text(type === "Percentage" ? "(درصد)" : "(تومان)");
}

/** Shows the stored or newly chosen image without a round-trip. */
function setMainImage(url) {
  var $wrapper = $("#Form_MainImage_wrapper");
  var $preview = $wrapper.find(".image-input-wrapper");
  var background = $wrapper.data("kt-image-input-background");

  $("#Form_MainImage").val("");
  imageRemoved = false;
  $("#Form_RemoveImage").val("0");

  if (url) {
    $wrapper.removeClass("image-input-empty").addClass("image-input-changed");
    $preview.css("background-image", "url('" + url + "')");
    if (background) background.css("background-image", "url('" + url + "')");
  } else {
    $wrapper.removeClass("image-input-changed").addClass("image-input-empty");
    $preview.css("background-image", "none");
    if (background) background.css("background-image", "none");
  }
}

/**
 * Initialises the form selects with the same positioning helper the page filters use, so the
 * options open directly under their own box inside the modal. The status select has no search.
 */
function initFormSelects() {
  var $modal = $("#kt_modal_course");

  ["#Form_CategoryIds", "#Form_InstructorIds"].forEach(function (selector) {
    var $select = $(selector);
    if (!$select.length || !$.fn.select2) return;

    AdminTable.filterSelect2($select, {
      dropdownParent: $modal,
      placeholder:
        selector === "#Form_CategoryIds" ? "انتخاب دسته‌بندی‌ها" : "انتخاب مدرسین",
      allowClear: true,
      width: "100%",
      dropdownAutoWidth: false,
      closeOnSelect: false,
    });
  });

  var $status = $("#Form_DisplayStatus");
  if ($status.length && $.fn.select2) {
    AdminTable.filterSelect2($status, {
      dropdownParent: $modal,
      width: "100%",
      dropdownAutoWidth: false,
      minimumResultsForSearch: Infinity,
    });
  }
}
//#endregion

//#region CRUD Page Configuration
function initPage() {
  page = AdminCrud.createPage({
    table: {
      selector: "#kt_courses_table",
      options: {
        serverSide: true,
        processing: true,
        searching: false, // the generic picker is the search and filter UI
        order: [],
        pageLength: 10,
        lengthMenu: [10, 25, 50, 100],
        ajax: AdminTable.serverSideAjax({
          conditions: courseConditions,
          errorMessage: "خطا در دریافت لیست دوره‌ها.",
        }),
        columns: [
          { data: null, defaultContent: "", name: "", orderable: false },
          { data: null, defaultContent: "", name: "name" },
          {
            data: null,
            defaultContent: "",
            name: "categoryNames",
            orderable: false,
          },
          {
            data: null,
            defaultContent: "",
            name: "instructorNames",
            orderable: false,
          },
          { data: null, defaultContent: "", name: "price" },
          { data: null, defaultContent: "", name: "displayStatus" },
          { data: null, defaultContent: "", name: "", orderable: false },
        ],
        columnDefs: [
          { responsivePriority: 1, targets: 2 },
          { responsivePriority: 2, targets: 6 },
          { responsivePriority: 3, targets: 5 },
          { responsivePriority: 4, targets: 0 },
          { responsivePriority: 5, targets: 4 },
          { responsivePriority: 6, targets: 1 },
          { responsivePriority: 7, targets: 3 },
          { orderable: false, targets: [0, 2, 3, 6] },
        ],
        createdRow: fillCourseRow,
      },
    },
    modal: {
      id: "kt_modal_course",
      titleEl: "#modal-course-title",
      submitBtn: "kt_course_submit",
    },
    deleteModal: {
      id: "kt_modal_delete_course",
      nameEl: "#delete-course-name",
    },
    texts: {
      createTitle: "افزودن دوره جدید",
      editTitle: "ویرایش دوره",
      created: "دوره با موفقیت ایجاد شد.",
      updated: "تغییرات با موفقیت ذخیره شد.",
      deleted: "دوره با موفقیت حذف شد.",
    },

    onModalShown: function () {
      initFormSelects();
      if (!descriptionEditor.ready()) descriptionEditor.init();
    },
    onModalHidden: function () {
      descriptionEditor.destroy();
    },

    loadItem: loadCourse,

    fillForm: function (course) {
      course = course || {
        name: "",
        latinName: "",
        slug: "",
        shortDescription: "",
        description: "",
        price: null,
        discountType: "None",
        discountValue: null,
        isSaleActive: true,
        displayStatus: "Draft",
        isFeatured: false,
        isAmazing: false,
        mainImageUrl: null,
        categoryIds: [],
        instructorIds: [],
      };

      $("#Form_Id").val(course.id || "");
      $("#Form_Name").val(course.name);
      $("#Form_LatinName").val(course.latinName);
      $("#Form_Slug").val(course.slug);
      slugTouched = !!course.slug;
      $("#Form_ShortDescription").val(course.shortDescription || "");
      setDescription(course.description || "");
      $("#Form_Price").val(
        course.price === null || course.price === undefined
          ? ""
          : AdminCrud.Utils.toFa(course.price),
      );
      $("#Form_DiscountType").val(course.discountType || "None");
      setDiscountState(course.discountType || "None");
      $("#Form_DiscountValue").val(
        course.discountValue === null || course.discountValue === undefined
          ? ""
          : AdminCrud.Utils.toFa(course.discountValue),
      );
      $("#Form_IsSaleActive").prop("checked", !!course.isSaleActive);
      $("#Form_DisplayStatus")
        .val(course.displayStatus || "Draft")
        .trigger("change");
      $("#Form_IsFeatured").prop("checked", !!course.isFeatured);
      $("#Form_IsAmazing").prop("checked", !!course.isAmazing);
      $("#Form_CategoryIds")
        .val((course.categoryIds || []).map(String))
        .trigger("change");
      $("#Form_InstructorIds")
        .val((course.instructorIds || []).map(String))
        .trigger("change");
      setMainImage(course.mainImageUrl || null);
      $("#kt_course_form .is-invalid").removeClass("is-invalid");
    },

    getData: function () {
      return {
        name: $("#Form_Name").val().trim(),
        latinName: $("#Form_LatinName").val().trim(),
        slug: $("#Form_Slug").val().trim(),
        shortDescription: $("#Form_ShortDescription").val().trim(),
        description: getDescription(),
        price: parseNumberInput("#Form_Price"),
        discountType: $("#Form_DiscountType").val() || "None",
        discountValue: parseNumberInput("#Form_DiscountValue"),
        isSaleActive: $("#Form_IsSaleActive").is(":checked"),
        displayStatus: $("#Form_DisplayStatus").val(),
        isFeatured: $("#Form_IsFeatured").is(":checked"),
        isAmazing: $("#Form_IsAmazing").is(":checked"),
        categoryIds: $("#Form_CategoryIds").val() || [],
        instructorIds: $("#Form_InstructorIds").val() || [],
        removeImage: imageRemoved,
      };
    },

    validate: function () {
      var errors = [];
      var required = [
        { el: "#Form_Name", msg: "نام دوره را وارد کنید." },
        { el: "#Form_LatinName", msg: "نام لاتین را وارد کنید." },
        { el: "#Form_Slug", msg: "اسلاگ را وارد کنید." },
      ];

      required.forEach(function (field) {
        if (AdminCrud.Utils.setInvalid($(field.el), !$(field.el).val().trim()))
          errors.push(field.msg);
      });

      var slug = $("#Form_Slug").val().trim();
      if (slug && !/^[a-z0-9]+(?:-[a-z0-9]+)*$/.test(slug)) {
        errors.push("اسلاگ فقط می‌تواند شامل حروف لاتین کوچک، عدد و خط تیره باشد.");
        $("#Form_Slug").addClass("is-invalid");
      }

      var categoryIds = $("#Form_CategoryIds").val() || [];
      if (!categoryIds.length) {
        errors.push("حداقل یک دسته‌بندی برای دوره انتخاب کنید.");
        $("#Form_CategoryIds").addClass("is-invalid");
      } else {
        $("#Form_CategoryIds").removeClass("is-invalid");
      }

      var price = parseNumberInput("#Form_Price");
      if ($("#Form_Price").val().trim() && (!price || price <= 0)) {
        errors.push("قیمت باید بزرگ‌تر از صفر باشد یا خالی بماند.");
        $("#Form_Price").addClass("is-invalid");
      } else {
        $("#Form_Price").removeClass("is-invalid");
      }

      var type = $("#Form_DiscountType").val();
      if (type && type !== "None") {
        var value = parseNumberInput("#Form_DiscountValue");
        var invalid =
          value === null ||
          (type === "Percentage" && (value < 0 || value > 100)) ||
          (type === "FixedAmount" && (price === null || value > price));

        if (AdminCrud.Utils.setInvalid($("#Form_DiscountValue"), invalid))
          errors.push("مقدار تخفیف با نوع تخفیف و قیمت دوره سازگار نیست.");
      } else {
        $("#Form_DiscountValue").removeClass("is-invalid");
      }

      return errors;
    },

    onSubmit: saveCourse,
    onDelete: deleteCourse,
  });
}
//#endregion

//#region Form Events
$("#Form_LatinName").on("input", function () {
  if (!slugTouched) $("#Form_Slug").val(AdminCrud.Utils.slugify(this.value));
});

$("#Form_Slug").on("input", function () {
  slugTouched = this.value.length > 0;
});

$("#Form_Price, #Form_DiscountValue").on("input", function () {
  var digits = AdminTable.latinDigits(this.value).replace(/[^0-9]/g, "");
  this.value = digits ? Number(digits).toLocaleString("en-US") : "";
});

$("#Form_DiscountType").on("change", function () {
  setDiscountState(this.value);
});

$("#Form_MainImage").on("change", function () {
  if (this.files && this.files.length) {
    imageRemoved = false;
    $("#Form_RemoveImage").val("0");
  }
});

$("#kt_modal_course").on("click", "[data-kt-image-input-action='remove']", function () {
  imageRemoved = true;
  $("#Form_RemoveImage").val("1");
});
//#endregion

//#region Inline onclick Handlers
function openCreateModal() {
  if (page) page.openCreate();
}

function submitCourseForm() {
  if (page) page.submit();
}

function openDeleteModal(id, name) {
  if (page) page.openDelete(id, name);
}

function confirmDelete() {
  if (page) page.confirmDelete();
}

function addFilterCondition() {
  if (picker) picker.add();
}

function clearFilterConditions() {
  if (picker) picker.clear();
}
//#endregion

//#region Boot
// The picker and the form selects read the same two option lists; the table is created after
// both have answered so its first request can already carry the selected filters.
$(function () {
  picker = AdminTable.filterPicker({
    fieldEl: "#filterField",
    operatorEl: "#filterOperator",
    valueWrapEl: "#filterValueWrap",
    chipsEl: "#filterChips",
    kinds: courseFilterKinds,
    onChange: reloadTable,
  });

  $.when(picker.load(), loadFormOptions())
    .always(function () {
      initPage();
    })
    .fail(function () {
      toastr.error("خطا در دریافت اطلاعات پایهٔ صفحه.", "خطا");
    });
});
//#endregion
//#endregion
