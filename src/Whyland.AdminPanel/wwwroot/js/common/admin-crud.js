/* =========================================================
 * admin-crud.js — Shared logic for the admin panel CRUD pages
 * Dependencies: jQuery, DataTables, Bootstrap 5, toastr
 * ========================================================= */
(function (window, $) {
  "use strict";

  //#region DataTables Persian Language Pack
  // Localized strings handed to every DataTable created by this module.
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
  //#endregion

  //#region General Purpose Helpers
  var Utils = {
    /** Turns a Latin/Persian title into a URL-friendly slug. */
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

    /** Strips non-digit characters and returns a number (null when empty). */
    parseNumber: function (val) {
      var digits = (val || "").replace(/[^\d]/g, "");
      return digits ? Number(digits) : null;
    },

    /** Formats a number with thousands separators ("" for null/undefined). */
    toFa: function (n) {
      return n === null || n === undefined
        ? ""
        : Number(n).toLocaleString("en-US");
    },

    /** Re-formats a numeric text input while the user types. */
    formatNumberInput: function (el) {
      var digits = el.value.replace(/[^\d]/g, "");
      el.value = digits ? Number(digits).toLocaleString("en-US") : "";
    },

    /** Toggles Bootstrap's "is-invalid" class; returns the new state. */
    setInvalid: function ($el, invalid) {
      $el.toggleClass("is-invalid", invalid);
      return invalid;
    },

    /** Shows the Bootstrap modal with the given element id. */
    showModal: function (id) {
      bootstrap.Modal.getOrCreateInstance(document.getElementById(id)).show();
    },

    /** Hides the Bootstrap modal with the given element id (if instantiated). */
    hideModal: function (id) {
      var m = bootstrap.Modal.getInstance(document.getElementById(id));
      if (m) m.hide();
    },
  };
  //#endregion

  //#region DataTable Factory
  /**
   * Creates a DataTable with sensible admin defaults.
   * @param {string} selector      e.g. "#kt_courses_table"
   * @param {object} [overrides]   any DataTables option to override
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
  //#endregion

  //#region Custom (Combinable) Filters
  /**
   * Registers multiple independent filters on a single table.
   * @param {DataTable} table
   * @param {string}    tableId   table id without the leading "#"
   * @param {Array}     filters   [{ el:"#statusFilter", test:function(row$, val){...} }]
   * A filter is ignored whenever its current value is empty.
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

    // Re-draw the table whenever any filter control changes.
    var selectors = filters
      .map(function (f) {
        return f.el;
      })
      .join(", ");
    $(selectors).on("change", function () {
      table.draw();
    });
  }
  //#endregion

  //#region Ready-made Filter Predicates
  var Filters = {
    /** Row matches when data-<attr>="value" equals the filter value. */
    dataEquals: function (attr) {
      return function ($row, val) {
        return String($row.data(attr)) === String(val);
      };
    },
    /** Row matches when the comma-separated data-<attr> contains the value. */
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
  //#endregion

  //#region CRUD Page Controller
  /**
   * Wires up search, filters, create/edit modals and delete confirmation
   * for one admin list page.
   *
   * @param {object} cfg
   *  table:        { selector, options }
   *  search:       "#searchInput"
   *  filters:      [{ el, test }]
   *  modal:        { id, titleEl, submitBtn }
   *  deleteModal:  { id, nameEl }
   *  texts:        { createTitle, editTitle, created, updated, deleted }
   *  fillForm(data | null)   fill the form; null means "create" mode
   *  getData()               read the form into an object
   *  validate()              return an array of error messages (optional)
   *  loadItem(id)            fetch the item to edit (optional; may return a Promise)
   *  onSubmit(data, mode)    send to the server (optional; return Promise/jqXHR)
   *  onDelete(id)            delete on the server (optional; return a Promise)
   *  onModalShown / onModalHidden  (optional)
   */
  function createCrudPage(cfg) {
    var tableId = cfg.table.selector.replace("#", "");
    var table = createTable(cfg.table.selector, cfg.table.options);
    var state = { mode: "create", editId: null, deleteId: null };

    //#region Search Box Binding
    if (cfg.search) {
      $(cfg.search).on("keyup", function () {
        table.search(this.value).draw();
      });
    }
    //#endregion

    //#region Filter Bindings
    if (cfg.filters && cfg.filters.length) {
      bindFilters(table, tableId, cfg.filters);
    }
    //#endregion

    //#region Modal Lifecycle Hooks
    var modalEl = document.getElementById(cfg.modal.id);
    if (cfg.onModalShown)
      modalEl.addEventListener("shown.bs.modal", cfg.onModalShown);
    if (cfg.onModalHidden)
      modalEl.addEventListener("hidden.bs.modal", cfg.onModalHidden);
    //#endregion

    //#region Create
    function openCreate() {
      state.mode = "create";
      state.editId = null;
      $(cfg.modal.titleEl).text(cfg.texts.createTitle);
      cfg.fillForm(null);
      Utils.showModal(cfg.modal.id);
    }
    //#endregion

    //#region Edit
    function openEdit(id) {
      state.mode = "edit";
      state.editId = id;
      $(cfg.modal.titleEl).text(cfg.texts.editTitle);
      $.when(cfg.loadItem ? cfg.loadItem(id) : null).done(function (item) {
        cfg.fillForm(item);
        Utils.showModal(cfg.modal.id);
      });
    }
    //#endregion

    //#region Submit (Create / Edit)
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

      // Show the busy indicator on the submit button while saving.
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
        // No server handler wired up yet: simulate a successful save.
        console.log(data);
        setTimeout(success, 800);
      }
    }
    //#endregion

    //#region Delete
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
    //#endregion

    // Initialize Select2 on any [data-control="select2"] element.
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
  //#endregion

  // Public API of the module.
  window.AdminCrud = {
    createPage: createCrudPage,
    createTable: createTable,
    bindFilters: bindFilters,
    Filters: Filters,
    Utils: Utils,
    FA_LANG: FA_LANG,
  };
})(window, jQuery);
