// ===== DataTable =====
var table = $("#kt_categories_table").DataTable({
  responsive: true,
  order: [],
  pageLength: 25,
  columnDefs: [{ orderable: false, targets: [-1] }],
  language: {
    search: "جستجو:",
    lengthMenu: "_MENU_ مورد",
    info: "نمایش _START_ تا _END_ از _TOTAL_ مورد",
    infoEmpty: "هیچ موردی یافت نشد",
    infoFiltered: "(فیلتر شده از _MAX_ مورد)",
    zeroRecords: "موردی یافت نشد",
    emptyTable: "داده‌ای موجود نیست",
    loadingRecords: "در حال بارگذاری...",
    processing: "در حال پردازش...",
    paginate: {
      first: "اول",
      last: "آخر",
      next: "بعدی",
      previous: "قبلی",
    },
  },
});

var currentMode = "create"; // "create" | "edit"

$("#categorySearch").on("keyup", function () {
  table.search(this.value).draw();
});

$("#statusFilter").on("change", function () {
  var val = this.value;

  $.fn.dataTable.ext.search.pop();

  if (val !== "") {
    $.fn.dataTable.ext.search.push(function (settings, data, dataIndex) {
      var row = table.row(dataIndex).node();
      return $(row).data("active").toString() === val;
    });
  }
  table.draw();
});

function openCreateModal() {
  currentMode = "create"; // ✅

  $("#modal-category-title").text("افزودن دسته‌بندی");
  $("#Form_Name").val("");
  $("#Form_LatinName").val("");
  $("#Form_DisplayOrder").val(0);
  $("#Form_ParentId").val("").trigger("change");
  $("#Form_IsActive").prop("checked", true);
}

function openEditModal(id, name, latinName, displayOrder, parentId, isActive) {
  currentMode = "edit"; // ✅

  $("#modal-category-title").text("ویرایش دسته‌بندی");
  $("#Form_Name").val(name);
  $("#Form_LatinName").val(latinName);
  $("#Form_DisplayOrder").val(displayOrder);
  $("#Form_ParentId")
    .val(parentId ?? "")
    .trigger("change");
  $("#Form_IsActive").prop("checked", isActive);

  var modal = new bootstrap.Modal(document.getElementById("kt_modal_category"));
  modal.show();
}

function openDeleteModal(id, name) {
  $("#delete-category-name").text(name);
  var modal = new bootstrap.Modal(
    document.getElementById("kt_modal_delete_category"),
  );
  modal.show();
}

function submitCategoryForm() {
  var btn = document.getElementById("kt_modal_category_submit");
  btn.setAttribute("data-kt-indicator", "on");
  btn.disabled = true;

  setTimeout(function () {
    btn.removeAttribute("data-kt-indicator");
    btn.disabled = false;

    var modal = bootstrap.Modal.getInstance(
      document.getElementById("kt_modal_category"),
    );
    modal.hide();

    if (currentMode === "create") {
      toastr.success("دسته‌بندی با موفقیت ایجاد شد.", "موفق");
    } else {
      toastr.success("تغییرات با موفقیت ذخیره شد.", "موفق");
    }
  }, 800);
}

function confirmDelete() {
  var modal = bootstrap.Modal.getInstance(
    document.getElementById("kt_modal_delete_category"),
  );
  modal.hide();

  toastr.success("دسته‌بندی با موفقیت حذف شد.", "موفق");
}

if (typeof KTSelect2 !== "undefined") {
  KTSelect2.init();
}
