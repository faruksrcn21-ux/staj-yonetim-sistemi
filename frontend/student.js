/* Öğrenci paneli - sadece student.html'de çalışır (app.js önce yüklenmeli) */
(function () {
  const user = Session.require([ROLES.STUDENT]);
  if (!user) return;

  const MAX_SIZE = 5 * 1024 * 1024; // 5 MB

  const listEl = document.getElementById("app-list");
  const emptyEl = document.getElementById("empty");
  const form = document.getElementById("app-form");
  const fileInput = document.getElementById("files");
  const fileList = document.getElementById("file-list");
  const msg = document.getElementById("form-message");
  const submitBtn = document.getElementById("submit-btn");

  document.getElementById("user-name").textContent = user.username;
  document.getElementById("logout-btn").addEventListener("click", () => Session.logout());

  function showMessage(text) {
    msg.textContent = text;
    msg.className = "message is-error";
    msg.hidden = !text;
  }

  /* ---- Başvuru listesi ---- */
  function renderApps(apps) {
    emptyEl.hidden = apps.length > 0;
    listEl.innerHTML = apps
      .slice()
      .reverse() // en yeni üstte
      .map(a => {
        const docs = a.documents.length
          ? '<ul class="docs">' + a.documents.map(d => "<li>" + esc(d) + "</li>").join("") + "</ul>"
          : "";
        const note = a.feedback
          ? '<p class="note"><strong>Komisyon notu:</strong> ' + esc(a.feedback) + "</p>"
          : "";
        return (
          '<li class="app-item is-' + esc(a.status) + '">' +
            "<header><h3>" + esc(a.company) + "</h3>" + statusBadge(a.status) + "</header>" +
            '<div class="muted">' + fmtDate(a.startDate) + " - " + fmtDate(a.endDate) + "</div>" +
            docs + note +
          "</li>"
        );
      })
      .join("");
  }

  async function loadApps() {
    try {
      renderApps(await api.getMyApplications());
    } catch (err) {
      toast(err.message, "error");
    }
  }

  /* ---- Dosya seçimi ---- */
  function validFiles() {
    const files = Array.from(fileInput.files);
    for (const f of files) {
      const isPdf = f.type === "application/pdf" || f.name.toLowerCase().endsWith(".pdf");
      if (!isPdf) return { error: f.name + " PDF değil. Sadece PDF yükleyebilirsin." };
      if (f.size > MAX_SIZE) return { error: f.name + " 5 MB sınırını aşıyor." };
    }
    return { files };
  }

  fileInput.addEventListener("change", () => {
    const res = validFiles();
    if (res.error) {
      showMessage(res.error);
      fileInput.value = "";
      fileList.innerHTML = "";
      return;
    }
    showMessage("");
    fileList.innerHTML = res.files
      .map(f => "<li>" + esc(f.name) + " (" + Math.max(1, Math.round(f.size / 1024)) + " KB)</li>")
      .join("");
  });

  /* ---- Başvuru gönderme ---- */
  form.addEventListener("submit", async (e) => {
    e.preventDefault();
    const company = form.elements.company.value.trim();
    const startDate = form.elements.startDate.value;
    const endDate = form.elements.endDate.value;

    if (company.length < 2) return showMessage("Firma adını gir.");
    if (!startDate || !endDate) return showMessage("Başlangıç ve bitiş tarihini seç.");
    if (endDate <= startDate) return showMessage("Bitiş tarihi başlangıçtan sonra olmalı.");

    const res = validFiles();
    if (res.error) return showMessage(res.error);
    if (res.files.length === 0) return showMessage("En az bir PDF belge yükle.");

    showMessage("");
    submitBtn.disabled = true;
    submitBtn.textContent = "Gönderiliyor...";
    try {
      await api.submitApplication({ company, startDate, endDate, files: res.files });
      form.reset();
      fileList.innerHTML = "";
      toast("Başvurun gönderildi. Durum: Beklemede.");
      await loadApps();
    } catch (err) {
      showMessage(err.message);
    } finally {
      submitBtn.disabled = false;
      submitBtn.textContent = "Başvuruyu gönder";
    }
  });

  loadApps();
})();