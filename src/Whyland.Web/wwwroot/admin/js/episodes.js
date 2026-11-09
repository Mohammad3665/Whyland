//#region Episodes Page Scripts
/**
 * Admin "Episodes" page: the episodes of one course section, ordered by DisplayOrder.
 * Rows, saves, deletes and drag & drop ordering go through the Razor Page handlers and the
 * shared AdminCrud module. A video can be either an uploaded protected MP4 or an external
 * http(s) link; the two inputs are mutually exclusive, and the link is stored only after the
 * server validates it. Replacing or removing the video goes through its own handler so the
 * protected file is cleaned up server-side.
 */

var page; // shared CRUD page controller (set in initPage)
var reorder; // shared drag & drop ordering controller (set in initPage)
var existingVideoType = null; // "file" | "link" | null, as loaded by the edit modal
var previewObjectUrl = null; // object URL of the picked file, revoked when it changes
var currentDetailsEpisode = null; // row shown by the details modal
var picker; // generic filter picker (set in the boot block)

/** Conditions of the generic picker, including the value currently typed in it. */
function episodeConditions() {
  return picker ? picker.conditions() : [];
}

/** Re-runs the client-side DataTable with the current filter conditions. */
function applyEpisodeFilters() {
  if (page) page.table.draw(false);
}

function addFilterCondition() {
  if (picker) picker.add();
}

function clearFilterConditions() {
  if (picker) picker.clear();
}

/** Boolean fields get their own value box so «رایگان» reads as بله/خیر, not فعال/غیرفعال. */
var episodeFilterKinds = {
  boolean: {
    fill: function ($select) {
      $select.append($("<option>").val("").text("انتخاب..."));
      $select.append($("<option>").val("true").text("بله"));
      $select.append($("<option>").val("false").text("خیر"));
    },
    label: function (field, value) {
      return String(value).toLowerCase() === "true" ? "بله" : "خیر";
    },
  },
};

//#region Server Connectivity (Razor Page handlers)
function csrfToken() {
  return $('input[name="__RequestVerificationToken"]').val();
}

function sectionId() {
  return parseInt($("#sectionId").val(), 10) || 0;
}

function courseId() {
  return parseInt($("#courseId").val(), 10) || 0;
}

/**
 * Sends a form-encoded POST and adapts the flat {isSuccess, message} envelope to the AdminCrud
 * contract, so the shared module can show the server's validation message.
 */
