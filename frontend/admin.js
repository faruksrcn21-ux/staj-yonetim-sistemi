/* Komisyon / Yönetici paneli - sadece admin.html'de çalışır (app.js önce yüklenmeli) */
(function () {
  const user = Session.require(["komisyon", "yonetici"]);
  if (!user) return;

  const rowsEl = document.getElementById("rows");
  const emptyEl = document.getElementById("empty");
  const statsEl = document.getElementById("stats");
  const searchEl = document.getElementById("search");
  const filterEl = document.getElementById("status-filter");

  const dialog = document.getElementById("review-dialog");
  const dlgInfo = document.getElementById("dlg-info");
  const dlgDocs = document.getElementById("dlg-docs");
  const feedbackEl = document.getElementById("feedback");
  const dlgMsg = document.getElementById("dlg-message");
  const approveBtn = document.getElementById("dlg-approve");
  const rejectBtn = document.getElementById("dlg-reject");

  let applications = [];
  let current = null; // incelenen başvuru

  document.getElementById("user-name").textContent = user.fullName;
  document.getElementById("logout-btn").addEventListener("click", () => Session.logout());

  /* ---- Özet sayaçları ---- */
  function renderStats() {
    const count = s => applications.filter(a => a.status === s).length;
    const items = [
      ["Toplam", applications.length],
      ["Beklemede", count("pending")],
      ["Onaylandı", count("approved")],
      ["Reddedildi", count("rejected")]
    ];
    statsEl.innerHTML = items.map(([label, n]) => '<div class="stat"><b>' + n + "</b><span>" + label + "</span></div>").join("");
  }

  /* ---- Tablo ---- */
  function filtered() {
    const q = searchEl.value.trim().toLocaleLowerCase("tr");
    const st = filterEl.value;
    return applications.filter(a => {
      if (st !== "all" && a.status !== st) return false;
      if (!q) return true;
      return [a.studentName, a.studentNo, a.company].join(" ").toLocaleLowerCase("tr").includes(q);
    });
  }

  function renderTable() {
    const list = filtered();
    emptyEl.hidden = list.length > 0;
    rowsEl.innerHTML = list.map(a =>
      "<tr>" +
        "<td>" + esc(a.studentName) + "</td>" +
        "<td>" + esc(a.studentNo) + "</td>" +
        "<td>" + esc(a.company) + "</td>" +
        "<td>" + fmtDate(a.startDate) + " - " + fmtDate(a.endDate) + "</td>" +
        "<td>" + statusBadge(a.status) + "</td>" +
        '<td><button type="button" class="btn" data-id="' + a.id + '">İncele</button></td>' +
      "</tr>"
    ).join("");
  }

  async function load() {
    try {
      applications = await api.getAllApplications();
      renderStats();
      renderTable();
    } catch (err) {
      toast(err.message, "error");
    }
  }

  searchEl.addEventListener("input", renderTable);
  filterEl.addEventListener("change", renderTable);

  rowsEl.addEventListener("click", (e) => {
    const btn = e.target.closest("button[data-id]");
    if (!btn) return;
    openReview(applications.find(a => String(a.id) === btn.dataset.id));
  });

  /* ---- İnceleme penceresi ---- */
  function setBusy(busy) {
    approveBtn.disabled = busy;
    rejectBtn.disabled = busy;
  }

  function showDlgMessage(text) {
    dlgMsg.textContent = text;
    dlgMsg.className = "message is-error";
    dlgMsg.hidden = !text;
  }

  function openReview(app) {
    if (!app) return;
    current = app;
    dlgInfo.innerHTML =
      "<dt>Öğrenci</dt><dd>" + esc(app.studentName) + " (" + esc(app.studentNo) + ")</dd>" +
      "<dt>Firma</dt><dd>" + esc(app.company) + "</dd>" +
      "<dt>Tarih aralığı</dt><dd>" + fmtDate(app.startDate) + " - " + fmtDate(app.endDate) + "</dd>" +
      "<dt>Durum</dt><dd>" + statusBadge(app.status) + "</dd>";

    dlgDocs.innerHTML = app.documents.length
      ? app.documents.map(d =>
          '<div class="doc-row"><span>' + esc(d) + '</span>' +
          '<button type="button" class="btn" data-doc="' + esc(d) + '">Görüntüle</button></div>'
        ).join("")
      : '<p class="muted">Belge yüklenmemiş.</p>';

    feedbackEl.value = app.feedback || "";
    showDlgMessage("");
    setBusy(false);
    dialog.showModal();
  }

  dlgDocs.addEventListener("click", (e) => {
    const btn = e.target.closest("button[data-doc]");
    if (!btn || !current) return;
    const url = documentUrl(current.id, btn.dataset.doc);
    if (!url) return toast("Sahte veri: belge önizlemesi backend bağlanınca açılacak.", "error");
    window.open(url, "_blank", "noopener");
  });

  async function decide(status) {
    const feedback = feedbackEl.value.trim();
    if (status === "rejected" && feedback.length < 5) {
      return showDlgMessage("Reddetmek için gerekçeyi nota yaz (en az 5 karakter).");
    }
    setBusy(true);
    showDlgMessage("");
    try {
      await api.reviewApplication(current.id, { status, feedback });
      dialog.close();
      toast(status === "approved" ? "Başvuru onaylandı." : "Başvuru reddedildi.");
      await load();
    } catch (err) {
      showDlgMessage(err.message);
      setBusy(false);
    }
  }

  approveBtn.addEventListener("click", () => decide("approved"));
  rejectBtn.addEventListener("click", () => decide("rejected"));
  document.getElementById("dlg-close").addEventListener("click", () => dialog.close());

  load();
})();