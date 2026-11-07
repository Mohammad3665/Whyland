//#region Instructors Page Scripts
/**
 * Admin instructors catalog.
 * - Rows come from ?handler=Rows through the shared server-side table.
 * - ?handler=Fields builds the generic filter picker.
 * - ?handler=FormOptions lists active accounts without a profile.
 * - ?handler=Details fills the edit form.
 * - POST ?handler=Create / Update persists the profile and its optional image.
 */

var instructorPage = null; // { table }
var instructorPicker = null;
var instructorFormOptions = null;
var instructorEditId = null;

//#region Helpers
function instructorOnlyDigits(value) {
  return (value || "").replace(/\D/g, "");
}

function instructorInitials(profile) {
  return (
    ((profile.firstName || "").charAt(0) || "") +
    "‌" +
    ((profile.lastName || "").charAt(0) || "")
  );
}

function instructorAvatar(profile) {
  if (profile.profileImageUrl) {
    return $("<img>")
      .attr("src", profile.profileImageUrl)
      .attr("alt", profile.firstName + " " + profile.lastName);
  }

  return $('<span class="symbol-label fw-bold bg-light-primary text-primary">').text(
    instructorInitials(profile),
  );
}

function instructorUrlValid(value) {
  if (!value) return true;
  try {
    var url = new URL(
      /^https?:\/\//i.test(value) ? value : "https://" + value,
    );
    return url.protocol === "http:" || url.protocol === "https:";
  } catch (e) {
    return false;
  }
}

function formatSheba(input) {
  var digits = instructorOnlyDigits(input.value).slice(0, 24);
  input.value = digits.replace(/(.{4})/g, "$1 ").trim();
}

function formatInstructorAccount(input) {
  input.value = instructorOnlyDigits(input.value).slice(0, 20);
}
//#endregion

//#region Filter picker
function loadInstructorFields() {
  instructorPicker = AdminTable.filterPicker({
    fieldEl: "#filterField",
    operatorEl: "#filterOperator",
    valueWrapEl: "#filterValueWrap",
    chipsEl: "#filterChips",
    onChange: function () {
      if (instructorPage) instructorPage.table.draw();
    },
  });

  return instructorPicker.load();
}
//#endregion

//#region Server Connectivity
function instructorConditions() {
  return instructorPicker ? instructorPicker.conditions() : [];
}

function reloadInstructorTable() {
  if (instructorPage) instructorPage.table.ajax.reload(null, false);
}

/** Loads the eligible accounts once per create session. */
function loadInstructorFormOptions(force) {
  if (instructorFormOptions && !force)
    return $.Deferred().resolve(instructorFormOptions).promise();

  return $.getJSON(AdminTable.handlerUrl("FormOptions")).then(function (res) {
    if (!res || !res.isSuccess)
      return $.Deferred().reject({ responseJSON: res }).promise();

    instructorFormOptions = (res.data && res.data.users) || [];
    fillInstructorUserOptions(instructorFormOptions);
    return instructorFormOptions;
  });
}

function fillInstructorUserOptions(users) {
  var $select = $("#Form_UserId").empty();
  $select.append($("<option>").val("").text("انتخاب کاربر..."));

  users.forEach(function (user) {
    $select.append(
      $("<option>")
        .val(user.id)
        .text(
          user.name +
            (user.phoneNumber ? " — " + user.phoneNumber : ""),
        ),
    );
  });

  $select
    .val("")
    .trigger("change");
}

function fetchInstructorDetails(id) {
  return $.getJSON(AdminTable.handlerUrl("Details"), { id: id }).then(function (res) {
    if (res && res.isSuccess && res.data)
      return res.data;

    return $.Deferred().reject({ responseJSON: res }).promise();
  });
}

function saveInstructorProfile(data, mode) {
  var form = new FormData();
  form.append("Id", mode === "edit" ? instructorEditId : "");
  form.append("UserId", data.userId || "");
  form.append("Bio", data.bio || "");
  form.append("Biography", data.biography || "");
  form.append("Specialization", data.specialization || "");
  form.append("Website", data.website || "");
  form.append("LinkedIn", data.linkedIn || "");
  form.append("Instagram", data.instagram || "");
  form.append("ShebaNumber", data.shebaNumber || "");
  form.append("BankAccountNumber", data.bankAccountNumber || "");
  form.append("RemoveImage", data.removeImage ? "true" : "false");

  var file = $("#Form_ProfileImageFile")[0].files[0];
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

  return dfd.promise();
}
//#endregion

