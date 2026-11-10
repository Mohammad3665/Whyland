//#region Blog Posts Page Scripts
/**
 * Admin "Blog Posts" list and CRUD form (section 12.1.2 of the SRS).
 *
 *   GET  ?handler=Rows        one page of rows (paging, sorting, search and filters server-side)
 *   GET  ?handler=Fields      the generic filterable fields and their operators
 *   GET  ?handler=FormOptions the blog taxonomy and the accounts allowed to author a post
 *   POST ?handler=Create/Update/Delete
 *
 * The table, filter picker and server round-trip come from common/admin-table.js and
 * common/admin-crud.js. This file keeps only what belongs to a blog post: its columns, its
 * category/author picker kinds, the details modal and the multipart form with its cover image.
 */

var page; // shared CRUD controller
var picker; // the generic filter picker
var formOptions = { categories: [], authors: [] };
var rowsById = {};
var imageRemoved = false;
var slugTouched = false;

//#region Content Editor (shared generic module)
var contentHtmlEditor = HtmlEditor.create({
  selector: "#Form_Content",
  height: 520,
  minHeight: 420,
});

/** Sets the editor content. */
function setContent(html) {
  contentHtmlEditor.set(html);
}

/** Returns the current editor content as HTML. */
function getContent() {
  return contentHtmlEditor.get();
}
//#endregion

//#region Form and Filter Options
/** Reads the categories and authors once; both the filters and the form use them. */
function loadFormOptions() {
  return $.getJSON(AdminTable.handlerUrl("FormOptions")).then(function (res) {
    if (!res || !res.isSuccess || !res.data) {
      return $.Deferred().reject({ responseJSON: res }).promise();
    }

    formOptions = {
      categories: res.data.categories || [],
      authors: res.data.authors || [],
    };
    fillFormCategoryOptions();
    return formOptions;
  });
}

/** The indentation of one category in the flat option list. */
function categoryLabel(category) {
  var prefix = "";
  for (var i = 1; i < (category.depth || 1); i++) prefix += "— ";
  return prefix + category.name + (category.isActive ? "" : " (غیرفعال)");
}

/** The display name of one author option. */
function authorName(author) {
  return author.name || "—";
}

/** Splits a comma-joined value into the ids it carries. */
function splitIds(value) {
  return String(value || "")
    .split(",")
    .map(function (id) {
      return id.trim();
    })
    .filter(Boolean);
}

/** Turns ids into the labels the admin picked; unknown ids stay readable. */
function namesFor(options, value, labelOf) {
  var byId = {};
  options.forEach(function (option) {
    byId[String(option.id)] = labelOf(option);
  });

  return splitIds(value)
    .map(function (id) {
      return byId[id] || id;
    })
    .join("، ");
}

function fillFormCategoryOptions() {
  var $select = $("#Form_CategoryId");
  var selected = $select.val() || "";

  $select.empty().append($("<option>").val("").text("انتخاب دسته‌بندی..."));
  formOptions.categories.forEach(function (category) {
    $select.append(
      $("<option>")
        .val(category.id)
        .text(categoryLabel(category))
        .prop("disabled", !category.isActive),
    );
  });

  $select.val(selected).trigger("change");
}

/**
 * The two post-specific picker kinds. They read the same lists the form uses, so the category and
 * author filters stay inside the generic «فیلتر روی» picker.
 */
var blogFilterKinds = {
  blogCategory: {
    fill: function ($select) {
      if (!$select.prop("multiple")) {
        $select.append($("<option>").val("").text("انتخاب دسته‌بندی..."));
      }
      formOptions.categories.forEach(function (category) {
        $select.append(
          $("<option>").val(category.id).text(categoryLabel(category)),
        );
      });
    },
    label: function (field, value) {
      return namesFor(formOptions.categories, value, function (category) {
        return category.name;
      });
    },
  },
  blogAuthor: {
    fill: function ($select) {
      if (!$select.prop("multiple")) {
        $select.append($("<option>").val("").text("انتخاب نویسنده..."));
      }
      formOptions.authors.forEach(function (author) {
        $select.append($("<option>").val(author.id).text(authorName(author)));
      });
    },
    label: function (field, value) {
      return namesFor(formOptions.authors, value, authorName);
    },
  },
};
//#endregion

//#region Server Connectivity
/** The generic picker already contains the category and author filters. */
function postConditions() {
  return picker ? picker.conditions() : [];
}

function reloadTable() {
  if (page) page.table.ajax.reload(null, false);
}

