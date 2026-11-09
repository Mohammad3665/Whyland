/* =========================================================
 * admin-table.js — shared logic for the server-side admin tables
 * Dependencies: jQuery, DataTables, toastr
 *
 * What is here and why: every list page in the panel shows the same three things — a table
 * that pages and sorts on the server, the generic filter picker built from ?handler=Fields,
 * and the {isSuccess, message} round-trip of a modal POST. Two pages written against that
 * contract had already started to grow the same 300 lines each (the picker, the chip list,
 * the digit and date helpers), which is how two identical-looking filter boxes end up
 * behaving differently. The shared parts live here once; a page keeps only what is genuinely
 * its own — its columns, its modals, and the value loaders of whatever exotic field kind it adds.
 * ========================================================= */
(function (window, $) {
  "use strict";

  //#region Persian labels
  /** Operator symbols/labels for the picker, keyed by the FilterOperator names. */
  var OPERATOR_LABELS = {
    Equals: "مساوی با",
    NotEquals: "نامساوی با",
    Contains: "شامل",
    StartsWith: "شروع با",
    EndsWith: "ختم به",
    GreaterThan: "بزرگتر از",
    GreaterThanOrEqual: "بزرگتر یا مساوی با",
    LessThan: "کوچکتر از",
    LessThanOrEqual: "کوچکتر یا مساوی با",
    In: "یکی از",
  };

  /** Persian labels for the values a bool or an enum field can be filtered on. */
  var VALUE_LABELS = {
    true: "فعال",
    false: "غیرفعال",
    Male: "مرد",
    Female: "زن",
  };

  /** Reads a value label, falling back to the raw option so nothing renders empty. */
  function valueLabel(value) {
    if (value === "" || value === null || value === undefined) return "";
    return Object.prototype.hasOwnProperty.call(VALUE_LABELS, value)
      ? VALUE_LABELS[value]
      : String(value);
  }
  //#endregion

  //#region Server Connectivity (Razor Page handlers)
  /** The page URL of a handler, e.g. "/Admin/Users?handler=Rows". */
  function handlerUrl(name) {
    return location.pathname + "?handler=" + name;
  }

  /** Reads the antiforgery token rendered by @Html.AntiForgeryToken(). */
  function csrfToken() {
    return $('input[name="__RequestVerificationToken"]').val();
  }

  /** Pulls the server-side message out of a failed response. */
  function serverMessage(xhr, fallback) {
    return (xhr && xhr.responseJSON && xhr.responseJSON.message) || fallback;
  }

  /**
   * Sends a form-encoded POST to one handler and adapts the {isSuccess, message} envelope to
   * the AdminCrud contract: resolves only when isSuccess is true, otherwise rejects with
   * {responseJSON} so the shared module surfaces the server message. Arrays are serialized
   * traditionally (permissionIds=1&permissionIds=2) — jQuery's default "permissionIds[]" key
   * would not bind.
   */
  function postHandler(handler, data) {
    var dfd = $.Deferred();
    $.ajax({
      url: handlerUrl(handler),
      type: "POST",
      data: $.param(data, true),
      headers: { RequestVerificationToken: csrfToken() },
    })
      .done(function (res) {
        if (res && res.isSuccess) dfd.resolve(res);
        else dfd.reject({ responseJSON: res });
      })
      .fail(function (xhr) {
        dfd.reject(xhr);
      });
    return dfd.promise();
  }
  //#endregion

  //#region Filter selects (Select2)
  /**
   * Places the list below the box, whatever room the viewport has left. This is the bundled
   * Select2's own AttachBody._positionDropdown - the single function that writes the offsets on
   * open, on scroll and on resize - with the viewport tests and the flip to "above" taken out.
   * Overriding that function instead of repositioning after "select2:open" is what keeps the list
   * from being painted on the wrong side for the first frame.
   */
  function positionBelowField() {
    var $offsetParent = this.$dropdownParent;

    // A statically positioned parent is measured against the element that really offsets it.
    if ($offsetParent.css("position") === "static")
      $offsetParent = $offsetParent.offsetParent();

    var offset = this.$container.offset();
    var height = this.$container.outerHeight(false);
    var width = this.$container.outerWidth(false);
    var parentOffset = { top: 0, left: 0 };

    if (
      $.contains(document.body, $offsetParent[0]) ||
      $offsetParent[0].isConnected
    )
      parentOffset = $offsetParent.offset();

    // Both edges are written, and the list is pinned to the same box: left alone would let a
    // `right` from the RTL stylesheet win over it, and a list of its own natural width would end
    // up beside the field instead of under it.
    this.$dropdownContainer.css({
      position: "absolute",
      left: offset.left - parentOffset.left,
      right: "auto",
      top: offset.top + height - parentOffset.top,
      width: width,
    });

    this.$dropdown.css({
      position: "relative",
      top: "auto",
      left: "auto",
      right: "auto",
      width: "100%",
    });

    // The direction classes are what round the corners of the box and the list.
    this.$dropdown
      .removeClass("select2-dropdown--above")
      .addClass("select2-dropdown--below");
    this.$container
      .removeClass("select2-container--above")
      .addClass("select2-container--below");
  }

  /**
   * The one Select2 setup every filter select of the panel gets: the list opens below the box,
   * shows at most six rows and then scrolls (both are sized in css/site.css), and a select with
   * fewer than six options keeps no search box, so a two-operator field is still a plain select.
   *
   * width: "element" is not a detail. The bundled theme sets the defaults to width "100%" and
   * selectionCssClass ":all:", so left alone the wrapper spans the whole filter row: the three
   * boxes stack one under another and, because the list is anchored to the wrapper's left edge
   * while an RTL box sits at its right, the options open on the far side of the page. Measuring
   * the select itself gives each wrapper its own w-175px/w-150px/w-250px width instead.
   *
   * Re-initialising destroys the previous instance first: the value control of the picker is a
   * fresh element every time the field changes, and Select2 refuses a second instance over a first.
   */
  function filterSelect2($el, options) {
    if (!$el.length) return $el;

    if ($el.data("select2")) $el.select2("destroy");

    $el.select2(
      $.extend(
        true,
        {
          dir: "rtl",
          width: "element",
          containerCssClass: "kt-filter-select2",
          dropdownCssClass: "kt-filter-select2",
          minimumResultsForSearch: 6,
        },
        options || {},
      ),
    );

    var instance = $el.data("select2");
    if (instance && instance.dropdown) instance.dropdown._positionDropdown = positionBelowField;

    // The search box sits above the scrolling results list, not inside it. Without this, a wheel
    // gesture over the focused search box scrolls the page while the list under it stays put; an
    // operator who types a query first naturally keeps the pointer there.
    $el.off("select2:open.ktFilterWheel").on("select2:open.ktFilterWheel", function () {
      var current = $el.data("select2");
      var $dropdown = current && current.$dropdown;
      if (!$dropdown) return;

      var $results = $dropdown.find(".select2-results__options");
      $dropdown
        .find(".select2-search__field")
        .off("wheel.ktFilterWheel")
        .on("wheel.ktFilterWheel", function (event) {
          var wheel = event.originalEvent;
          if (!wheel || !$results.length) return;

          $results[0].scrollTop += wheel.deltaY;
          event.preventDefault();
        });
    });

    return $el;
  }
  //#endregion

  //#region Formatting helpers
  /** Rewrites Persian (۰-۹) and Arabic-Indic (٠-٩) digits as Latin ones. */
  function latinDigits(value) {
    return String(value === null || value === undefined ? "" : value)
      .replace(/[۰-۹]/g, function (d) {
        return String("۰۱۲۳۴۵۶۷۸۹".indexOf(d));
      })
      .replace(/[٠-٩]/g, function (d) {
        return String("٠١٢٣٤٥٦٧٨٩".indexOf(d));
      });
  }

  /** A number written with Persian digits, the way the panel shows every count. */
  function faDigits(n) {
    return PersianDigits.toPersian(n);
  }

  /** Formats a UTC timestamp as a Persian date. */
  function faDate(value) {
    if (!value) return "";
    var date = new Date(value);
    return isNaN(date.getTime()) ? "" : date.toLocaleDateString("fa-IR");
  }

  /** Formats a UTC timestamp as a Persian date with time. */
  function faDateTime(value) {
    if (!value) return "";
    var date = new Date(value);
    if (isNaN(date.getTime())) return "";
    return (
      date.toLocaleDateString("fa-IR") +
      " - " +
      date.toLocaleTimeString("fa-IR", { hour: "2-digit", minute: "2-digit" })
    );
  }

  /** A Metronic soft badge, built as a node so the caller can append anything to it. */
  function badge(modifier, text) {
    return $('<span class="badge d-inline-block me-1 mb-1">')
      .addClass("badge-light-" + modifier)
      .text(text);
  }

  /** DataTables' Persian strings, merged into the options of any table created by a page. */
  function serverSideAjax(cfg) {
    return $.extend(
      true,
      {
        // Every list page answers its table from ?handler=Rows, so the URL needs no argument
        // unless a page names the endpoint differently.
        url: cfg.url || handlerUrl("Rows"),
        type: "GET",
        data: function (d) {
          // DataTables 2 hands over the clicked column as { column, dir, name }; the name is the
          // entity property each column declares, which is what the server resolves.
          var order = (d.order || [])[0];
          if (order) {
            var name =
              order.name || ((d.columns || [])[order.column] || {}).name || "";
            d.sortColumn = name;
            d.sortColumnDirection = order.dir || "";
          }
          // Indexed keys are what the ASP.NET Core binder expects for List<FilterCondition>.
          var active = cfg.conditions();
          for (var i = 0; i < active.length; i++) {
            d["Filters[" + i + "].Property"] = active[i].property;
            d["Filters[" + i + "].Operator"] = active[i].operator;
            d["Filters[" + i + "].Value"] = active[i].value;
          }
        },
        error: function (xhr) {
          toastr.error(serverMessage(xhr, cfg.errorMessage), "خطا");
        },
      },
      cfg.options || {},
    );
  }
  //#endregion

  //#region Filter value helpers
  /**
   * option request → [{ id, … }]: every list a filter picker reads from the server, kept so a
   * field asks once. Both halves of the picker need the same answer — the value box is dressed
   * with it, and a chip is named with it after the box has been thrown away and rebuilt.
   */
  var optionDirectories = {};

  /** The cache key of an option request: its endpoint together with its arguments. */
  function optionsKey(handler, params) {
    return handlerUrl(handler) + "?" + $.param(params || {});
  }

  /**
   * Reads one option list from its page endpoint, once.
   * @param {string} handler the endpoint name, e.g. "UserOptions" or "RoleOptions".
   * @param {object} [params] the query arguments of that endpoint.
   */
  function loadOptions(handler, params) {
    var key = optionsKey(handler, params);
    if (optionDirectories[key])
      return $.Deferred().resolve(optionDirectories[key]).promise();

    return $.getJSON(handlerUrl(handler), params || {}).then(function (res) {
      optionDirectories[key] = (res && res.data) || [];
      return optionDirectories[key];
    });
  }

  /** The text an option shows: its name, unless the kind reads its list a different way. */
  function optionText(option, label) {
    return label ? label(option) : option.name;
  }

  /** Appends every entry of one option list to the picker; the box is built without waiting. */
  function fillOptions($select, handler, params, emptyText, label) {
    loadOptions(handler, params)
      .done(function (list) {
        if (!list.length) {
          $select.append($("<option>").val("").text(emptyText));
          return;
        }

        list.forEach(function (option) {
          $select.append(
            $("<option>").val(option.id).text(optionText(option, label)),
          );
        });
      })
      .fail(function () {
        toastr.error("خطا در دریافت لیست گزینه‌ها.", "خطا");
      });
  }

  /** Turns a comma-separated list of ids into the labels the admin picked. */
  function optionLabels(handler, params, value, label) {
    var directory = optionDirectories[optionsKey(handler, params)];
    if (!directory) return value;

    var byId = {};
    directory.forEach(function (option) {
      byId[option.id] = optionText(option, label);
    });

    return value
      .split(",")
      .map(function (id) {
        return byId[id] || id;
      })
      .join("، ");
  }

  /** A mutation changes who and what can be offered, so no cached list is still complete. */
  function resetOptionDirectories() {
    optionDirectories = {};
  }

  /** The Persian wording of a field's allowed range, empty when the value is unbounded. */
  function rangeHint(field) {
    if (!field || field.kind !== "number") return "";

    var hasMin = field.min !== null && field.min !== undefined;
    var hasMax = field.max !== null && field.max !== undefined;
    if (!hasMin && !hasMax) return "";
    if (hasMin && hasMax) return faDigits(field.min) + " تا " + faDigits(field.max);
    return hasMax
      ? "تا " + faDigits(field.max)
      : "از " + faDigits(field.min) + " به بعد";
  }

  /**
   * Whether a typed value is usable for the chosen field. A field that declares a range — the tree
   * level is one — cannot be searched outside it: the walk stops here, so no request asks for a
   * value that cannot exist.
   */
  function inRange(field, value) {
    if (!rangeHint(field)) return true;

    return value
      .split(",")
      .filter(function (x) {
        return x !== "";
      })
      .every(function (part) {
        var number = Number(part);
        if (isNaN(number) || String(number) !== part) return false;
        if (
          field.min !== null &&
          field.min !== undefined &&
          number < Number(field.min)
        )
          return false;
        if (
          field.max !== null &&
          field.max !== undefined &&
          number > Number(field.max)
        )
          return false;
        return true;
      });
  }

  /** Marks the value box the way the rest of the admin marks an unusable input. */
  function markInvalid(invalid) {
    $("#filterValue").toggleClass("is-invalid", invalid);
  }
  //#endregion

  //#region Generic filter picker
  /**
   * Field kinds this module renders itself: the two whose value is a row of another table rather
   * than something typed. A page may add kinds — or replace one — through cfg.kinds.
   */
  var BUILTIN_KINDS = {
    // An author column names a person, so its box lists the accounts that hold the permission
    // behind it — only people who could really have done the edit.
    user: {
      fill: function ($select, field) {
        $select.append($("<option>").val("").text("انتخاب کاربر..."));
        fillOptions(
          $select,
          "UserOptions",
          { field: field.property },
          "کاربری این دسترسی را ندارد",
        );
      },
      label: function (field, value) {
        return optionLabels("UserOptions", { field: field.property }, value);
      },
    },
    // A role is not a column of the account but a row of the assignment table, so the server
    // answers this kind with an EXISTS and the box lists the roles by their title.
    role: {
      fill: function ($select) {
        $select.append($("<option>").val("").text("انتخاب نقش..."));
        fillOptions($select, "RoleOptions", null, "نقشی تعریف نشده است", function (
          role,
        ) {
          return role.title;
        });
      },
      label: function (field, value) {
        return optionLabels("RoleOptions", null, value, function (role) {
          return role.title;
        });
      },
    },
  };

  //#region Client-side generic conditions
  /** Normalizes the Persian/Arabic digits the admin may type into a number. */
  function numberValue(value) {
    var text = latinDigits(String(value === null || value === undefined ? "" : value))
      .replace(/,/g, "")
      .trim();
    if (!text) return null;
    var number = Number(text);
    return isNaN(number) ? null : number;
  }

  /** A calendar key (YYYY-MM-DD) so date comparisons ignore the time of day. */
  function dateKey(value) {
    if (!value) return null;
    var date = new Date(value);
    if (isNaN(date.getTime())) return null;
    return (
      date.getFullYear() +
      "-" +
      String(date.getMonth() + 1).padStart(2, "0") +
      "-" +
      String(date.getDate()).padStart(2, "0")
    );
  }

  function booleanValue(value) {
    return value === true || value === 1 || String(value).toLowerCase() === "true";
  }

  /**
   * Evaluates one condition against a row object. Strings are compared case-insensitively,
   * numbers may arrive with Persian digits, and dates are compared by calendar day so the
   * <code>date</code> input works without a time part.
   */
  function matchesCondition(entity, field, condition) {
    var actual = entity ? entity[condition.property] : null;
    // Server filter metadata often uses PascalCase while the JSON row is camelCase.
    if (entity && actual === undefined) {
      var camel = condition.property.charAt(0).toLowerCase() + condition.property.slice(1);
      actual = entity[camel];
    }
    var kind = field ? field.kind : "string";
    var expected = condition.value === null || condition.value === undefined
      ? ""
      : String(condition.value).trim();
    var operator = condition.operator;

    function test(expectedPart) {
      if (kind === "number") {
        var left = numberValue(actual);
        var right = numberValue(expectedPart);
        if (left === null || right === null) return false;
        if (operator === "Equals") return left === right;
        if (operator === "NotEquals") return left !== right;
        if (operator === "GreaterThan") return left > right;
        if (operator === "GreaterThanOrEqual") return left >= right;
        if (operator === "LessThan") return left < right;
        if (operator === "LessThanOrEqual") return left <= right;
        return false;
      }

      if (kind === "boolean") {
        var leftBool = booleanValue(actual);
        var rightBool = booleanValue(expectedPart);
        return operator === "NotEquals" ? leftBool !== rightBool : leftBool === rightBool;
      }

      if (kind === "date") {
        var leftDate = dateKey(actual);
        var rightDate = dateKey(expectedPart);
        if (!leftDate || !rightDate) return false;
        if (operator === "Equals") return leftDate === rightDate;
        if (operator === "NotEquals") return leftDate !== rightDate;
        if (operator === "GreaterThan") return leftDate > rightDate;
        if (operator === "GreaterThanOrEqual") return leftDate >= rightDate;
        if (operator === "LessThan") return leftDate < rightDate;
        if (operator === "LessThanOrEqual") return leftDate <= rightDate;
        return false;
      }

      var leftText = String(actual === null || actual === undefined ? "" : actual).toLowerCase();
      var rightText = String(expectedPart).toLowerCase();
      if (operator === "Equals") return leftText === rightText;
      if (operator === "NotEquals") return leftText !== rightText;
      if (operator === "Contains") return leftText.indexOf(rightText) >= 0;
      if (operator === "StartsWith") return leftText.indexOf(rightText) === 0;
      if (operator === "EndsWith")
        return leftText.slice(-rightText.length) === rightText;
      return false;
    }

    if (operator === "In") {
      return expected
        .split(",")
        .map(function (part) {
          return part.trim();
        })
        .filter(Boolean)
        .some(test);
    }

    return test(expected);
  }

  /** Whether one row satisfies every committed or currently typed condition. */
  function matchesConditions(entity, conditions, fields) {
    if (!conditions || !conditions.length) return true;

    var byProperty = {};
    (fields || []).forEach(function (field) {
      byProperty[field.property] = field;
    });

    for (var i = 0; i < conditions.length; i++) {
      var condition = conditions[i];
      if (!matchesCondition(entity, byProperty[condition.property], condition))
        return false;
    }

    return true;
  }

  /**
   * Registers a DataTables search filter for a client-side table. The table rows must carry the
   * source object with <code>.data("entity", row)</code>; the conditions and field descriptors are
   * read on every draw, so the filter stays in sync with the picker.
   */
  function clientFilter(tableId, conditions, fields) {
    $.fn.dataTable.ext.search.push(function (settings, data, dataIndex) {
      if (settings.nTable.id !== tableId) return true;

      var active = conditions();
      if (!active || !active.length) return true;

      var row = settings.aoData[dataIndex] && settings.aoData[dataIndex].nTr;
      if (!row) return true;

      return matchesConditions($(row).data("entity"), active, fields());
    });
  }
  //#endregion

  /**
   * Builds the «فیلتر روی» picker from ?handler=Fields: the field list, the operators each field
   * accepts and a value control rebuilt from the field kind. Nothing is field-specific — a new
   * column on the model arrives as a picker entry without a line of markup or JS.
   *
   * @param {object} cfg
   *  fieldEl / operatorEl / valueWrapEl / chipsEl : selectors of the markup in the page.
   *  fieldsUrl : the GET endpoint (default "?handler=Fields").
   *  onChange  : called to re-query the table after any picker change.
   *  kinds     : extra or overriding field kinds, e.g.
   *              { role: { fill($select, field), label(field, value) } }.
   * @returns object with load(), conditions(), add(), clear(), fields().
   */
  function filterPicker(cfg) {
    var $field = $(cfg.fieldEl);
    var $operator = $(cfg.operatorEl);
    var $wrap = $(cfg.valueWrapEl);
    var $chips = $(cfg.chipsEl);

    var fieldList = []; // ?handler=Fields metadata: [{ property, label, kind, operators, options }]
    var committed = []; // chips: [{ property, label, operator, operatorLabel, value, display }]
    var liveTimer = null; // debounce for the value box

    /** The kind entry that renders this field: the page's own kinds win over the built-in ones. */
    function kindFor(field) {
      if (!field) return null;
      var page = cfg.kinds && cfg.kinds[field.kind];
      return page || BUILTIN_KINDS[field.kind];
    }

    /** The field currently chosen in «فیلتر روی», or null before the metadata arrives. */
    function currentField() {
      var name = $field.val();
      for (var i = 0; i < fieldList.length; i++) {
        if (fieldList[i].property === name) return fieldList[i];
      }
      return null;
    }

    /** Refills the operator select with the operators the chosen field accepts. */
    function renderOperators() {
      var field = currentField();
      var $op = $operator.empty();
      if (!field) return;

      field.operators.forEach(function (op) {
        var label = OPERATOR_LABELS[op];
        $op.append(
          $("<option>")
            .val(op)
            .text(label ? label : op),
        );
      });
    }

    /**
     * Rebuilds the value control from the chosen field kind: an option list for a bool or an enum,
     * a kind's own box for user and whatever a page adds, a date/number box for those kinds, plain
     * text otherwise. Every control keeps the same id, so reading the current value never depends
     * on the kind.
     */
    function renderValueControl() {
      var field = currentField();
      $wrap.empty();
      if (!field) return;

      var operator = $operator.val();
      var options = field.options || [];
      var kind = kindFor(field);

      // A kind the module or the page knows how to render (an author column lists accounts).
      if (kind) {
        var $custom = $(
          '<select id="filterValue" class="form-select form-select-solid w-250px">',
        );
        if (operator === "In") $custom.attr("multiple", true);
        $wrap.append($custom);
        kind.fill($custom, field);
        filterSelect2($custom);
        bindValueInput();
        return;
      }

      if (options.length && operator !== "In") {
        var $select = $(
          '<select id="filterValue" class="form-select form-select-solid w-200px">',
        );
        $select.append($("<option>").val("").text("انتخاب..."));
        options.forEach(function (option) {
          $select.append($("<option>").val(option).text(valueLabel(option)));
        });
        $wrap.append($select);
        filterSelect2($select);
      } else {
        var $input = $(
          '<input type="text" id="filterValue" class="form-control form-control-solid w-200px">',
        );
        if (field.kind === "date") $input.attr("type", "date").attr("dir", "ltr");
        else if (field.kind === "number")
          // A text box with a numeric keyboard, not <input type="number">: the latter throws away
          // anything typed in Persian digits, and every numeric input of this admin accepts ۰-۹.
          $input.attr("inputmode", "numeric").attr("dir", "rtl");
        else if (field.kind === "guid") $input.attr("dir", "ltr");

        // A field that declares a range says so on the box; inRange() is what enforces it.
        var hint = rangeHint(field);
        if (hint) {
          if (field.min !== null && field.min !== undefined)
            $input.attr("min", field.min);
          if (field.max !== null && field.max !== undefined)
            $input.attr("max", field.max);
          $input.attr("title", hint);
        }

        $input.attr(
          "placeholder",
          operator === "In"
            ? "چند مقدار با ویرگول"
            : field.kind === "string"
              ? "جستجو..."
              : hint || "مقدار",
        );
        $wrap.append($input);
      }

      bindValueInput();
    }

    /** Live search: the value box filters while it is typed, debounced. */
    function bindValueInput() {
      $("#filterValue")
        .off("input.change.table")
        .on("input.change.table", function () {
          // The box goes red while the value is outside the field's range, before the request runs.
          markInvalid(!inRange(currentField(), readValue()));
          clearTimeout(liveTimer);
          liveTimer = setTimeout(cfg.onChange, 350);
        });
    }

    /**
     * Reads the value control, joining a multi-select into the comma form the In operator wants.
     * A typed number or date is written in Persian digits by an admin who means 2, not by anyone
     * searching a name that happens to contain ۲ — so only the non-text kinds convert.
     */
    function readValue() {
      var raw = $("#filterValue").val();
      if (raw === null || raw === undefined) return "";

      var field = currentField();
      return (Array.isArray(raw) ? raw : [raw])
        .map(function (x) {
          var text = String(x).trim();
          return field && field.kind !== "string" ? latinDigits(text) : text;
        })
        .filter(function (x) {
          return x !== "";
        })
        .join(",");
    }

    /**
     * The conditions sent with the next request: the committed chips plus the value currently typed
     * in the picker. The typed value is not committed, so it never duplicates a chip and disappears
     * on reload instead of lingering as an invisible filter. An out-of-range value is not sent at
     * all: the box says why, the query stays untouched.
     *
     * `value` is always what the server must filter on, never what the chip shows — a chip for an
     * author column reads «محمد» while the condition stays the account id.
     */
    function conditions() {
      var all = committed.map(function (condition) {
        return {
          property: condition.property,
          operator: condition.operator,
          value: condition.value,
        };
      });
      var field = currentField();
      var value = readValue();

      if (field && value && inRange(field, value)) {
        all.push({
          property: field.property,
          operator: $operator.val(),
          value: value,
        });
      }
      return all;
    }

    /** Renders the active chips, each with its own remove handler. */
    function renderChips() {
      $chips.empty();

      committed.forEach(function (condition, index) {
        var $chip = $(
          '<span class="badge badge-light-primary d-inline-flex align-items-center fs-7 py-2 px-3">',
        );
        $chip.append(
          $("<span>")
            .text(
              condition.label +
                " " +
                condition.operatorLabel +
                " " +
                condition.display,
            )
            .attr("dir", "auto"),
        );

        var $remove = $(
          '<button type="button" class="btn btn-sm btn-icon btn-transparent-dark ms-1" title="حذف این فیلتر">',
        ).append(
          '<i class="ki-duotone ki-cross fs-4"><span class="path1"></span><span class="path2"></span></i>',
        );
        $remove.on("click", function () {
          committed.splice(index, 1);
          renderChips();
          cfg.onChange();
        });

        $chip.append($remove);
        $chips.append($chip);
      });
    }

    /** Commits the picker state as a chip and re-queries the table. */
    function add() {
      var field = currentField();
      var operator = $operator.val();
      var value = readValue();

      if (!field || !operator) return;
      if (!value) {
        toastr.error("مقدار فیلتر را وارد کنید.", "خطا");
        return;
      }
      if (!inRange(field, value)) {
        toastr.error(
          "مقدار «" + field.label + "» باید " + rangeHint(field) + " باشد.",
          "خطا",
        );
        markInvalid(true);
        return;
      }

      // A chip shows a human label (an account name, «فعال») but the server must keep filtering on
      // what was really chosen, so both travel with the condition.
      var kind = kindFor(field);
      committed.push({
        property: field.property,
        label: field.label,
        operator: operator,
        operatorLabel: OPERATOR_LABELS[operator] || operator,
        value: value,
        display: kind && kind.label ? kind.label(field, value) : valueLabel(value) || value,
      });

      $("#filterValue").val("");
      renderChips();
      cfg.onChange();
    }

    /** Drops every chip and the typed value, then re-queries. */
    function clear() {
      committed = [];
      $("#filterValue").val("");
      renderChips();
      cfg.onChange();
    }

    /** Fetches the field metadata and opens the picker on the first text field. */
    function load() {
      return $.getJSON(cfg.fieldsUrl || handlerUrl("Fields")).then(function (res) {
        fieldList = (res && res.data) || [];

        $field.empty();
        fieldList.forEach(function (f) {
          $field.append($("<option>").val(f.property).text(f.label));
        });

        // Start on the first text field, so the value box is a working search box right away.
        var firstText = fieldList.filter(function (f) {
          return f.kind === "string";
        })[0];
        if (firstText) $field.val(firstText.property);

        // The two selects of the picker exist in the markup once and only their options change, so
        // they are dressed here and never re-initialised: Select2 reads the live option list.
        filterSelect2($field);
        filterSelect2($operator);

        renderOperators();
        renderValueControl();
      });
    }

    $field.on("change", function () {
      renderOperators();
      renderValueControl();
    });
    $operator.on("change", renderValueControl);

    return {
      load: load,
      add: add,
      clear: clear,
      conditions: conditions,
      fields: function () {
        return fieldList;
      },
    };
  }
  //#endregion

  // Public API of the module.
  window.AdminTable = {
    handlerUrl: handlerUrl,
    csrfToken: csrfToken,
    serverMessage: serverMessage,
    postHandler: postHandler,
    filterPicker: filterPicker,
    filterSelect2: filterSelect2,
    clientFilter: clientFilter,
    matchesConditions: matchesConditions,
    resetOptionDirectories: resetOptionDirectories,
    serverSideAjax: serverSideAjax,
    badge: badge,
    valueLabel: valueLabel,
    faDigits: faDigits,
    latinDigits: latinDigits,
    faDate: faDate,
    faDateTime: faDateTime,
    OPERATOR_LABELS: OPERATOR_LABELS,
    VALUE_LABELS: VALUE_LABELS,
  };
})(window, jQuery);
