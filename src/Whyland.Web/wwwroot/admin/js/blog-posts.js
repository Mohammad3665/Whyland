//#region Blog Posts Page Scripts
/**
 * Admin "Blog Posts" list page: DataTable, filters and the full-screen
 * create/edit modal. The rich-text «محتوای کامل» editor is the SHARED
 * generic HtmlEditor module (js/common/html-editor.js) — the same code
 * the Courses page uses; nothing editor-specific is duplicated here.
 * Built on top of the shared AdminCrud module (admin-crud.js).
 */

//#region Imports
var U = AdminCrud.Utils;
var F = AdminCrud.Filters;
//#endregion

//#region Content Editor (shared generic module)
var contentHtmlEditor = window.HtmlEditor.create({
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

var slugTouched = false; // True once the user edits the slug manually
//#endregion

//#region Image Field Helper
/** Renders the post-image picker: shows the given url or resets to empty. */
function setPostImage(url) {
  var $w = $("#Form_Image_wrapper");
  $("#Form_Image").val("");
  $w.find('input[name="Image_remove"]').val("");
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
    selector: "#kt_blog_posts_table",
    options: {
      pageLength: 10,
    },
  },
  search: "#blogPostSearch",
  filters: [
    { el: "#statusFilter", test: F.dataEquals("status") },
    { el: "#categoryFilter", test: F.dataContains("categories") },
  ],
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

  // Create/destroy TinyMCE with the modal so its content stays fresh.
  onModalShown: function () {
    if (!contentHtmlEditor.ready()) contentHtmlEditor.init();
  },
  onModalHidden: function () {
    contentHtmlEditor.destroy();
  },

  loadItem: function (id) {
    // TODO: fetch the item from the server instead of the local sample data.
    return blogPostsData[id];
  },

  // Populate all blog post form fields from a post object.
  fillForm: function (p) {
    p = p || {
      title: "",
      latinTitle: "",
      slug: "",
      shortDescription: "",
      content: "",
      authorId: "1",
      categoryId: "",
      displayStatus: "Draft",
      displayOrder: 0,
      image: null,
    };
    $("#Form_Title").val(p.title);
    $("#Form_LatinTitle").val(p.latinTitle);
    $("#Form_Slug").val(p.slug);
    slugTouched = !!p.slug;
    $("#Form_ShortDescription").val(p.shortDescription);
    setContent(p.content);
    $("#Form_DisplayStatus").val(p.displayStatus);
    $("#Form_DisplayOrder").val(p.displayOrder);
    $("#Form_AuthorId").val(p.authorId).trigger("change");
    $("#Form_CategoryId")
      .val(p.categoryId ?? "")
      .trigger("change");
    setPostImage(p.image);
    $("#kt_blog_post_form .is-invalid").removeClass("is-invalid");
  },

  // Client-side validation; returns the list of error messages.
  validate: function () {
    var errors = [];
    if (U.setInvalid($("#Form_Title"), !$("#Form_Title").val().trim()))
      errors.push("عنوان مقاله را وارد کنید.");
    if (
      U.setInvalid(
        $("#Form_ShortDescription"),
        !$("#Form_ShortDescription").val().trim(),
      )
    )
      errors.push("توضیح کوتاه را وارد کنید.");
    if (U.setInvalid($("#Form_Slug"), !$("#Form_Slug").val().trim()))
      errors.push("اسلاگ را وارد کنید.");
    if (U.setInvalid($("#Form_AuthorId"), !$("#Form_AuthorId").val()))
      errors.push("نویسنده را انتخاب کنید.");
    return errors;
  },

  // Collect the form into the blog post data object sent to the server.
  getData: function () {
    return {
      title: $("#Form_Title").val().trim(),
      latinTitle: $("#Form_LatinTitle").val().trim(),
      slug: $("#Form_Slug").val().trim(),
      shortDescription: $("#Form_ShortDescription").val().trim(),
      content: getContent(),
      authorId: $("#Form_AuthorId").val(),
      categoryId: $("#Form_CategoryId").val() || null,
      displayStatus: $("#Form_DisplayStatus").val(),
      displayOrder: parseInt($("#Form_DisplayOrder").val(), 10) || 0,
    };
  },
});
//#endregion

//#region Blog-Post-Specific Form Events
// Auto-generate the slug from the Latin title until the user edits it manually.
$("#Form_LatinTitle").on("input", function () {
  if (!slugTouched) $("#Form_Slug").val(U.slugify(this.value));
});
$("#Form_Slug").on("input", function () {
  slugTouched = this.value.length > 0;
});
//#endregion

//#region Inline onclick Handlers (HTML compatibility)
function openCreateModal() {
  page.openCreate();
}
function openEditModal(id) {
  page.openEdit(id);
}
function submitBlogPostForm() {
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
var blogPostsData = {
  1: {
    title: "معرفی ASP.NET Core 8",
    latinTitle: "ASP.NET Core 8 intro",
    slug: "aspnet-core-8-intro",
    shortDescription: "نگاهی به ویژگی‌های جدید",
    content:
      "<h2>ASP.NET Core 8</h2><p>از قابلیت‌های جدید این نسخه می‌توان به … اشاره کرد.</p>",
    authorId: "1",
    categoryId: "2",
    displayStatus: "Published",
    displayOrder: 1,
    image: "/assets/media/stock/600x400/img-8.jpg",
  },
  2: {
    title: "راهنمای شروع با Blazor",
    latinTitle: "Blazor getting started",
    slug: "blazor-getting-started",
    shortDescription: "آموزش پایه Blazor",
    content: "<p>Blazor یک فریم‌ورک …</p>",
    authorId: "2",
    categoryId: "1",
    displayStatus: "Published",
    displayOrder: 2,
    image: "/assets/media/stock/600x400/img-9.jpg",
  },
  3: {
    title: "گام‌های اول SQL Server",
    latinTitle: "SQL Server first steps",
    slug: "sql-server-first-steps",
    shortDescription: "آشنایی با SQL Server",
    content: "",
    authorId: "3",
    categoryId: "3",
    displayStatus: "Draft",
    displayOrder: 3,
    image: null,
  },
  4: {
    title: "بازاریابی محتوایی برای دوره‌های آنلاین",
    latinTitle: "Content marketing for online courses",
    slug: "content-marketing-online-courses",
    shortDescription: "استراتژی محتوا برای آموزش باکیفیت",
    content: "<p>…</p>",
    authorId: "1",
    categoryId: "5",
    displayStatus: "Hidden",
    displayOrder: 4,
    image: "/assets/media/stock/600x400/img-10.jpg",
  },
};
//#endregion
//#endregion
