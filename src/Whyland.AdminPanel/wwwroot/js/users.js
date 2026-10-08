//#region Users Page Scripts
/**
 * Admin "Users" list page (sections 4.1 + 4.2 of the UI/UX doc):
 * authentication users with a status filter, search, pagination
 * (DataTables built-in) and row actions (edit / view profile / toggle
 * active status). The create/edit modal covers base info, the user
 * profile and multi-select roles; the password field is shown only
 * in create mode.
 * Built on top of the shared AdminCrud module (admin-crud.js).
 */

//#region CRUD Page Configuration
var page = AdminCrud.createPage({
  table: {
    selector: "#kt_users_table",
    options: {
      pageLength: 10, // pagination required by section 4.1
    },
  },
  search: "#userSearch",
  filters: [
    {
      el: "#userStatusFilter",
      test: AdminCrud.Filters.dataEquals("status"),
    },
  ],
  modal: {
    id: "kt_modal_user",
    titleEl: "#modal-user-title",
    submitBtn: "kt_modal_user_submit",
  },
  texts: {
    createTitle: "افزودن کاربر",
    editTitle: "ویرایش کاربر",
    created: "کاربر با موفقیت ایجاد شد.",
    updated: "تغییرات با موفقیت ذخیره شد.",
  },

  // Populate the form fields from a user object (null = create mode).
  fillForm: function (user) {
    user = user || {
      firstName: "",
      lastName: "",
      email: "",
      phoneNumber: "",
      isActive: true,
      roles: [],
    };
    $("#Form_FirstName").val(user.firstName);
    $("#Form_LastName").val(user.lastName);
    $("#Form_Email").val(user.email);
    $("#Form_PhoneNumber").val(user.phoneNumber);
    $("#Form_IsActive").prop("checked", user.isActive);
    $("#Form_Roles")
      .val(user.roles.length ? user.roles : null)
      .trigger("change");

    // Password is create-only: hidden while editing an existing user.
    setPasswordVisibility(state.mode === "create");
  },

  // Collect the form into the user data object sent to the server.
  getData: function () {
    var roleValues = $("#Form_Roles").val();
    return {
      firstName: $("#Form_FirstName").val().trim(),
      lastName: $("#Form_LastName").val().trim(),
      email: $("#Form_Email").val().trim(),
      phoneNumber: $("#Form_PhoneNumber").val().trim(),
      password: $("#Form_Password").val(),
      isActive: $("#Form_IsActive").is(":checked"),
      roles: roleValues ? roleValues : [],
      profile: {
        birthDate: $("#Form_BirthDate").val().trim(),
        gender: $("#Form_Gender").val() || null,
      },
    };
  },

  validate: function () {
    var errors = [];
    var requiredFields = [
      { el: "#Form_FirstName", msg: "نام الزامی است." },
      { el: "#Form_LastName", msg: "نام خانوادگی الزامی است." },
      { el: "#Form_PhoneNumber", msg: "شماره موبایل الزامی است." },
    ];

    // Password is required only when creating a new user.
    if (state.mode === "create") {
      requiredFields.push({
        el: "#Form_Password",
        msg: "رمز عبور الزامی است.",
      });
    }

    requiredFields.forEach(function (f) {
      if (!$(f.el).val().trim()) {
        errors.push(f.msg);
        $(f.el).addClass("is-invalid");
      } else {
        $(f.el).removeClass("is-invalid");
      }
    });

    // Basic email format check when provided (unique check is server-side).
    var email = $("#Form_Email").val().trim();
    if (email && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
      errors.push("فرمت ایمیل صحیح نیست.");
      $("#Form_Email").addClass("is-invalid");
    } else {
      $("#Form_Email").removeClass("is-invalid");
    }
    return errors;
  },
});

// The PageModel keeps state private to createPage; capture it for helpers.
var state = page.state;
//#endregion

//#region Password Visibility (create-only field)
/** Shows or hides the password field depending on create vs edit mode. */
function setPasswordVisibility(visible) {
  $("#user-password-row").toggle(visible);
  $("#Form_Password").prop("required", visible);
}
//#endregion

//#region Toggle Active Status
var toggleTarget = null; // { id, name, makeActive }

/**
 * Opens the toggle-status confirmation.
 * makeActive is the NEW state the row will get after confirmation.
 */
function toggleUserStatus(id, name, makeActive) {
  toggleTarget = { id: id, name: name, makeActive: makeActive };
  $("#toggle-status-user-name").text(name);
  $("#toggle-status-user-text").text(makeActive ? "فعال شود؟" : "غیرفعال شود؟");
  AdminCrud.Utils.showModal("kt_modal_toggle_user_status");
}

function confirmToggleStatus() {
  if (!toggleTarget) return;
  var row = page.table
    .rows()
    .nodes()
    .to$()
    .filter('[data-id="' + toggleTarget.id + '"]');

  // Demo mode: swap the badge classes/text in place.
  // TODO: POST to ?handler=ToggleStatus when the data layer is connected.
  // Both attr and data are updated: the shared status filter predicate
  // reads $row.data("status"), which caches on first access — updating
  // only the attribute would leave the filter with a stale value.
  var newStatus = toggleTarget.makeActive ? "active" : "inactive";
  row.attr("data-status", newStatus);
  row.data("status", newStatus);
  row
    .find("td")
    .eq(5)
    .html(
      toggleTarget.makeActive
        ? '<span class="badge badge-light-success">فعال</span>'
        : '<span class="badge badge-light-danger">غیرفعال</span>',
    );
  row
    .find('button[title="تغییر وضعیت"]')
    .attr(
      "onclick",
      "toggleUserStatus(" +
        toggleTarget.id +
        ", '" +
        toggleTarget.name +
        "', " +
        !toggleTarget.makeActive +
        ")",
    );

  AdminCrud.Utils.hideModal("kt_modal_toggle_user_status");
  toastr.success(
    "وضعیت کاربر «" + toggleTarget.name + "» با موفقیت تغییر کرد.",
    "موفق",
  );
  toggleTarget = null;
}
//#endregion

//#region Inline onclick Handlers (HTML compatibility)
function openCreateModal() {
  page.openCreate();
}
function submitUserForm() {
  page.submit();
}
function openEditModal(id) {
  // Edit uses the inline data already present in the table row.
  var row = page.table
    .rows()
    .nodes()
    .to$()
    .filter('[data-id="' + id + '"]');

  var nameParts = row.find("td").eq(1).find(".fw-bold").text().trim();
  var isActive = row.attr("data-status") === "active";

  page.state.mode = "edit";
  page.state.editId = id;
  $("#modal-user-title").text("ویرایش کاربر");
  $("#Form_FirstName").val(nameParts.split(" ")[0] || "");
  $("#Form_LastName").val(nameParts.split(" ").slice(1).join(" ") || "");
  $("#Form_Email").val(row.find("td").eq(2).text().trim());
  $("#Form_PhoneNumber").val(row.find("td").eq(3).text().trim());
  $("#Form_IsActive").prop("checked", isActive);
  $("#Form_Roles").val(null).trigger("change"); // TODO: load real roles
  setPasswordVisibility(false);
  AdminCrud.Utils.showModal("kt_modal_user");
}
//#endregion
//#endregion
