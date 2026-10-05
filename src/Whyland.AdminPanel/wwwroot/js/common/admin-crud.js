/* =========================================================
 * admin-crud.js  —  منطق مشترک صفحات CRUD پنل ادمین
 * وابستگی‌ها: jQuery, DataTables, Bootstrap 5, toastr
 * ========================================================= */
(function (window, $) {
  "use strict";

  // ---------- زبان فارسی DataTable ----------
  var FA_LANG = {
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
  };

  // ---------- توابع کمکی عمومی ----------
  var Utils = {
    slugify: function (text) {
      return text
        .toString()
        .toLowerCase()
        .trim()
        .replace(/[^a-z0-9\u0600-\u06FF\s-]/g, "")
        .replace(/[\s_]+/g, "-")
        .replace(/-+/g, "-")
        .replace(/^-|-$/g, "");
    },
    parseNumber: function (val) {
      var digits = (val || "").replace(/[^\d]/g, "");
      return digits ? Number(digits) : null;
    },
    toFa: function (n) {
      return n === null || n === undefined
        ? ""
        : Number(n).toLocaleString("en-US");
    },
    formatNumberInput: function (el) {
      var digits = el.value.replace(/[^\d]/g, "");
      el.value = digits ? Number(digits).toLocaleString("en-US") : "";
    },
    setInvalid: function ($el, invalid) {
      $el.toggleClass("is-invalid", invalid);
      return invalid;
    },
    showModal: function (id) {
      bootstrap.Modal.getOrCreateInstance(document.getElementById(id)).show();
    },
    hideModal: function (id) {
      var m = bootstrap.Modal.getInstance(document.getElementById(id));
      if (m) m.hide();
    },
  };

  // ---------- DataTable ----------
  /**
   * @param {string} selector      مثل "#kt_courses_table"
   * @param {object} [overrides]   هر آپشن DataTables که بخواهی بازنویسی شود
   */
  function createTable(selector, overrides) {
    var defaults = {
      responsive: true,
      order: [],
      pageLength: 25,
      columnDefs: [{ orderable: false, targets: [-1] }],
      language: FA_LANG,
    };
    return $(selector).DataTable($.extend(true, {}, defaults, overrides || {}));
  }

  // ---------- فیلترهای سفارشی (چندتایی، بدون تداخل) ----------
  /**
   * @param {DataTable} table
   * @param {string}    tableId   id جدول بدون #
   * @param {Array}     filters   [{ el:"#statusFilter", test:function(row$, val){...} }]
   * هر فیلتر وقتی مقدارش خالی باشد نادیده گرفته می‌شود.
   */
  function bindFilters(table, tableId, filters) {
    $.fn.dataTable.ext.search.push(function (settings, data, dataIndex) {
      if (settings.nTable.id !== tableId) return true;
      var $row = $(table.row(dataIndex).node());
      for (var i = 0; i < filters.length; i++) {
        var val = $(filters[i].el).val();
        if (val && !filters[i].test($row, val)) return false;
      }
      return true;
    });

    var selectors = filters
      .map(function (f) {
        return f.el;
      })
      .join(", ");
    $(selectors).on("change", function () {
      table.draw();
    });
  }

  // ---------- فیلترهای آماده ----------
  var Filters = {
    // data-<attr>="value"  برابر باشد
    dataEquals: function (attr) {
      return function ($row, val) {
        return String($row.data(attr)) === String(val);
      };
    },
    // data-<attr>="1,2,3"  شامل مقدار باشد
    dataContains: function (attr) {
      return function ($row, val) {
        return (
          String($row.data(attr) || "")
            .split(",")
            .indexOf(val) !== -1
        );
      };
    },
  };

  // ---------- صفحه CRUD ----------
  /**
   * @param {object} cfg
   *  table:        { selector, options }
   *  search:       "#searchInput"
   *  filters:      [{ el, test }]
   *  modal:        { id, titleEl, submitBtn }
   *  deleteModal:  { id, nameEl }
   *  texts:        { createTitle, editTitle, created, updated, deleted }
   *  fillForm(data | null)   پر کردن فرم؛ null یعنی حالت ایجاد
   *  getData()               خواندن فرم → آبجکت
   *  validate()              آرایه خطاها (اختیاری)
   *  loadItem(id)            داده آیتم برای ویرایش (اختیاری؛ می‌تواند Promise هم برگرداند)
   *  onSubmit(data, mode)    ارسال به سرور (اختیاری؛ باید Promise/jqXHR برگرداند)
   *  onDelete(id)            حذف در سرور (اختیاری؛ Promise)
   *  onModalShown / onModalHidden  (اختیاری)
   */
  function createCrudPage(cfg) {
    var tableId = cfg.table.selector.replace("#", "");
    var table = createTable(cfg.table.selector, cfg.table.options);
    var state = { mode: "create", editId: null, deleteId: null };

    // جستجو
    if (cfg.search) {
      $(cfg.search).on("keyup", function () {
        table.search(this.value).draw();
      });
    }

    // فیلترها
    if (cfg.filters && cfg.filters.length) {
      bindFilters(table, tableId, cfg.filters);
    }

    // رویدادهای مودال
    var modalEl = document.getElementById(cfg.modal.id);
    if (cfg.onModalShown)
      modalEl.addEventListener("shown.bs.modal", cfg.onModalShown);
    if (cfg.onModalHidden)
      modalEl.addEventListener("hidden.bs.modal", cfg.onModalHidden);

    // ----- ایجاد -----
    function openCreate() {
      state.mode = "create";
      state.editId = null;
      $(cfg.modal.titleEl).text(cfg.texts.createTitle);
      cfg.fillForm(null);
      Utils.showModal(cfg.modal.id);
    }

    // ----- ویرایش -----
    function openEdit(id) {
      state.mode = "edit";
      state.editId = id;
      $(cfg.modal.titleEl).text(cfg.texts.editTitle);
      $.when(cfg.loadItem ? cfg.loadItem(id) : null).done(function (item) {
        cfg.fillForm(item);
        Utils.showModal(cfg.modal.id);
      });
    }

    // ----- ارسال فرم -----
    function submit() {
      if (cfg.validate) {
        var errors = cfg.validate();
        if (errors && errors.length) {
          toastr.error(errors[0], "خطا");
          return;
        }
      }

      var data = cfg.getData();
      data.id = state.editId;

      var btn = document.getElementById(cfg.modal.submitBtn);
      btn.setAttribute("data-kt-indicator", "on");
      btn.disabled = true;

      function finish() {
        btn.removeAttribute("data-kt-indicator");
        btn.disabled = false;
      }
      function success() {
        finish();
        Utils.hideModal(cfg.modal.id);
        toastr.success(
          state.mode === "create" ? cfg.texts.created : cfg.texts.updated,
          "موفق",
        );
      }

      if (cfg.onSubmit) {
        $.when(cfg.onSubmit(data, state.mode))
          .done(success)
          .fail(function () {
            finish();
            toastr.error("خطا در ذخیره اطلاعات.", "خطا");
          });
      } else {
        console.log(data);
        setTimeout(success, 800);
      }
    }

    // ----- حذف -----
    function openDelete(id, name) {
      state.deleteId = id;
      $(cfg.deleteModal.nameEl).text(name);
      Utils.showModal(cfg.deleteModal.id);
    }

    function confirmDelete() {
      function done() {
        Utils.hideModal(cfg.deleteModal.id);
        toastr.success(cfg.texts.deleted, "موفق");
      }
      if (cfg.onDelete) {
        $.when(cfg.onDelete(state.deleteId))
          .done(done)
          .fail(function () {
            toastr.error("خطا در حذف.", "خطا");
          });
      } else {
        done();
      }
    }

    if (typeof KTSelect2 !== "undefined") KTSelect2.init();

    return {
      table: table,
      state: state,
      openCreate: openCreate,
      openEdit: openEdit,
      submit: submit,
      openDelete: openDelete,
      confirmDelete: confirmDelete,
    };
  }

  window.AdminCrud = {
    createPage: createCrudPage,
    createTable: createTable,
    bindFilters: bindFilters,
    Filters: Filters,
    Utils: Utils,
    FA_LANG: FA_LANG,
  };
})(window, jQuery);