/**
 * Persists the visual order after a drag & drop. The rows keep the display-order slots the server
 * gave them (so page two saves 11..20, not 1..10), and the table is reloaded in the default
 * display-order sort on success.
 */
function saveBlogPostOrder(rows) {
  $.ajax({
    url: AdminTable.handlerUrl("Reorder"),
    type: "POST",
    contentType: "application/json; charset=UTF-8",
    data: JSON.stringify(rows),
    headers: { RequestVerificationToken: AdminTable.csrfToken() },
  })
    .done(function (res) {
      if (res && res.isSuccess) {
        toastr.success("ترتیب مقالات با موفقیت ذخیره شد.", "موفق");
        page.table.order([]).ajax.reload(null, false);
      } else {
        toastr.error(
          AdminTable.serverMessage({ responseJSON: res }, "خطا در ذخیره ترتیب."),
          "خطا",
        );
        reloadTable();
      }
    })
    .fail(function (xhr) {
      toastr.error(AdminTable.serverMessage(xhr, "خطا در ذخیره ترتیب."), "خطا");
      reloadTable();
    });
}

/**
 * Wires the drag & drop ordering of the server-side table. Only the current page is moved, so the
 * target display-order slots of its rows are reused: dragging a row inside page two changes the
 * relative order without colliding with the rows on page one.
 */
function initRowReorder() {
  page.table.on("row-reorder", function () {
    var $rows = $(page.table.table().body()).find("tr[data-id]").not(".child");

    var slots = $rows
      .map(function () {
        return Number($(this).find(".blog-post-order").attr("data-value"));
      })
      .get()
      .filter(function (value) {
        return !isNaN(value) && value > 0;
      })
      .sort(function (a, b) {
        return a - b;
      });

    if (!slots.length || slots.length !== $rows.length) {
      reloadTable();
      return;
    }

    var rows = [];
    $rows.each(function (index) {
      var $row = $(this);
      var order = slots[index];
      $row
        .find(".blog-post-order")
        .attr("data-value", order)
        .text(AdminTable.faDigits(order));
      rows.push({ id: Number($row.attr("data-id")), displayOrder: order });
    });

    saveBlogPostOrder(rows);
  });
}

/** Deletes one post and refreshes the current page. */
function deleteBlogPost(id) {
  return AdminTable.postHandler("Delete", { id: id }).done(reloadTable);
}