//#region Table
function fillInstructorRow(row, profile) {
  var $cells = $("td", row);

  $cells
    .eq(1)
    .html(
      "<div class='d-flex align-items-center'>" +
        "<div class='symbol symbol-circle symbol-45px me-3 instructor-avatar'></div>" +
        "<div class='d-flex flex-column'>" +
        "<a class='text-gray-800 fw-bold text-hover-primary instructor-name mb-1' href='#'></a>" +
        "<span class='text-muted fs-7 instructor-email'></span>" +
        "</div></div>",
    );
  $cells.eq(1).find(".instructor-avatar").append(instructorAvatar(profile));
  $cells
    .eq(1)
    .find(".instructor-name")
    .text(profile.firstName + " " + profile.lastName)
    .on("click", function (e) {
      e.preventDefault();
      window.location.href = instructorDetailsUrl(profile.id);
    });
  $cells
    .eq(1)
    .find(".instructor-email")
    .text(profile.email || "—");

  $cells
    .eq(2)
    .append(
      $('<span class="text-gray-700">').text(profile.specialization || "—"),
    );

  $cells
    .eq(3)
    .append(
      $('<div class="text-gray-700 fs-7" dir="ltr">').text(
        profile.phoneNumber || "—",
      ),
    );

  $cells
    .eq(4)
    .append(
      $('<span class="badge badge-light-info">').text(
        AdminTable.faDigits(profile.courseCount || 0) + " دوره",
      ),
    );

  $cells
    .eq(5)
    .append(
      $('<span class="text-gray-600 fs-7">').text(
        AdminTable.faDate(profile.createdAt),
      ),
    );

  var $actions = $cells.eq(6).addClass("text-end nowrap-cell");

  var $view = $(
    '<a class="btn btn-sm btn-icon btn-bg-light btn-active-color-primary me-1" title="مشاهده پروفایل">',
  )
    .attr("href", instructorDetailsUrl(profile.id))
    .append(
      '<i class="ki-duotone ki-eye fs-2"><span class="path1"></span><span class="path2"></span><span class="path3"></span></i>',
    );

  var $edit = $(
    '<button type="button" class="btn btn-sm btn-icon btn-bg-light btn-active-color-primary me-1" title="ویرایش">',
  ).append(
    '<i class="ki-duotone ki-pencil fs-2"><span class="path1"></span><span class="path2"></span></i>',
  );
  $edit.on("click", function () {
    openInstructorEditModal(profile.id);
  });

  $actions.append($view, $edit);
}

function instructorDetailsUrl(id) {
  return "/Admin/InstructorProfile/Details/" + id;
}

function initInstructorTable() {
  instructorPage = {
    table: $("#kt_instructors_table").DataTable({
      serverSide: true,
      processing: true,
      searching: false,
      order: [],
      pageLength: 10,
      lengthMenu: [10, 25, 50, 100],
      // Cell contents are written by createdRow, not bound as column data, so the child row has
      // to be built from the real nodes instead of the (empty) internal row data.
      responsive: {
        details: { renderer: "listHiddenNodes" },
      },
      ajax: AdminTable.serverSideAjax({
        conditions: instructorConditions,
        errorMessage: "خطا در دریافت لیست مدرسان.",
      }),
      columns: [
        {
          data: null,
          defaultContent: "",
          name: "",
          orderable: false,
          className: "dtr-control",
        },
        { data: null, defaultContent: "", name: "firstName" },
        { data: null, defaultContent: "", name: "specialization" },
        { data: null, defaultContent: "", name: "phoneNumber" },
        { data: null, defaultContent: "", name: "courseCount", className: "min-tablet" },
        { data: null, defaultContent: "", name: "createdAt", className: "min-tablet" },
        {
          data: null,
          defaultContent: "",
          name: "",
          orderable: false,
          className: "min-tablet",
        },
      ],
      columnDefs: [
        { responsivePriority: 1, targets: 1 },
        { responsivePriority: 2, targets: 0 },
        { responsivePriority: 3, targets: 2 },
        { responsivePriority: 4, targets: 3 },
        { responsivePriority: 5, targets: 4 },
        { responsivePriority: 6, targets: 5 },
        { responsivePriority: 7, targets: 6 },
      ],
      createdRow: fillInstructorRow,
    }),
  };
}
//#endregion

