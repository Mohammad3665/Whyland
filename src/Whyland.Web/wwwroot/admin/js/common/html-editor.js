/**
 * html-editor.js — Generic TinyMCE rich-text editor wrapper shared by all
 * admin pages that need an HTML editor field (courses description, blog
 * post content, ...).
 *
 * Dependencies: jQuery, TinyMCE (assets/plugins/custom/tinymce/tinymce.bundle.js)
 *
 * Usage inside a page script:
 *
 *   var descEditor = HtmlEditor.create({
 *     selector:  "#Form_Description",   // textarea to upgrade (usually hidden inside a modal)
 *     height:    420,                   // optional, default 420
 *     minHeight: 320,                   // optional
 *     rtl:       true,                  // optional, default true
 *   });
 *
 *   Fill / read the field:
 *     descEditor.set("...html...");
 *     descEditor.get();                 // => html
 *
 *   Lifecycle (create/destroy with the modal so content stays fresh):
 *     modal "shown.bs.modal"  -> if (!descEditor.ready()) descEditor.init();
 *     modal "hidden.bs.modal" -> descEditor.destroy(); descEditor = null;
 *
 *   Or let AdminCrud drive it via hooks:
 *     onModalShown:  function () { if (!tinymce.get(id)) descEditor.init(); },
 *     onModalHidden: function () { descEditor.destroy(); descEditor = null; }
 */
(function (window, $) {
  "use strict";

  /**
   * Creates (but does not yet initialize) one editor instance bound to a
   * textarea. Call init() when its host modal becomes visible — TinyMCE
   * mis-renders the inline toolbar when created inside a hidden modal.
   *
   * @param {object} cfg
   *   selector:  textarea selector (required)
   *   height:    editor height in px (default 420)
   *   minHeight: minimum height in px (default 320)
   *   rtl:       RTL editing (default true)
   * @returns {{
   *   cfg: object, init: Function, destroy: Function, ready: Function,
   *   set: Function, get: Function
   * }}
   */
  function create(cfg) {
    cfg = $.extend(
      {
        selector: "",
        height: 420,
        minHeight: 320,
        rtl: true,
      },
      cfg || {},
    );

    if (!cfg.selector) {
      throw new Error("HtmlEditor.create: 'selector' is required");
    }

    var editorId = cfg.selector.replace(/^#/, "");
    var pending = null; // HTML to apply once the editor finishes initializing
    var instance = null; // active TinyMCE editor (null until init())

    /** Initializes TinyMCE (idempotent — does nothing when already live). */
    function init() {
      if (instance || typeof tinymce === "undefined") return;

      // Option names differ between TinyMCE 5 and 6+; detect the major version.
      var ver = parseInt(tinymce.majorVersion, 10) || 5;
      var isNew = ver >= 6;

      var options = {
        selector: cfg.selector,
        readonly: 0,
        directionality: cfg.rtl ? "rtl" : "ltr",
        height: cfg.height,
        min_height: cfg.minHeight,
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
        // Pick a local image file and insert it as a base64 data URL.
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
          "body { font-family: Vazirmatn, IRANSans, Tahoma, sans-serif; font-size: 15px; line-height: 1.9; direction: " +
          (cfg.rtl ? "rtl" : "ltr") +
          "; padding: 12px; }" +
          " img { max-width: 100%; height: auto; }" +
          " table { border-collapse: collapse; } table td, table th { border: 1px solid #ddd; padding: 6px 10px; }",
        setup: function (ed) {
          ed.on("init", function () {
            instance = ed;
            // Force design (visual) mode in both TinyMCE 5 and 6+.
            try {
              if (ed.mode && ed.mode.set) ed.mode.set("design");
              else if (ed.setMode) ed.setMode("design");
            } catch (e) {}
            // Apply any content that was set before the editor finished loading.
            if (pending !== null) ed.setContent(pending);
          });
        },
      };

      // TinyMCE 6+ only: self-hosted GPL build settings.
      if (isNew) {
        options.license_key = "gpl";
        options.promotion = false;
        options.font_size_formats = options.fontsize_formats;
      }

      // Match the admin panel's dark theme when it is active.
      if (typeof KTThemeMode !== "undefined" && KTThemeMode.getMode() === "dark") {
        options["skin"] = "oxide-dark";
        options["content_css"] = "dark";
      }

      tinymce.init(options);
    }

    /** Removes the editor instance and its pending state. */
    function destroy() {
      var ed = tinymce.get(editorId);
      if (ed) ed.remove();
      instance = null;
      pending = null;
    }

    /** True when the editor instance exists and is ready for content. */
    function ready() {
      return !!tinymce.get(editorId);
    }

    /** Sets the content (queued until the editor exists, if needed). */
    function set(html) {
      pending = html || "";
      var ed = tinymce.get(editorId);
      if (ed) ed.setContent(pending);
      else $(cfg.selector).val(pending);
    }

    /** Returns the current editor content as HTML. */
    function get() {
      var ed = tinymce.get(editorId);
      return ed ? ed.getContent() : $(cfg.selector).val();
    }

    return {
      cfg: cfg,
      init: init,
      destroy: destroy,
      ready: ready,
      set: set,
      get: get,
    };
  }

  window.HtmlEditor = {
    create: create,
  };
})(window, jQuery);
