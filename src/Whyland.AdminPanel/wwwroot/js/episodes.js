//#region Episodes Page Scripts
/**
 * Admin "Episodes" list page (section 3.4 of the UI/UX doc):
 * episodes of a course section with drag & drop reordering
 * (DataTables RowReorder), create/edit modal and delete confirmation.
 * The modal supports a video file upload OR a video URL (mutually
 * exclusive — selecting one locks the other) and auto-detects the
 * video duration, locking the duration field once detected.
 * Built on top of the shared AdminCrud module (admin-crud.js).
 */

var reorder; // shared drag & drop ordering controller (set below)

//#region CRUD Page Configuration
var page = AdminCrud.createPage({
  table: {
    selector: "#kt_episodes_table",
    options: $.extend(true, AdminCrud.rowReorderOptions(), {
      columnDefs: [
        { orderable: false, targets: [0, 1, 5] },
        { className: "all", targets: [1, 2] },
        { className: "min-tablet", targets: [3, 4, 5] },
        { className: "dtr-control", targets: 0 },
      ],
    }),
  },
  search: "#episodeSearch",
  modal: {
    id: "kt_modal_episode",
    titleEl: "#modal-episode-title",
    submitBtn: "kt_modal_episode_submit",
  },
  deleteModal: {
    id: "kt_modal_delete_episode",
    nameEl: "#delete-episode-name",
  },
  texts: {
    createTitle: "افزودن اپیزود",
    editTitle: "ویرایش اپیزود",
    created: "اپیزود با موفقیت ایجاد شد.",
    updated: "تغییرات با موفقیت ذخیره شد.",
    deleted: "اپیزود با موفقیت حذف شد.",
  },

  // Populate the form fields from an episode object (null = create mode).
  fillForm: function (ep) {
    ep = ep || {
      title: "",
      description: "",
      video: null,
      videoUrl: "",
      duration: "",
      isFree: false,
      displayOrder: 0,
    };
    $("#Form_Title").val(ep.title);
    $("#Form_Description").val(ep.description);
    resetVideoInputs();
    $("#Form_Duration").val(ep.duration || "");
    unlockDuration();
    $("#Form_IsFree").prop("checked", ep.isFree);
    $("#Form_DisplayOrder").val(
      ep.displayOrder > 0 ? ep.displayOrder : reorder.getNextOrder(),
    );
  },

  // Collect the form into the episode data object sent to the server.
  getData: function () {
    var videoInput = document.getElementById("Form_Video");
    return {
      title: $("#Form_Title").val().trim(),
      description: $("#Form_Description").val().trim(),
      videoFile:
        videoInput && videoInput.files.length ? videoInput.files[0].name : null,
      videoUrl: $("#Form_VideoUrl").val().trim() || null,
      duration: $("#Form_Duration").val().trim(),
      isFree: $("#Form_IsFree").is(":checked"),
      displayOrder: parseInt($("#Form_DisplayOrder").val(), 10) || 0,
    };
  },

  validate: function () {
    var errors = [];
    if (!$("#Form_Title").val().trim()) {
      errors.push("عنوان اپیزود الزامی است.");
      $("#Form_Title").addClass("is-invalid");
    } else {
      $("#Form_Title").removeClass("is-invalid");
    }
    return errors;
  },
});
//#endregion

//#region Video Sources: File vs URL (Mutual Lock) + Auto Duration
var lastProbedUrl = "";

/** Clears both video inputs and re-enables them. */
function resetVideoInputs() {
  var videoInput = document.getElementById("Form_Video");
  if (videoInput) videoInput.value = "";
  $("#Form_VideoUrl").val("");
  lastProbedUrl = "";
  syncVideoLocks();
  unlockDuration();
}

/**
 * Mutual exclusion: a URL locks the file input (and vice versa).
 * The inactive field is cleared so exactly one source stays active.
 */
function syncVideoLocks() {
  var hasUrl = $("#Form_VideoUrl").val().trim().length > 0;
  var videoInput = document.getElementById("Form_Video");
  var hasFile = !!(videoInput && videoInput.files.length);
  $(videoInput).prop("disabled", hasUrl);
  $("#Form_VideoUrl").prop("disabled", hasFile);
}

