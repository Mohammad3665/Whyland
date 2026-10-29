(function () {
  "use strict";

  var tables = document.querySelectorAll("[data-live-messages]");
  if (!tables.length || typeof signalR === "undefined") {
    return;
  }

  var dot = document.getElementById("live-dot");
  var text = document.getElementById("live-text");

  // Literal class names only: Tailwind cannot see concatenated utilities.
  var TONE = {
    idle: "inline-block h-2 w-2 rounded-full bg-gray-300",
    pending: "inline-block h-2 w-2 rounded-full bg-warning",
    live: "inline-block h-2 w-2 rounded-full bg-success",
    dead: "inline-block h-2 w-2 rounded-full bg-danger",
  };

  function setStatus(label, tone) {
    if (text) {
      text.textContent = label;
    }
    if (dot) {
      dot.className = TONE[tone] || TONE.idle;
    }
  }

  // The Messages page filter inputs do not exist on the dashboard, so the compact feed accepts
  // every message. Mirrors InMemorySmsStore.ListMessages: ordinal substring, case-insensitive status.
  function acceptForFullTable(message) {
    var statusField = document.getElementById("status");
    var recipientField = document.getElementById("recipient");

    if (statusField && statusField.value) {
      if (message.status.toLowerCase() !== statusField.value.toLowerCase()) {
        return false;
      }
    }

    if (recipientField && recipientField.value.trim()) {
      if (message.recipient.indexOf(recipientField.value.trim()) === -1) {
        return false;
      }
    }

    return true;
  }

  function cell(classes, value, dir) {
    var td = document.createElement("td");
    td.className = classes;
    td.textContent = value;
    if (dir) {
      td.setAttribute("dir", dir);
    }
    return td;
  }

  function statusCell(classes, message) {
    var td = document.createElement("td");
    td.className = classes;
    var badge = document.createElement("span");
    badge.className =
      message.statusTone +
      " inline-flex items-center rounded-full px-2.5 py-1 text-[11px] font-semibold leading-none whitespace-nowrap";
    badge.textContent = message.statusLabel;
    td.appendChild(badge);
    return td;
  }

  // Every node is built with createElement/textContent: the message body is caller-supplied text.
  function fullRow(message) {
    var row = document.createElement("tr");
    row.className = "border-t border-gray-100 transition-colors hover:bg-light/60";

    row.appendChild(cell("whitespace-nowrap px-4 py-3 font-medium", message.recipient, "ltr"));
    row.appendChild(cell("min-w-56 px-4 py-3 text-body", message.preview));
    row.appendChild(cell("whitespace-nowrap px-4 py-3", message.templateCode || "—", "ltr"));
    row.appendChild(statusCell("whitespace-nowrap px-4 py-3", message));
    row.appendChild(cell("whitespace-nowrap px-4 py-3 text-gray-400", message.createdAt, "ltr"));

    var details = document.createElement("td");
    details.className = "whitespace-nowrap px-4 py-3";
    var link = document.createElement("a");
    link.className = "font-semibold text-primary transition-opacity hover:opacity-80";
    link.href = message.detailsUrl;
    link.textContent = "مشاهده";
    details.appendChild(link);
    row.appendChild(details);

    return row;
  }

  function compactRow(message) {
    var row = document.createElement("tr");
    row.className = "border-t border-gray-100";

    row.appendChild(cell("whitespace-nowrap px-4 py-2.5 font-medium", message.recipient, "ltr"));
    row.appendChild(cell("px-4 py-2.5 text-body", message.shortPreview));
    row.appendChild(statusCell("px-4 py-2.5", message));
    row.appendChild(cell("whitespace-nowrap px-4 py-2.5 text-gray-400", message.createdTime, "ltr"));

    return row;
  }

  function appendTo(tbody, message) {
    var full = tbody.getAttribute("data-live-messages") === "full";
    if (full && !acceptForFullTable(message)) {
      return false;
    }

    var emptyRow = tbody.querySelector("[data-live-empty]");
    if (emptyRow) {
      emptyRow.remove();
    }

    tbody.insertBefore(full ? fullRow(message) : compactRow(message), tbody.firstChild);

    var max = parseInt(tbody.getAttribute("data-limit"), 10);
    if (Number.isFinite(max) && max > 0) {
      while (tbody.children.length > max) {
        tbody.removeChild(tbody.lastElementChild);
      }
    }

    return true;
  }

  function bumpCounter(message) {
    // The dashboard cards are computed over the stored window, so a live message raises the
    // message counter without pretending a rejected API call ever happened.
    var counter = document.querySelector('[data-live-count="messages"]');
    if (!counter) {
      return;
    }
    var current = parseInt(counter.textContent, 10);
    if (Number.isFinite(current)) {
      counter.textContent = String(current + 1);
    }
  }

  function broadcast(message) {
    var shown = false;
    tables.forEach(function (tbody) {
      shown = appendTo(tbody, message) || shown;
    });
    if (shown) {
      bumpCounter(message);
    }
  }

  var connection = new signalR.HubConnectionBuilder()
    .withUrl("/hubs/sms", { withCredentials: true })
    .withAutomaticReconnect([1000, 3000, 10000, 30000])
    .build();

  connection.on("MessageStored", broadcast);

  connection.onreconnecting(function () {
    setStatus("اتصال قطع شد، تلاش مجدد…", "pending");
  });
  connection.onreconnected(function () {
    setStatus("زنده", "live");
  });
  connection.onclose(function () {
    setStatus("قطع", "dead");
  });

  setStatus("در حال اتصال…", "idle");

  connection
    .start()
    .then(function () {
      setStatus("زنده", "live");
    })
    .catch(function () {
      // 401 means the panel cookie is missing or expired.
      setStatus("قطع (وارد پنل شوید)", "dead");
    });
})();