/** Sends the multipart post form; the image may be absent, replaced or explicitly removed. */
function saveBlogPost(data, mode) {
  var form = new FormData();

  form.append("Id", mode === "edit" ? page.state.editId : "");
  form.append("Title", data.title);
  form.append("ShortDescription", data.shortDescription);
  form.append("Content", data.content || "");
  form.append("CategoryId", data.categoryId || "");
  form.append("Slug", data.slug);
  form.append("DisplayOrder", data.displayOrder);
  form.append("DisplayStatus", data.displayStatus);
  form.append("RemoveImage", data.removeImage ? "true" : "false");

  var file = $("#Form_Image")[0].files[0];
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

/** Reads the cached row for the edit form and details modal. */
function loadPost(id) {
  var post = rowsById[id];

  if (post) return post;

  var failure = {
    responseJSON: { message: "اطلاعات مقاله در صفحه پیدا نشد؛ لطفاً جدول را دوباره بارگذاری کنید." },
  };
  toastr.error(AdminTable.serverMessage(failure, "خطا در دریافت اطلاعات مقاله."), "خطا");
  return $.Deferred().reject(failure).promise();
}
//#endregion

//#region Formatting Helpers
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

function detailActor(name, id) {
  if (name && id) return name + " (" + id + ")";
  return name || id || "—";
}
//#endregion

//#region Details Modal
function fillPostImage(url) {
  $("#bpd-image").attr("src", url || "").toggleClass("d-none", !url);
  $("#bpd-image-empty").toggleClass("d-none", !!url);
}

/** Renders every property of the cached row into the read-only modal. */
function fillPostDetails(post) {
  fillPostImage(post.imageUrl || post.thumbnailUrl);

  detailText("#bpd-title", post.title);
  detailText("#bpd-title-info", post.title);
  detailText("#bpd-short-description", post.shortDescription);
  detailText("#bpd-short-description-info", post.shortDescription);
  detailText("#bpd-slug", post.slug);
  detailText("#bpd-id", post.id);
  detailText("#bpd-author", detailActor(post.authorName, post.authorId));
  detailText("#bpd-category", post.categoryName);
  detailText("#bpd-display-order", post.displayOrder);
  detailText("#bpd-image-path", post.image);
  detailText("#bpd-image-url", post.imageUrl);
  detailText("#bpd-thumbnail-url", post.thumbnailUrl);

  var status = STATUS_BADGES[post.displayStatus] || STATUS_BADGES.Draft;
  $("#bpd-status").empty().append(AdminTable.badge(status.modifier, status.text));

  if (post.content) $("#bpd-content").html(post.content);
  else $("#bpd-content").text("—");

  detailText("#bpd-created-at", AdminTable.faDateTime(post.createdAt));
  detailText("#bpd-created-by", detailActor(post.createdByName, post.createdBy));
  detailText("#bpd-updated-at", AdminTable.faDateTime(post.updatedAt));
  detailText("#bpd-updated-by", detailActor(post.updatedByName, post.updatedBy));
  detailText("#bpd-is-deleted", post.isDeleted ? "حذف شده" : "خیر");
  detailText("#bpd-deleted-at", AdminTable.faDateTime(post.deletedAt));
  detailText("#bpd-deleted-by", detailActor(post.deletedByName, post.deletedBy));

  $("#bpd-edit")
    .off("click.blogPostDetails")
    .on("click.blogPostDetails", function () {
      AdminCrud.Utils.hideModal("kt_modal_blog_post_details");
      page.openEdit(post.id);
    });
}

function openPostDetails(id) {
  var post = rowsById[id];
  if (!post) return;

  fillPostDetails(post);
  AdminCrud.Utils.showModal("kt_modal_blog_post_details");
}
//#endregion

//#region Row Rendering
/** Fills one row of the eight-column server-side table. */
function fillBlogPostRow(row, post) {
  rowsById[post.id] = post;
  $(row).attr("data-id", post.id);

  var $cells = $("td", row);

  // Order — first column, before the image. The handle and the badge live in the same cell; the
  // unformatted number is kept in data-value because persian-digits.js rewrites the painted label.
  $cells
    .eq(0)
    .addClass("text-center reorder")
    .attr("tabindex", 0)
    .append(
      $("<div class='d-flex align-items-center justify-content-center'>").append(
        '<i class="ki-duotone ki-burger-menu-2 fs-2 text-gray-400 cursor-move me-2">' +
          '<span class="path1"></span><span class="path2"></span><span class="path3"></span>' +
          '<span class="path4"></span><span class="path5"></span><span class="path6"></span>' +
          '<span class="path7"></span><span class="path8"></span><span class="path9"></span>' +
          '<span class="path10"></span></i>',
        $("<span class='badge badge-light-secondary blog-post-order'>")
          .attr("data-value", post.displayOrder)
          .text(AdminTable.faDigits(post.displayOrder)),
      ),
    );

  // Image
  var $image = $("<div class='symbol symbol-45px symbol-2by3'>");
  if (post.thumbnailUrl || post.imageUrl) {
    $image.append(
      $("<img>")
        .attr("src", post.thumbnailUrl || post.imageUrl)
        .attr("alt", post.title),
    );
  } else {
    $image.append(
      $("<div class='symbol-label bg-light-primary text-primary'>").append(
        '<i class="ki-duotone ki-picture fs-2"><span class="path1"></span><span class="path2"></span></i>',
      ),
    );
  }
  $cells.eq(2).addClass("text-center").append($image);

  // Title
  $cells
    .eq(3)
    .append(
      $("<span class='text-gray-800 fw-bold blog-post-title-text'>")
        .attr("title", post.title)
        .text(post.title),
    );

  // Author
  $cells.eq(4).append(
    post.authorName
      ? $("<span class='text-gray-700'>").text(post.authorName)
      : $("<span class='text-muted'>").text("—"),
  );

  // Category
  $cells.eq(5).append(
    post.categoryName
      ? AdminTable.badge("info", post.categoryName)
      : $("<span class='text-muted'>").text("—"),
  );

  // Status
  var status = STATUS_BADGES[post.displayStatus] || STATUS_BADGES.Draft;
  $cells
    .eq(6)
    .addClass("text-center")
    .append(AdminTable.badge(status.modifier, status.text));

  // Created at
  $cells
    .eq(7)
    .append($("<span class='text-gray-600'>").text(AdminTable.faDate(post.createdAt)));

  // Actions
  var $actions = $cells.eq(8).addClass("text-end nowrap-cell");

  var $details = $(
    '<button type="button" class="btn btn-sm btn-icon btn-bg-light btn-active-color-info me-1" title="جزئیات">',
  ).append(
    '<i class="ki-duotone ki-eye fs-2"><span class="path1"></span><span class="path2"></span><span class="path3"></span></i>',
  );
  $details.on("click", function () {
    openPostDetails(post.id);
  });

  var $edit = $(
    '<button type="button" class="btn btn-sm btn-icon btn-bg-light btn-active-color-primary me-1" title="ویرایش">',
  ).append(
    '<i class="ki-duotone ki-pencil fs-2"><span class="path1"></span><span class="path2"></span></i>',
  );
  $edit.on("click", function () {
    page.openEdit(post.id);
  });

  var $delete = $(
    '<button type="button" class="btn btn-sm btn-icon btn-bg-light btn-active-color-danger" title="حذف">',
  ).append(
    '<i class="ki-duotone ki-trash fs-2"><span class="path1"></span><span class="path2"></span><span class="path3"></span><span class="path4"></span><span class="path5"></span></i>',
  );
  $delete.on("click", function () {
    page.openDelete(post.id, post.title);
  });

  $actions.append($details, $edit, $delete);
}
//#endregion

//#region Form Helpers
/** Shows the stored or newly chosen image without a round-trip. */
function setPostImage(url) {
  var $wrapper = $("#Form_Image_wrapper");
  var $preview = $wrapper.find(".image-input-wrapper");
  var background = $wrapper.data("kt-image-input-background");

  $("#Form_Image").val("");
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

/** Initialises the form selects with the shared modal Select2 setup. */
function initFormSelects() {
  var $modal = $("#kt_modal_blog_post");

  ["#Form_CategoryId"].forEach(function (selector) {
    var $select = $(selector);
    if (!$select.length || !$.fn.select2) return;

    AdminTable.modalSelect2($select, {
      dropdownParent: $modal.find(".modal-content"),
      placeholder: $(selector).data("placeholder"),
      width: "100%",
      dropdownAutoWidth: false,
    });
  });

  if ($.fn.select2) {
    AdminTable.filterSelect2($("#Form_DisplayStatus"), {
      dropdownParent: $modal.find(".modal-content"),
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
      selector: "#kt_blog_posts_table",
      options: $.extend(true, AdminCrud.rowReorderOptions(), {
        serverSide: true,
        processing: true,
        searching: false, // the generic picker is the search and filter UI
        order: [],
        pageLength: 10,
        lengthMenu: [10, 25, 50, 100],
        ajax: AdminTable.serverSideAjax({
          conditions: postConditions,
          errorMessage: "خطا در دریافت لیست مقالات.",
        }),
        responsive: {
          details: {
            type: "column",
            target: 1,
            renderer: "listHiddenNodes",
          },
        },
        columns: [
          { data: null, defaultContent: "", name: "", orderable: false },
          { data: null, defaultContent: "", name: "", orderable: false },
          { data: null, defaultContent: "", name: "", orderable: false },
          { data: null, defaultContent: "", name: "Title" },
          { data: null, defaultContent: "", name: "AuthorName" },
          { data: null, defaultContent: "", name: "CategoryName" },
          { data: null, defaultContent: "", name: "DisplayStatus" },
          { data: null, defaultContent: "", name: "CreatedAt" },
          { data: null, defaultContent: "", name: "", orderable: false },
        ],
        columnDefs: [
          { responsivePriority: 1, targets: [0, 1] },
          { responsivePriority: 2, targets: 3 },
          { responsivePriority: 3, targets: 8 },
          { responsivePriority: 4, targets: 6 },
          { responsivePriority: 5, targets: 5 },
          { responsivePriority: 6, targets: 4 },
          { responsivePriority: 7, targets: 2 },
          { responsivePriority: 8, targets: 7 },
          { className: "dtr-control", targets: 1 },
          { orderable: false, targets: [0, 1, 2, 8] },
        ],
        createdRow: fillBlogPostRow,
      }),
    },
    modal: {
      id: "kt_modal_blog_post",
      titleEl: "#modal-blog-post-title",
      submitBtn: "kt_blog_post_submit",
    },
    deleteModal: {
      id: "kt_modal_delete_blog_post",
      nameEl: "#delete-blog-post-name",
    },
    texts: {
      createTitle: "افزودن مقاله جدید",
      editTitle: "ویرایش مقاله",
      created: "مقاله با موفقیت ایجاد شد.",
      updated: "تغییرات با موفقیت ذخیره شد.",
      deleted: "مقاله با موفقیت حذف شد.",
    },

    onModalShown: function () {
      initFormSelects();
      if (!contentHtmlEditor.ready()) contentHtmlEditor.init();
    },
    onModalHidden: function () {
      contentHtmlEditor.destroy();
    },

    loadItem: loadPost,

    fillForm: function (post) {
      post = post || {
        title: "",
        shortDescription: "",
        content: "",
        categoryId: "",
        displayStatus: "Draft",
        displayOrder: 0,
        imageUrl: null,
      };

      $("#Form_Title").val(post.title);
      $("#Form_ShortDescription").val(post.shortDescription);
      $("#Form_Slug").val(post.slug || "");
      slugTouched = !!post.slug;
      $("#Form_DisplayStatus").val(post.displayStatus || "Draft");
      $("#Form_DisplayOrder").val(post.displayOrder || 0);

      // A post already filed under a category that was switched off stays selectable for this row,
      // so the operator can move it instead of losing the stored association.
      $("#Form_CategoryId option").prop("disabled", function () {
        var category = formOptions.categories.filter(function (x) {
          return String(x.id) === String(this.value);
        }, this)[0];
        return !!category && !category.isActive && String(category.id) !== String(post.categoryId);
      });
      $("#Form_CategoryId").val(post.categoryId || "").trigger("change");

      setPostImage(post.imageUrl || post.thumbnailUrl);
      setContent(post.content || "");
      $("#kt_blog_post_form .is-invalid").removeClass("is-invalid");
    },

    getData: function () {
      return {
        title: $("#Form_Title").val().trim(),
        shortDescription: $("#Form_ShortDescription").val().trim(),
        content: getContent(),
        categoryId: $("#Form_CategoryId").val() || "",
        slug: $("#Form_Slug").val().trim().toLowerCase(),
        displayOrder: parseInt($("#Form_DisplayOrder").val(), 10) || 0,
        displayStatus: $("#Form_DisplayStatus").val(),
        removeImage: imageRemoved,
      };
    },

    validate: function () {
      var errors = [];
      var required = [
        { el: "#Form_Title", msg: "عنوان مقاله را وارد کنید." },
        { el: "#Form_ShortDescription", msg: "توضیح کوتاه را وارد کنید." },
        { el: "#Form_Slug", msg: "اسلاگ را وارد کنید." },
        { el: "#Form_CategoryId", msg: "دسته‌بندی مقاله را انتخاب کنید." },
      ];

      required.forEach(function (field) {
        if (AdminCrud.Utils.setInvalid($(field.el), !$(field.el).val().trim()))
          errors.push(field.msg);
      });

      var slug = $("#Form_Slug").val().trim();
      if (slug && !/^[a-z0-9]+(?:-[a-z0-9]+)*$/.test(slug)) {
        AdminCrud.Utils.setInvalid($("#Form_Slug"), true);
        errors.push("اسلاگ فقط می‌تواند شامل حروف لاتین کوچک، عدد و خط تیره باشد.");
      }

      var contentText = String(getContent() || "")
        .replace(/<[^>]*>/g, "")
        .replace(/&nbsp;/g, " ")
        .trim();
      if (!contentText) {
        errors.push("محتوای کامل مقاله را وارد کنید.");
        $("#Form_Content").addClass("is-invalid");
      } else {
        $("#Form_Content").removeClass("is-invalid");
      }

      return errors;
    },

    onSubmit: saveBlogPost,
    onDelete: deleteBlogPost,
  });

  initRowReorder();
}
//#endregion

//#region Form Events
// The slug is built from the Latin title, not the Persian one: the domain only accepts lowercase
// Latin letters, digits and hyphens.
$("#Form_LatinTitle").on("input", function () {
  if (!slugTouched) $("#Form_Slug").val(AdminCrud.Utils.slugify(this.value));
});

$("#Form_Slug").on("input", function () {
  slugTouched = this.value.length > 0;
});

$("#Form_Image").on("change", function () {
  if (this.files && this.files.length) {
    imageRemoved = false;
    $("#Form_RemoveImage").val("0");
  }
});

$("#kt_modal_blog_post").on(
  "click",
  "[data-kt-image-input-action='remove']",
  function () {
    imageRemoved = true;
    $("#Form_RemoveImage").val("1");
  },
);
//#endregion

//#region Inline onclick Handlers
function openCreateModal() {
  if (page) page.openCreate();
}

function openEditModal(id) {
  if (page) page.openEdit(id);
}

function submitBlogPostForm() {
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
$(function () {
  picker = AdminTable.filterPicker({
    fieldEl: "#filterField",
    operatorEl: "#filterOperator",
    valueWrapEl: "#filterValueWrap",
    chipsEl: "#filterChips",
    kinds: blogFilterKinds,
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
