/* Admin ekranı (admin.html) */
(function () {
  const user = Session.require(ROLE_ADMIN);
  if (!user) return;

  const $ = id => document.getElementById(id);
  const dlg = $("review-dialog"), dlgMsg = $("dlg-message"), statusSel = $("status-select");
  let docs = [], current = null;

  $("user-name").textContent = user.username;
  $("logout-btn").addEventListener("click", () => Session.logout());

  /* ---- Sekmeler ---- */
  document.querySelectorAll(".tab[data-tab]").forEach(t => t.addEventListener("click", () => {
    document.querySelectorAll(".tab[data-tab]").forEach(x => {
      x.classList.toggle("is-active", x === t);
    });
    document.querySelectorAll(".tab-section").forEach(s => { s.hidden = s.id !== "tab-" + t.dataset.tab; });
  }));

  /* ---- Belge tabloları ---- */
  function row(d) {
    return "<tr><td>" + esc(d.studentName) + "</td><td>" + esc(d.studentNo) + "</td><td>" + esc(d.fileName) + "</td><td>" +
      fmtDateTime(d.uploadedAt) + "</td><td>" + statusBadge(d.status) + "</td><td>" + (d.comparison && d.comparison.ready ? "Evet" : "Hayır") +
      '</td><td><button type="button" class="btn" data-id="' + d.id + '">İncele</button></td></tr>';
  }

  function renderDocs() {
    const fill = (id, list) => {
      $(id).innerHTML = list.length ? list.map(row).join("") : '<tr><td colspan="7" class="empty">Belge yok.</td></tr>';
    };
    fill("rows-open", docs.filter(d => d.status !== "completed"));
    fill("rows-done", docs.filter(d => d.status === "completed"));
  }

  async function loadDocs() {
    try { docs = await api.getAllDocuments(); renderDocs(); } catch (err) { toast(err.message, "error"); }
  }

  ["rows-open", "rows-done"].forEach(id => $(id).addEventListener("click", (e) => {
    const b = e.target.closest("button[data-id]");
    if (b) openReview(docs.find(d => String(d.id) === b.dataset.id));
  }));

  /* ---- İnceleme penceresi: durum güncelle / sil ---- */
  function showDlgMessage(text) { dlgMsg.textContent = text; dlgMsg.className = "message is-error"; dlgMsg.hidden = !text; }

  function openReview(d) {
    if (!d) return;
    current = d;
    $("dlg-info").innerHTML = "<dt>Öğrenci</dt><dd>" + esc(d.studentName) + "</dd><dt>Dosya</dt><dd>" + esc(d.fileName) + "</dd>" +
      FIELDS.map(([k, label]) => "<dt>" + esc(label) + "</dt><dd>" + esc((d.data || {})[k]) + "</dd>").join("");
    $("dlg-cmp").innerHTML = cmpHtml(d.comparison);

    // Sadece akışa uygun işlemler listelenir.
    const next = NEXT_STATUSES[d.status] || [];
    statusSel.innerHTML = '<option value="">Şu an: ' + esc(STATUS_LABEL[d.status] || d.status) + "</option>" +
      next.map(s => '<option value="' + s + '">' + esc(ACTION_LABEL[s]) + "</option>").join("");
    statusSel.disabled = next.length === 0;
    $("dlg-save").disabled = next.length === 0;

    showDlgMessage(next.length ? "" : (d.status === "faulty"
      ? "Öğrencinin belgeyi düzeltip yeniden yüklemesi bekleniyor."
      : "Bu belgede durum değişikliği yapılamaz."));
    if (!next.length) dlgMsg.className = "message";
    dlg.showModal();
  }

  $("dlg-close").addEventListener("click", () => dlg.close());

  $("dlg-file").addEventListener("click", async () => {
    try { await api.openDocumentFile(current.id, current.fileName); } catch (err) { showDlgMessage(err.message); }
  });

  $("dlg-save").addEventListener("click", async () => {
    const status = statusSel.value;
    if (!status) return showDlgMessage("Bir işlem seç.");
    let reason;
    if (status === "faulty") {
      reason = prompt("Hatalı / Eksik sebebi (öğrenci bu notu görecek):");
      if (reason === null) return; // vazgeçti
    }
    try {
      await api.updateDocumentStatus(current.id, status, reason);
      dlg.close(); toast("Durum güncellendi."); loadDocs();
    } catch (err) { showDlgMessage(err.message); }
  });

  $("dlg-delete").addEventListener("click", async () => {
    if (!confirm("Bu belge kalıcı olarak silinecek. Emin misin?")) return;
    try {
      await api.deleteDocument(current.id);
      dlg.close(); toast("Belge silindi."); loadDocs();
    } catch (err) { showDlgMessage(err.message); }
  });

  /* ---- Test veri seti ---- */
  $("data-head").innerHTML = "<tr>" + FIELDS.slice(0, 12).map(f => "<th>" + esc(f[1]) + "</th>").join("") + "</tr>";

  async function loadDataset() {
    try {
      const rows = await api.getDataset();
      $("data-rows").innerHTML = rows.length
        ? rows.map(r => "<tr>" + DATASET_KEYS.map(k => "<td>" + esc(r[k]) + "</td>").join("") + "</tr>").join("")
        : '<tr><td colspan="12" class="empty">Veri seti yüklenmedi.</td></tr>';
    } catch (err) { toast(err.message, "error"); }
  }

  $("dataset-btn").addEventListener("click", async () => {
    const file = $("dataset-file").files[0];
    if (!file) return toast("Bir dosya seç.", "error");
    if (!DATASET_EXT.includes(extOf(file.name))) return toast("Sadece CSV veya Excel (.xlsx) yükleyebilirsin.", "error");
    try {
      const res = await api.uploadDataset(file);
      toast(res.added + " kayıt eklendi, " + res.updated + " kayıt güncellendi.");
      // Atlanan satırlar sebebiyle gösterilir (ilk 5).
      if (res.errorCount) {
        const list = res.errors.slice(0, 5).map(e => "Satır " + e.rowNumber + ": " + e.reason).join(" ");
        toast(res.errorCount + " satır alınmadı. " + list + (res.errorCount > 5 ? " ..." : ""), "error");
      }
      loadDataset();
    } catch (err) { toast(err.message, "error"); }
  });

  loadDocs();
  loadDataset();
})();