//#region Modal
function resetInstructorForm() {
  instructorEditId = null;

  $("#Form_Id").val("");
  $("#Form_UserId").val("").trigger("change").prop("disabled", false);
  $("#instructor-user-row").show();

  $("#Form_Bio").val("");
  $("#Form_Biography").val("");
  $("#Form_Specialization").val("");
  $("#Form_Website").val("");
  $("#Form_LinkedIn").val("");
  $("#Form_Instagram").val("");
  $("#Form_ShebaNumber").val("");
  $("#Form_BankAccountNumber").val("");
  $("#bio-char-count").text("0 / 1000");

  $("#Form_ProfileImageFile").val("");
  $("#Form_ProfileImage_remove").val("");
  setInstructorImage("/assets/media/svg/files/blank-image.svg", true);
  $("#instructor-image-remove").hide();
}

function setInstructorImage(url, blank) {
  var $img = $("#Form_ProfileImage_preview");
  $img
    .attr("src", url || "/assets/media/svg/files/blank-image.svg")
    .css("visibility", "")
    .toggleClass("opacity-25", !!blank);
}

function openInstructorCreateModal() {
  resetInstructorForm();
  $("#modal-instructor-profile-title").text("افزودن مدرس");

  loadInstructorFormOptions(true)
    .done(function (users) {
      if (!users.length) {
        toastr.warning(
          "کاربر فعالی بدون پروفایل مدرس باقی نمانده است. ابتدا یک کاربر بسازید.",
          "توجه",
        );
      }
    })
    .fail(function (xhr) {
      toastr.error(
        AdminTable.serverMessage(xhr, "خطا در دریافت لیست کاربران."),
        "خطا",
      );
    });

  bootstrap.Modal.getOrCreateInstance(
    document.getElementById("kt_modal_instructor_profile"),
  ).show();
}

function openInstructorEditModal(id) {
  fetchInstructorDetails(id)
    .done(function (profile) {
      instructorEditId = profile.id;
      $("#modal-instructor-profile-title").text("ویرایش پروفایل مدرس");

      $("#Form_Id").val(profile.id);
      $("#instructor-user-row").hide();
      $("#Form_UserId")
        .append(
          $("<option>")
            .val(profile.userId)
            .text(profile.firstName + " " + profile.lastName),
        )
        .val(profile.userId)
        .trigger("change")
        .prop("disabled", true);

      $("#Form_Bio").val(profile.bio || "");
      $("#Form_Biography").val(profile.biography || "");
      $("#Form_Specialization").val(profile.specialization || "");
      $("#Form_Website").val(profile.website || "");
      $("#Form_LinkedIn").val(profile.linkedIn || "");
      $("#Form_Instagram").val(profile.instagram || "");
      $("#Form_ShebaNumber").val(profile.shebaNumber || "");
      formatSheba(document.getElementById("Form_ShebaNumber"));
      $("#Form_BankAccountNumber").val(profile.bankAccountNumber || "");
      $("#bio-char-count").text(($("#Form_Bio").val() || "").length + " / 1000");

      $("#Form_ProfileImageFile").val("");
      $("#Form_ProfileImage_remove").val("");
      setInstructorImage(
        profile.profileImageUrl || "/assets/media/svg/files/blank-image.svg",
        !profile.profileImageUrl,
      );
      $("#instructor-image-remove").toggle(!!profile.profileImageUrl);

      bootstrap.Modal.getOrCreateInstance(
        document.getElementById("kt_modal_instructor_profile"),
      ).show();
    })
    .fail(function (xhr) {
      toastr.error(
        AdminTable.serverMessage(xhr, "خطا در دریافت اطلاعات مدرس."),
        "خطا",
      );
    });
}

function previewInstructorImage(input) {
  if (!input.files || !input.files[0]) return;
  var file = input.files[0];

  if (file.size > 2 * 1024 * 1024) {
    toastr.error("حجم تصویر باید حداکثر ۲ مگابایت باشد.", "خطا");
    input.value = "";
    return;
  }

  if (!/^image\/(png|jpe?g|webp)$/.test(file.type)) {
    toastr.error("فرمت تصویر باید JPG، PNG یا WebP باشد.", "خطا");
    input.value = "";
    return;
  }

  var url = URL.createObjectURL(file);
  setInstructorImage(url, false);
  $("#Form_ProfileImage_remove").val("");
  $("#instructor-image-remove").show();
}

function markInstructorImageRemoved() {
  $("#Form_ProfileImageFile").val("");
  $("#Form_ProfileImage_remove").val("1");
  setInstructorImage("/assets/media/svg/files/blank-image.svg", true);
  $("#instructor-image-remove").hide();
}