function postHandler(handler, data) {
  var dfd = $.Deferred();
  $.post({
    url: location.pathname + "?handler=" + handler,
    data: data,
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

/**
 * Sends a multipart POST (file upload) with the same envelope handling as postHandler.
 */
function postFormData(handler, formData) {
  var dfd = $.Deferred();
  $.ajax({
    url: location.pathname + "?handler=" + handler,
    type: "POST",
    data: formData,
    processData: false,
    contentType: false,
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

function uploadVideo(id, file) {
  var formData = new FormData();
  formData.append("id", id);
  formData.append("file", file);
  return postFormData("UploadVideo", formData);
}

function setVideoLink(id, url) {
  return postHandler("SetVideoLink", { id: id, url: url });
}

function removeVideo(id) {
  return postHandler("RemoveVideo", { id: id });
}

function deleteEpisode(id) {
  return postHandler("Delete", { id: id }).then(function (res) {
    // Keep the confirmation open until the table has actually dropped the row.
    return refreshRows().then(function () {
      return res;
    });
  });
}

function saveOrder(rows) {
  $.ajax({
    url: location.pathname + "?handler=Reorder&sectionId=" + sectionId(),
    type: "POST",
    contentType: "application/json; charset=UTF-8",
    data: JSON.stringify(rows),
    headers: { RequestVerificationToken: csrfToken() },
  })
    .done(function (res) {
      if (res && res.isSuccess) {
        toastr.success("ترتیب اپیزودها با موفقیت ذخیره شد.", "موفق");
      } else {
        toastr.error((res && res.message) || "خطا در ذخیره ترتیب.", "خطا");
        refreshRows();
      }
    })
    .fail(function () {
      toastr.error("خطا در ذخیره ترتیب.", "خطا");
      refreshRows();
    });
}
//#endregion

//#region Video Source Helpers
/** Formats a duration in seconds as M:SS; null/zero renders as a dash. */
function formatDuration(totalSeconds) {
  if (!totalSeconds || totalSeconds <= 0) return "—";
  var s = Math.round(totalSeconds);
  var m = Math.floor(s / 60);
  var sec = s % 60;
  return m + ":" + String(sec).padStart(2, "0");
}

/** Reflects the current video state in the modal and shows/hides the remove action. */
function setVideoStatus(hasVideo, videoUrl) {
  var $status = $("#current-video-status");
  var $remove = $("#remove-video-btn");

  if (!hasVideo) {
    $status.text("ویدیویی ثبت نشده است.");
    $remove.addClass("d-none");
    return;
  }

  var saved = !!(existingVideoType && page && page.state.editId);
  $status.text(
    saved
      ? videoUrl
        ? "ویدیوی فعلی: لینک خارجی"
        : "ویدیوی فعلی: فایل محافظت‌شده"
      : videoUrl
        ? "ویدیوی انتخاب‌شده: لینک خارجی"
        : "ویدیوی انتخاب‌شده: فایل",
  );
  $remove.removeClass("d-none");
}

/** Clears the file input; used when the operator switches to a link. */
function clearVideoFile() {
  var input = document.getElementById("Form_Video");
  if (input) input.value = "";
}

/** Clears the link input; used when the operator switches to a file. */
function clearVideoUrl() {
  $("#Form_VideoUrl").val("");
}

/**
 * Enforces the single-source rule in the UI: a chosen file locks the link input, and a filled
 * link locks the file input. A video that already exists locks the alternative too; removing the
 * current video or clearing the link unlocks it again.
 */
function syncVideoInputs() {
  var fileInput = document.getElementById("Form_Video");
  var hasFile = !!(fileInput && fileInput.files && fileInput.files.length);
  var hasUrl = !!$("#Form_VideoUrl").val().trim();

  $("#Form_VideoUrl").prop(
    "disabled",
    hasFile || (!hasUrl && existingVideoType === "file"),
  );
  $("#Form_Video").prop("disabled", !hasFile && hasUrl);

  $("#Form_Video")
    .closest(".fv-row")
    .toggleClass("opacity-50", $("#Form_Video").prop("disabled"));
  $("#Form_VideoUrl")
    .closest(".fv-row")
    .toggleClass("opacity-50", $("#Form_VideoUrl").prop("disabled"));
}

/**
 * YouTube, Vimeo and Aparat watch URLs cannot play in a <video> element; their player URL is used
 * in an iframe instead. A direct media URL returns null and stays on the native player.
 */
function videoEmbedUrl(url) {
  if (!url) return null;

  try {
    var parsed = new URL(url);
    var host = parsed.hostname.replace(/^www\./, "").toLowerCase();

    if (host === "youtu.be") {
      var shortId = parsed.pathname.split("/")[1];
      return shortId ? "https://www.youtube.com/embed/" + shortId : null;
    }

    if (host === "youtube.com" || host === "m.youtube.com" || host === "youtube-nocookie.com") {
      var id = parsed.searchParams.get("v");
      if (!id) {
        var match = parsed.pathname.match(/^\/(?:embed|shorts|live)\/([^/?#]+)/);
        id = match ? match[1] : null;
      }
      return id ? "https://www.youtube.com/embed/" + id : null;
    }

    if (host === "vimeo.com" || host === "player.vimeo.com") {
      var vimeo = parsed.pathname.match(/(\d+)/);
      return vimeo ? "https://player.vimeo.com/video/" + vimeo[1] : null;
    }

    if (host === "aparat.com") {
      // Both share links (/v/{hash}) and the official embed URL are accepted.
      var aparat = parsed.pathname.match(
        /\/(?:v|video\/video\/embed\/videohash)\/([A-Za-z0-9]+)/,
      );
      return aparat
        ? "https://www.aparat.com/video/video/embed/videohash/" +
            aparat[1] +
            "/vt/frame"
        : null;
    }
  } catch (error) {
    return null;
  }

  return null;
}

function hideVideoPreviewError() {
  $("#episode-video-preview-error").addClass("d-none").text("");
}

function showVideoPreviewError() {
  $("#episode-video-preview-error")
    .removeClass("d-none")
    .text(
      "این لینک در پیش‌نمایش پخش نشد. آدرس باید مستقیم به فایل ویدیویی (مثل MP4) باشد یا لینک YouTube، Vimeo یا Aparat.",
    );
}

/**
 * Previews the source the operator just picked: the chosen file renders from a temporary object
 * URL, a direct link renders from the link itself, YouTube/Vimeo links use an iframe, and an
 * existing video renders through the protected media endpoint. The previous object URL is always
 * revoked so a replaced file is not retained.
 */
function updateVideoPreview() {
  var video = document.getElementById("episode-video-preview");
  var embed = document.getElementById("episode-video-preview-embed");
  if (!video) return;

  if (previewObjectUrl) {
    URL.revokeObjectURL(previewObjectUrl);
    previewObjectUrl = null;
  }

  hideVideoPreviewError();

  var fileInput = document.getElementById("Form_Video");
  var file = fileInput && fileInput.files.length ? fileInput.files[0] : null;
  var url = $("#Form_VideoUrl").val().trim();
  var src = null;
  var embedUrl = null;

  if (file) {
    previewObjectUrl = URL.createObjectURL(file);
    src = previewObjectUrl;
  } else if (url) {
    embedUrl = videoEmbedUrl(url);
    src = embedUrl ? null : url;
  } else if (existingVideoType && page && page.state.editId) {
    src = "/media/episodes/" + page.state.editId + "/video";
  }

  if (!src && !embedUrl) {
    $("#video-preview-wrapper").addClass("d-none");
    if (embed) $("#episode-video-preview-embed").addClass("d-none").removeAttr("src");
    video.pause();
    video.removeAttribute("src");
    video.load();
    return;
  }

  $("#video-preview-wrapper").removeClass("d-none");

  if (embedUrl) {
    video.pause();
    video.removeAttribute("src");
    video.load();
    video.classList.add("d-none");
    $(embed)
      .removeClass("d-none")
      .attr("src", embedUrl);
    return;
  }

  if (embed) $(embed).addClass("d-none").removeAttr("src");
  video.classList.remove("d-none");

  if (video.getAttribute("src") !== src) {
    video.src = src;
    video.load();
  }
}

/** Stops playback and releases the temporary file URL when a modal closes. */
function resetVideoPreview() {
  var video = document.getElementById("episode-video-preview");
  if (video) {
    video.pause();
    video.removeAttribute("src");
    video.load();
  }

  var embed = document.getElementById("episode-video-preview-embed");
  if (embed) $(embed).addClass("d-none").removeAttr("src");

  hideVideoPreviewError();

  if (previewObjectUrl) {
    URL.revokeObjectURL(previewObjectUrl);
    previewObjectUrl = null;
  }

  $("#video-preview-wrapper").addClass("d-none");
}
//#endregion

//#region Row Rendering
function faNumber(value) {
  return Number(value || 0).toLocaleString("fa-IR");
}

function attachmentsUrl(episodeId) {
  return (
    "/Admin/Catalog/Courses/Episodes/EpisodeAttachments?episodeId=" +
    episodeId +
    "&sectionId=" +
    sectionId() +
    "&courseId=" +
    courseId()
  );
}

function formatFileSize(bytes) {
  if (bytes >= 1024 * 1024) return Math.round(bytes / (1024 * 1024)) + " MB";
  if (bytes >= 1024) return Math.round(bytes / 1024) + " KB";
  return (bytes || 0) + " B";
}

function formatDateTime(value) {
  if (!value) return "—";
  var date = new Date(value);
  if (isNaN(date.getTime())) return "—";
  return date.toLocaleString("fa-IR");
}

function buildEpisodeRow(episode) {
  var $row = $(
    '<tr><td class="dtr-control-col"></td>' +
      '<td tabindex="0" class="reorder text-center dt-type-numeric dtr-control">' +
      '<div class="d-flex align-items-center justify-content-center">' +
      '<i class="ki-duotone ki-burger-menu-2 fs-2 text-gray-400 cursor-move me-2">' +
      '<span class="path1"></span><span class="path2"></span><span class="path3"></span>' +
      '<span class="path4"></span><span class="path5"></span><span class="path6"></span>' +
      '<span class="path7"></span><span class="path8"></span><span class="path9"></span><span class="path10"></span>' +
      "</i>" +
      '<span class="episode-order badge badge-light-secondary"></span>' +
      "</div></td>" +
      "<td></td>" +
      '<td class="episode-duration"></td>' +
      '<td class="episode-video"></td>' +
      '<td class="episode-attachments"></td>' +
      '<td class="episode-free"></td>' +
      '<td class="text-end nowrap-cell"></td>' +
      "</tr>",
  );

  $row.attr("data-id", episode.id).data("entity", episode);

  var cells = $row.find("td");
  $(cells[1]).find(".episode-order").text(faNumber(episode.displayOrder));

  // The title is the details trigger; the full description lives in that modal instead of
  // stretching the row. Line clamping keeps a long title inside its fixed-width column.
  var $title = $(
    '<button type="button" class="episode-title-link btn btn-link p-0 text-start text-gray-800 fw-bold">',
  )
    .text(episode.title)
    .attr("title", episode.title);
  $title.on("click", function () {
    openEpisodeDetails(episode);
  });
  $(cells[2]).append($title);

  const $wrap = $('<span class="d-inline-flex align-items-center gap-1">');
  $wrap.append(
    '<i class="ki-duotone ki-time fs-4 text-gray-500">' +
      '<span class="path1"></span><span class="path2"></span></i>',
  );
  $wrap.append(
    $('<span class="text-gray-600">').text(
      formatDuration(episode.durationSeconds),
    ),
  );
  $(cells[3]).append($wrap);

  var $video = $(cells[4]);
  if (episode.videoUrl) {
    $video.append(
      $(
        '<a target="_blank" rel="noopener noreferrer" class="badge badge-light-info">',
      )
        .attr("href", episode.videoUrl)
        .text("لینک خارجی"),
    );
  } else if (episode.hasVideo) {
    $video.append(
      $('<span class="badge badge-light-success">').text("فایل محافظت‌شده"),
    );
  } else {
    $video.append($('<span class="text-muted fs-7">').text("—"));
  }

  var attachmentsUrlForRow = attachmentsUrl(episode.id);
  var $attachmentsLink = $(
    '<a class="badge badge-light-primary text-hover-primary">',
  )
    .attr("href", attachmentsUrlForRow)
    .text(faNumber(episode.attachmentCount) + " پیوست");
  $(cells[5]).append($attachmentsLink);

  $(cells[6]).append(
    episode.isFree
      ? $('<span class="badge badge-light-success">').text("رایگان")
      : $('<span class="badge badge-light-secondary">').text("قفل"),
  );

  var $details = $(
    '<button type="button" class="btn btn-sm btn-icon btn-bg-light btn-active-color-info me-1" title="جزئیات">',
  ).append(
    '<i class="ki-duotone ki-eye fs-2"><span class="path1"></span><span class="path2"></span><span class="path3"></span></i>',
  );
  $details.on("click", function () {
    openEpisodeDetails(episode);
  });

  var $attachments = $(
    '<a class="btn btn-sm btn-icon btn-bg-light btn-active-color-success me-1" title="پیوست‌ها">',
  )
    .attr("href", attachmentsUrlForRow)
    .append(
      '<i class="ki-duotone ki-paper-clip fs-2"><span class="path1"></span><span class="path2"></span></i>',
    );

  var $edit = $(
    '<button type="button" class="btn btn-sm btn-icon btn-bg-light btn-active-color-primary me-1" title="ویرایش">',
  ).append(
    '<i class="ki-duotone ki-pencil fs-2"><span class="path1"></span><span class="path2"></span></i>',
  );
  $edit.on("click", function () {
    openEditModal(episode);
  });

  var $delete = $(
    '<button type="button" class="btn btn-sm btn-icon btn-bg-light btn-active-color-danger" title="حذف">',
  ).append(
    '<i class="ki-duotone ki-trash fs-2"><span class="path1"></span><span class="path2"></span><span class="path3"></span><span class="path4"></span><span class="path5"></span></i>',
  );
  $delete.on("click", function () {
    if (page) page.openDelete(episode.id, episode.title);
  });

  $(cells[7]).append($details, $attachments, $edit, $delete);
  return $row[0];
}

function loadRows() {
  return $.getJSON(
    location.pathname + "?handler=Rows&sectionId=" + sectionId(),
  ).then(function (rows) {
    var nodes = [];
    for (var i = 0; i < rows.length; i++) nodes.push(buildEpisodeRow(rows[i]));
    return nodes;
  });
}

function refreshRows() {
  return loadRows().then(function (rows) {
    $("#kt_episodes_table tbody").empty().append(rows);
    page.table.clear();
    page.table.rows.add(rows).draw();
    reorder.renumber();
  });
}
//#endregion

//#region Details Modal
/** Renders the downloadable attachments of the episode shown in the details modal. */
function loadEpisodeAttachments(episodeId) {
  var $list = $("#episode-details-attachments").empty();
  var $state = $("#episode-details-attachments-state")
    .removeClass("d-none")
    .text("در حال بارگذاری پیوست‌ها...");

  $.getJSON(location.pathname + "?handler=Attachments&id=" + episodeId)
    .done(function (attachments) {
      if (!currentDetailsEpisode || currentDetailsEpisode.id !== episodeId)
        return;

      if (!attachments.length) {
        $state.text("پیوستی برای این اپیزود ثبت نشده است.");
        return;
      }

      $state.addClass("d-none");

      for (var i = 0; i < attachments.length; i++) {
        var attachment = attachments[i];
        var $link = $(
          '<a class="notice d-flex align-items-center justify-content-between bg-light-primary border border-dashed border-primary rounded p-4 text-gray-800">',
        )
          .attr("href", "/media/attachments/" + attachment.id)
          .attr("download", attachment.fileName);

        var $info = $('<div class="d-flex align-items-center">');
        $info.append(
          '<i class="ki-duotone ki-file-down fs-2x text-primary me-3">' +
            '<span class="path1"></span><span class="path2"></span></i>',
        );

        var $text = $('<div class="d-flex flex-column">');
        $text.append(
          $('<span class="fw-semibold text-gray-800" dir="ltr">').text(
            attachment.fileName,
          ),
        );
        $text.append(
          $('<span class="text-muted fs-8">').text(
            formatFileSize(attachment.size) +
              " • " +
              (attachment.contentType || "فایل"),
          ),
        );
        $info.append($text);

        $link.append($info);
        $link.append(
          '<i class="ki-duotone ki-download fs-2 text-primary">' +
            '<span class="path1"></span><span class="path2"></span></i>',
        );

        $list.append($link);
      }
    })
    .fail(function () {
      if (!currentDetailsEpisode || currentDetailsEpisode.id !== episodeId)
        return;
      $state.text("خطا در دریافت پیوست‌ها.");
    });
}

/** RowReorder changes the visible order badge; the details modal must show that current order. */
function openEpisodeDetails(episode) {
  var snapshot = $.extend({}, episode);
  if (reorder)
    snapshot.displayOrder = reorder.orderOf(episode.id, episode.displayOrder);
  openDetailsModal(snapshot);
}

/** Opens the read-only details modal for one episode row. */
function openDetailsModal(episode) {
  currentDetailsEpisode = episode;

  $("#episode-details-title").text(episode.title);
  $("#episode-details-id").text(episode.id);
  $("#episode-details-section").text(episode.sectionId);
  $("#episode-details-order").text(faNumber(episode.displayOrder));
  $("#episode-details-duration").text(
    formatDuration(episode.durationSeconds),
  );
  $("#episode-details-attachment-count").text(
    faNumber(episode.attachmentCount),
  );
  $("#episode-details-created").text(formatDateTime(episode.createdAt));
  $("#episode-details-description").text(
    episode.description || "توضیحی ثبت نشده است.",
  );

  if (episode.videoUrl) {
    $("#episode-details-video-url-wrapper").removeClass("d-none");
    $("#episode-details-video-url")
      .attr("href", episode.videoUrl)
      .text(episode.videoUrl);
  } else {
    $("#episode-details-video-url-wrapper").addClass("d-none");
    $("#episode-details-video-url").removeAttr("href").text("");
  }

  $("#episode-details-free")
    .removeClass("badge-light-success badge-light-secondary")
    .addClass(episode.isFree ? "badge-light-success" : "badge-light-secondary")
    .text(episode.isFree ? "اپیزود رایگان" : "نیازمند خرید");

  var $type = $("#episode-details-video-type");
  if (!episode.hasVideo) {
    $type
      .removeClass("badge-light-success badge-light-info")
      .addClass("badge-light-secondary")
      .text("بدون ویدیو");
  } else if (episode.videoUrl) {
    $type
      .removeClass("badge-light-success badge-light-secondary")
      .addClass("badge-light-info")
      .text("ویدیوی لینک خارجی");
  } else {
    $type
      .removeClass("badge-light-info badge-light-secondary")
      .addClass("badge-light-success")
      .text("فایل محافظت‌شده");
  }

  var video = document.getElementById("episode-details-video");
  var embed = document.getElementById("episode-details-video-embed");
  var embedUrl = episode.videoUrl ? videoEmbedUrl(episode.videoUrl) : null;

  $("#episode-details-video-error").addClass("d-none").text("");

  if (episode.hasVideo) {
    $("#episode-details-video-wrapper").removeClass("d-none");
    $("#episode-details-video-empty").addClass("d-none");

    if (embedUrl) {
      video.pause();
      video.removeAttribute("src");
      video.load();
      video.classList.add("d-none");
      $(embed)
        .removeClass("d-none")
        .attr("src", embedUrl);
    } else {
      if (embed) $(embed).addClass("d-none").removeAttr("src");
      video.classList.remove("d-none");
      video.src = episode.videoUrl
        ? episode.videoUrl
        : "/media/episodes/" + episode.id + "/video";
      video.load();
    }
  } else {
    $("#episode-details-video-wrapper").addClass("d-none");
    $("#episode-details-video-empty").removeClass("d-none");
    if (embed) $(embed).addClass("d-none").removeAttr("src");
    video.pause();
    video.removeAttribute("src");
    video.load();
  }

  loadEpisodeAttachments(episode.id);
  AdminCrud.Utils.showModal("kt_modal_episode_details");
}

/** Moves from the read-only details modal into the edit form for the same row. */
function editEpisodeFromDetails() {
  if (!currentDetailsEpisode) return;
  var episode = currentDetailsEpisode;
  AdminCrud.Utils.hideModal("kt_modal_episode_details");
  openEditModal(episode);
}
//#endregion

//#region CRUD Page Configuration
function initPage() {
  page = AdminCrud.createPage({
    table: {
      selector: "#kt_episodes_table",
      options: $.extend(true, AdminCrud.rowReorderOptions(), {
        columnDefs: [
          { orderable: false, targets: [0, 1, 7] },
          { className: "all", targets: [1, 2] },
          { className: "min-tablet", targets: [3, 4, 5, 6, 7] },
          { className: "dtr-control", targets: 0 },
        ],
      }),
    },
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

    fillForm: function (episode) {
      clearVideoFile();
      clearVideoUrl();
      existingVideoType =
        episode && episode.hasVideo
          ? episode.videoUrl
            ? "link"
            : "file"
          : null;
      setVideoStatus(
        episode ? episode.hasVideo : false,
        episode ? episode.videoUrl : null,
      );

      $("#Form_Title").val(episode ? episode.title : "");
      $("#Form_Description").val(episode ? episode.description || "" : "");
      $("#Form_IsFree").prop("checked", episode ? episode.isFree : false);
      $("#Form_DisplayOrder").val(
        episode ? episode.displayOrder : reorder.getNextOrder(),
      );

      if (episode && episode.videoUrl)
        $("#Form_VideoUrl").val(episode.videoUrl);

      syncVideoInputs();
      updateVideoPreview();
    },

    getData: function () {
      return {
        title: $("#Form_Title").val().trim(),
        description: $("#Form_Description").val().trim(),
        isFree: $("#Form_IsFree").is(":checked"),
        displayOrder: parseInt($("#Form_DisplayOrder").val(), 10) || 0,
        videoUrl: $("#Form_VideoUrl").val().trim(),
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

      var fileInput = document.getElementById("Form_Video");
      var file =
        fileInput && fileInput.files.length ? fileInput.files[0] : null;
      var url = $("#Form_VideoUrl").val().trim();

      if (file && url) {
        errors.push("آپلود فایل و لینک ویدیو هم‌زمان مجاز نیست.");
      }

      if (file) {
        var dot = file.name.lastIndexOf(".");
        var extension = dot >= 0 ? file.name.slice(dot).toLowerCase() : "";
        if (extension !== ".mp4") {
          errors.push("فقط فایل MP4 قابل آپلود است.");
        }

        var maxSize = parseInt(fileInput.dataset.maxSizeBytes, 10) || 0;
        if (maxSize > 0 && file.size > maxSize) {
          errors.push("حجم فایل ویدیو بیشتر از حد مجاز است.");
        }
      }

      if (url && !/^https?:\/\/[^\s]+$/i.test(url)) {
        errors.push("لینک ویدیو باید یک آدرس کامل با http یا https باشد.");
      }

      return errors;
    },

    onSubmit: saveEpisode,
    onDelete: deleteEpisode,
  });

  AdminTable.clientFilter(
    "kt_episodes_table",
    episodeConditions,
    function () {
      return picker ? picker.fields() : [];
    },
  );

  reorder = AdminCrud.bindRowReorder(page.table, {
    badge: ".episode-order",
    orderCol: 1,
    logLabel: "Reorder episodes",
    successMsg: "ترتیب اپیزودها با موفقیت ذخیره شد.",
    onSave: saveOrder,
  });

  // Mutual exclusion: a chosen file clears the link and vice versa, so exactly one source is sent.
  $("#Form_Video").on("change", function () {
    if (this.files.length) {
      clearVideoUrl();
      setVideoStatus(true, null);
    }
    syncVideoInputs();
    updateVideoPreview();
  });

  $("#Form_VideoUrl").on("input", function () {
    if ($(this).val().trim()) {
      clearVideoFile();
      setVideoStatus(true, $(this).val().trim());
    }
    syncVideoInputs();
    updateVideoPreview();
  });

  $("#episode-video-preview").on("error", function () {
    if (this.getAttribute("src")) showVideoPreviewError();
  });

  $("#episode-details-video").on("error", function () {
    if (this.getAttribute("src")) {
      $("#episode-details-video-error")
        .removeClass("d-none")
        .text("ویدیوی این اپیزود در مرورگر پخش نشد.");
    }
  });

  var editModal = document.getElementById("kt_modal_episode");
  if (editModal)
    editModal.addEventListener("hidden.bs.modal", resetVideoPreview);

  var detailsModal = document.getElementById("kt_modal_episode_details");
  if (detailsModal) {
    detailsModal.addEventListener("hidden.bs.modal", function () {
      var video = document.getElementById("episode-details-video");
      if (video) {
        video.pause();
        video.removeAttribute("src");
        video.load();
      }
      var embed = document.getElementById("episode-details-video-embed");
      if (embed) $(embed).addClass("d-none").removeAttr("src");
      $("#episode-details-video-error").addClass("d-none").text("");
      currentDetailsEpisode = null;
    });
  }
}

/**
 * Saves metadata first, then applies the video choice: upload a file, store a link, or leave the
 * existing video untouched. All steps must succeed before the modal closes.
 */
function saveEpisode(data, mode) {
  var payload = {
    title: data.title,
    description: data.description,
    displayOrder: data.displayOrder,
    isFree: data.isFree,
  };

  if (mode === "create") payload.sectionId = sectionId();

  var request =
    mode === "create"
      ? postHandler("Create", payload)
      : postHandler("Update", $.extend({ id: page.state.editId }, payload));

  return request.then(function (res) {
    var id = mode === "create" ? res.id : page.state.editId;
    var fileInput = document.getElementById("Form_Video");
    var file = fileInput && fileInput.files.length ? fileInput.files[0] : null;
    var url = data.videoUrl;

    var chained = $.Deferred().resolve(res).promise();

    if (file) {
      chained = chained.then(function () {
        return uploadVideo(id, file);
      });
    } else if (url) {
      chained = chained.then(function () {
        return setVideoLink(id, url);
      });
    }

    return chained.then(function () {
      // Wait for the reloaded rows before resolving, so the shared controller closes the modal
      // only once the new state is actually on screen.
      return refreshRows().then(function () {
        return res;
      });
    });
  });
}

/** Clears the unsaved file/link selection without touching an already saved video. */
function clearVideoSelection() {
  existingVideoType = null;
  setVideoStatus(false, null);
  clearVideoUrl();
  clearVideoFile();
  syncVideoInputs();
  updateVideoPreview();
}

/**
 * Removes the video after a confirmation. In the create modal (or before a source was saved)
 * there is nothing to delete on the server, so the button just clears the local selection.
 */
function removeEpisodeVideo() {
  var id = page && page.state.editId;
  var hasSavedVideo = !!(id && existingVideoType);

  if (!hasSavedVideo) {
    clearVideoSelection();
    return;
  }

  if (!window.confirm("ویدیوی فعلی این اپیزود حذف شود؟")) return;

  removeVideo(id)
    .done(function () {
      clearVideoSelection();
      refreshRows();
      toastr.success("ویدیوی اپیزود حذف شد.", "موفق");
    })
    .fail(function (xhr) {
      var msg =
        (xhr && xhr.responseJSON && xhr.responseJSON.message) ||
        "خطا در حذف ویدیو.";
      toastr.error(msg, "خطا");
    });
}
//#endregion

//#region Inline onclick Handlers
function openCreateModal() {
  if (page) page.openCreate();
}

function submitEpisodeForm() {
  if (page) page.submit();
}

function confirmDelete() {
  if (page) page.confirmDelete();
}

function openDeleteModal(id, name) {
  if (page) page.openDelete(id, name);
}

/** Edits an episode from the row data; the closure keeps user text out of inline handlers. */
function openEditModal(episode) {
  if (!page) return;
  page.state.mode = "edit";
  page.state.editId = episode.id;
  $("#modal-episode-title").text("ویرایش اپیزود");

  clearVideoFile();
  clearVideoUrl();
  existingVideoType = episode.hasVideo
    ? episode.videoUrl
      ? "link"
      : "file"
    : null;
  setVideoStatus(episode.hasVideo, episode.videoUrl);

  $("#Form_Title").val(episode.title);
  $("#Form_Description").val(episode.description || "");
  $("#Form_IsFree").prop("checked", episode.isFree);
  $("#Form_DisplayOrder").val(
    reorder.orderOf(episode.id, episode.displayOrder),
  );

  if (episode.videoUrl) $("#Form_VideoUrl").val(episode.videoUrl);

  syncVideoInputs();
  updateVideoPreview();
  AdminCrud.Utils.showModal("kt_modal_episode");
}
//#endregion

//#region Boot
$(function () {
  picker = AdminTable.filterPicker({
    fieldEl: "#filterField",
    operatorEl: "#filterOperator",
    valueWrapEl: "#filterValueWrap",
    chipsEl: "#filterChips",
    kinds: episodeFilterKinds,
    onChange: applyEpisodeFilters,
  });

  var fieldsReady = picker.load().then(null, function () {
    toastr.error("خطا در دریافت فیلدهای فیلتر.", "خطا");
    return null;
  });

  $.when(loadRows(), fieldsReady)
    .done(function (rows) {
      $("#kt_episodes_table tbody").append(rows);
      initPage();
      reorder.renumber();
      applyEpisodeFilters();
    })
    .fail(function () {
      initPage();
      toastr.error("خطا در دریافت اپیزودها.", "خطا");
    });
});
//#endregion
//#endregion