function lockDuration() {
  $("#Form_Duration").prop("disabled", true);
}

function unlockDuration() {
  $("#Form_Duration").prop("disabled", false);
}

/** Formats a number of seconds as M:SS (e.g. 865 → "14:25"). */
function formatDuration(totalSeconds) {
  var s = Math.round(totalSeconds);
  var m = Math.floor(s / 60);
  var sec = s % 60;
  return m + ":" + String(sec).padStart(2, "0");
}

/**
 * Reads the real duration of a video (file object-URL or remote URL)
 * by loading its metadata in an off-screen <video> element, then
 * fills and locks the duration field.
 */
function probeVideoDuration(src, revokeObjectUrl) {
  var video = document.createElement("video");
  video.preload = "metadata";
  video.onloadedmetadata = function () {
    if (!isFinite(video.duration) || video.duration <= 0) {
      if (revokeObjectUrl) URL.revokeObjectURL(video.src);
      return;
    }
    $("#Form_Duration").val(formatDuration(video.duration));
    lockDuration();
    if (revokeObjectUrl) URL.revokeObjectURL(video.src);
  };
  video.onerror = function () {
    // Metadata unavailable (e.g. host without CORS/metadata access):
    // keep the duration field editable and unlocked.
    if (revokeObjectUrl) URL.revokeObjectURL(video.src);
  };
  video.src = src;
}

// File selected → clear & lock the URL input, probe the file duration.
$("#Form_Video").on("change", function () {
  if (this.files.length) {
    $("#Form_VideoUrl").val("");
    lastProbedUrl = "";
    syncVideoLocks();
    probeVideoDuration(URL.createObjectURL(this.files[0]), true);
  } else {
    syncVideoLocks();
    unlockDuration();
  }
});

// Typing a URL → clear & lock the file input.
$("#Form_VideoUrl").on("input", function () {
  if ($(this).val().trim()) {
    var videoInput = document.getElementById("Form_Video");
    if (videoInput && videoInput.files.length) videoInput.value = "";
    unlockDuration(); // until the URL metadata is probed on blur
  } else {
    lastProbedUrl = "";
    unlockDuration();
  }
  syncVideoLocks();
});

// URL finished (blur/enter) → probe the remote video duration.
$("#Form_VideoUrl").on("change", function () {
  var url = $(this).val().trim();
  if (url) {
    if (/^https?:\/\//i.test(url)) {
      if (url !== lastProbedUrl) {
        lastProbedUrl = url;
        probeVideoDuration(url, false);
      }
    } else {
      toastr.warning("آدرس ویدیو باید با http یا https شروع شود.", "هشدار");
    }
  }
  syncVideoLocks();
});
//#endregion

//#region Drag & Drop Reordering
// Renumbers the «ترتیب» badges and persists the new order after a drop.
// TODO: pass onSave here (POST [{ id, displayOrder }] to ?handler=Reorder)
// once the data layer is connected.
reorder = AdminCrud.bindRowReorder(page.table, {
  badge: ".episode-order",
  orderCol: 1,
  logLabel: "Reorder episodes",
  successMsg: "ترتیب اپیزودها با موفقیت ذخیره شد.",
});
//#endregion

//#region Inline onclick Handlers (HTML compatibility)
function openCreateModal() {
  page.openCreate();
}
function submitEpisodeForm() {
  page.submit();
}
function confirmDelete() {
  page.confirmDelete();
}
function openDeleteModal(id, name) {
  page.openDelete(id, name);
}
//#endregion

//#region Edit From Row Data
/**
 * Edit: keeps the inline onclick signature
 * (id, title, duration, isFree, displayOrder);
 * the values are passed in directly from the table row.
 */
function openEditModal(id, title, duration, isFree, displayOrder) {
  page.state.mode = "edit";
  page.state.editId = id;
  $("#modal-episode-title").text("ویرایش اپیزود");
  $("#Form_Title").val(title);
  $("#Form_Description").val("");
  resetVideoInputs();
  $("#Form_Duration").val(duration);
  unlockDuration();
  $("#Form_IsFree").prop("checked", isFree);
  $("#Form_DisplayOrder").val(displayOrder);
  AdminCrud.Utils.showModal("kt_modal_episode");
}
//#endregion
//#endregion