function collectInstructorForm() {
  return {
    userId: $("#Form_UserId").val(),
    bio: $("#Form_Bio").val().trim(),
    biography: $("#Form_Biography").val().trim(),
    specialization: $("#Form_Specialization").val().trim(),
    website: $("#Form_Website").val().trim(),
    linkedIn: $("#Form_LinkedIn").val().trim(),
    instagram: $("#Form_Instagram").val().trim(),
    shebaNumber: instructorOnlyDigits($("#Form_ShebaNumber").val()),
    bankAccountNumber: instructorOnlyDigits($("#Form_BankAccountNumber").val()),
    removeImage: $("#Form_ProfileImage_remove").val() === "1",
  };
}

function validateInstructorForm(data, mode) {
  var errors = [];

  if (mode === "create" && !data.userId) {
    errors.push("انتخاب کاربر الزامی است.");
    $("#Form_UserId").addClass("is-invalid");
  } else {
    $("#Form_UserId").removeClass("is-invalid");
  }

  ["#Form_Website", "#Form_LinkedIn", "#Form_Instagram"].forEach(function (selector) {
    var value = $(selector).val().trim();
    if (!instructorUrlValid(value)) {
      errors.push("آدرس «" + $(selector).closest(".fv-row").find("label").text() + "» معتبر نیست.");
      $(selector).addClass("is-invalid");
    } else {
      $(selector).removeClass("is-invalid");
    }
  });

  if (data.shebaNumber && data.shebaNumber.length !== 24) {
    errors.push("شماره شبا باید دقیقاً ۲۴ رقم باشد.");
    $("#Form_ShebaNumber").addClass("is-invalid");
  } else {
    $("#Form_ShebaNumber").removeClass("is-invalid");
  }

  if (
    data.bankAccountNumber &&
    (data.bankAccountNumber.length < 6 || data.bankAccountNumber.length > 20)
  ) {
    errors.push("شماره حساب باید بین ۶ تا ۲۰ رقم باشد.");
    $("#Form_BankAccountNumber").addClass("is-invalid");
  } else {
    $("#Form_BankAccountNumber").removeClass("is-invalid");
  }

  return errors;
}

function submitInstructorProfile() {
  var mode = instructorEditId ? "edit" : "create";
  var data = collectInstructorForm();
  var errors = validateInstructorForm(data, mode);

  if (errors.length) {
    toastr.error(errors.join("<br/>"), "خطا", { timeOut: 5000 });
    return;
  }

  var $btn = $("#kt_instructor_profile_submit");
  var $label = $btn.find(".indicator-label");
  var $progress = $btn.find(".indicator-progress");
  $label.hide();
  $progress.show();
  $btn.prop("disabled", true);

  saveInstructorProfile(data, mode)
    .done(function () {
      bootstrap.Modal.getOrCreateInstance(
        document.getElementById("kt_modal_instructor_profile"),
      ).hide();

      toastr.success(
        mode === "create"
          ? "پروفایل مدرس با موفقیت ایجاد شد."
          : "تغییرات پروفایل مدرس ذخیره شد.",
        "موفق",
      );

      instructorFormOptions = null;
      reloadInstructorTable();
    })
    .fail(function (xhr) {
      toastr.error(
        AdminTable.serverMessage(xhr, "خطا در ذخیره پروفایل مدرس."),
        "خطا",
        { timeOut: 5000 },
      );
    })
    .always(function () {
      $btn.prop("disabled", false);
      $progress.hide();
      $label.show();
    });
}
//#endregion

//#region Inline handlers
function addFilterCondition() {
  if (instructorPicker) instructorPicker.add();
}

function clearFilterConditions() {
  if (instructorPicker) instructorPicker.clear();
}
//#endregion

//#region Boot
$(function () {
  loadInstructorFields()
    .then(initInstructorTable)
    .fail(function () {
      initInstructorTable();
      toastr.error("خطا در دریافت فیلدهای قابل فیلتر.", "خطا");
    });

  $("#kt_modal_instructor_profile").on("shown.bs.modal", function () {
    if (!$("#Form_UserId").data("select2")) {
      $("#Form_UserId").select2({
        dropdownParent: $("#kt_modal_instructor_profile .modal-content"),
        placeholder: "انتخاب کاربر",
        allowClear: true,
        width: "100%",
        dropdownPosition: "below",
        dir: "rtl",
      });
    }
  });

  // A profile page links here with ?edit=<id>; open that profile as soon as the table is ready.
  var editId = new URLSearchParams(window.location.search).get("edit");
  if (editId) openInstructorEditModal(editId);
});
//#endregion
//#endregion
