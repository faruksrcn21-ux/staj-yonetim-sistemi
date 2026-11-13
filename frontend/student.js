/* Öğrenci ekranı - sadece student.html'de çalışır (app.js önce yüklenmeli) */
(function () {
  const user = Session.require(ROLE_STUDENT);
  if (!user) return;

  const $ = id => document.getElementById(id);
  const list = $("doc-list"), empty = $("empty"), dlg = $("upload-dialog");
  const form = $("doc-form"), fileInput = $("file"), msg = $("form-message"), submitBtn = $("submit-btn");
  let replaceId = null;

  $("user-name").textContent = user.fullName;
  $("logout-btn").addEventListener("click", () => Session.logout());

  // Ek-16 verilerini elle girme formu (alanlar app.js'teki FIELDS'tan gelir)
  $("fields").innerHTML = FIELDS.map(([k, label, type]) =>
    '<div class="field"><label for="f_' + k + '">' + esc(label) + '</label><input id="f_' + k + '" name="' + k + '" type="' + type + '"></div>'
  ).join("");

  function showMessage(text) { msg.textContent = text; msg.className = "message is-error"; msg.hidden = !text; }

  function openForm(doc) {
    form.reset(); showMessage("");
    replaceId = doc ? doc.id : null;
    $("dlg-title").textContent = doc ? "Düzelt ve yeniden yükle" : "Evrak yükle";
    const base = doc ? doc.data : { adSoyad: user.fullName, ogrenciNo: user.studentNo || "", email: user.email };
    FIELDS.forEach(([k]) => { $("f_" + k).value = base[k] || ""; });
    dlg.showModal();
  }

  function render(docs) {
    empty.hidden = docs.length > 0;
    list.innerHTML = docs.slice().reverse().map(d => {
      const actions = d.status === "completed"
        ? '<p class="note">Bu belge tamamlandı, değiştirilemez ve silinemez.</p>'
        : '<div class="row-actions"><button type="button" class="btn" data-act="redo" data-id="' + d.id + '">Düzelt ve yeniden yükle</button>' +
          '<button type="button" class="btn" data-act="del" data-id="' + d.id + '">Sil</button></div>';
      return '<li class="app-item"><header><h3>' + esc(d.fileName) + "</h3>" + statusBadge(d.status) + "</header>" +
        '<div class="muted">Yüklenme: ' + fmtDateTime(d.uploadedAt) + "</div>" + cmpHtml(d.comparison) + actions + "</li>";
    }).join("");
  }

  async function load() {
    try { render(await api.getMyDocuments()); } catch (err) { toast(err.message, "error"); }
  }

  $("upload-btn").addEventListener("click", () => openForm(null));
  $("cancel-btn").addEventListener("click", () => dlg.close());

  list.addEventListener("click", async (e) => {
    const b = e.target.closest("button[data-act]");
    if (!b) return;
    const id = Number(b.dataset.id);
    if (b.dataset.act === "redo") {
      const docs = await api.getMyDocuments();
      return openForm(docs.find(d => d.id === id));
    }
    if (!confirm("Bu belgeyi silmek istediğine emin misin?")) return;
    try { await api.deleteDocument(id); toast("Belge silindi."); load(); } catch (err) { toast(err.message, "error"); }
  });

  form.addEventListener("submit", async (e) => {
    e.preventDefault();
    const data = {};
    FIELDS.forEach(([k]) => { data[k] = $("f_" + k).value.trim(); });
    const missing = FIELDS.filter(([k]) => !data[k]).map(f => f[1]);
    if (missing.length) return showMessage("Şu alanları doldur: " + missing.join(", "));

    const file = fileInput.files[0];
    if (!file) return showMessage("Bir belge seç.");
    if (!ALLOWED_EXT.includes(extOf(file.name))) {
      fileInput.value = "";
      return showMessage("Desteklenmeyen format. Sadece .doc, .docx, .pdf, .jpeg ve .png yükleyebilirsin.");
    }

    showMessage(""); submitBtn.disabled = true; submitBtn.textContent = "Yükleniyor...";
    try {
      const doc = await api.uploadDocument({ data, file, replaceId });
      dlg.close();
      toast(doc.comparison && doc.comparison.ready ? "Evrak yüklendi, teslime hazır." : "Evrak yüklendi, uyuşmayan alanlar var.");
      load();
    } catch (err) {
      showMessage(err.message);
    } finally {
      submitBtn.disabled = false; submitBtn.textContent = "Yükle";
    }
  });

  load();
})();