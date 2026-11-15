   /* Ortak kod: sabitler, sahte veri, API çağrıları, yardımcılar, oturum, giriş sayfası */

/* ---------- 1) CONFIG + sabitler ---------- */
const CONFIG = {
  USE_MOCK: false, // true: sahte veri (backend olmadan), false: gerçek backend
  API_BASE: "http://localhost:5062/api",  // backend: backend/StajEvrak.Api -> "dotnet run"
  MOCK_DELAY: 300
};

const ROLE_STUDENT = "ROLE_STUDENT";
const ROLE_ADMIN = "ROLE_ADMIN";
const ROLE_HOME = { [ROLE_STUDENT]: "student.html", [ROLE_ADMIN]: "admin.html" };

const ALLOWED_EXT = ["doc", "docx", "pdf", "jpeg", "jpg", "png"];
const DATASET_EXT = ["csv", "xlsx"];

// Belge akışı: Yüklendi > İnceleniyor > Hatalı/Eksik > Onaylandı > Tamamlandı
const STATUSES = ["uploaded", "reviewing", "faulty", "approved", "completed"];
const STATUS_LABEL = {
  uploaded: "Yüklendi", reviewing: "İnceleniyor", faulty: "Hatalı / Eksik",
  approved: "Onaylandı", completed: "Tamamlandı"
};

// Backend statü kodu -> arayüzdeki anahtar
const STATUS_FROM_API = {
  YUKLENDI: "uploaded", INCELENIYOR: "reviewing", HATALI_EKSIK: "faulty",
  ONAYLANDI: "approved", TAMAMLANDI: "completed"
};

// Admin'in yapabileceği statü geçişleri (backend kuralıyla aynı).
const NEXT_STATUSES = {
  uploaded: ["reviewing"],
  reviewing: ["completed", "faulty"],
  faulty: [], approved: [], completed: ["faulty"]
};
const ACTION_LABEL = { reviewing: "İncelemeye al", completed: "Onayla (Tamamlandı)", faulty: "Hatalı / Eksik olarak işaretle" };

// Ek-16'daki "Sağlık yardım statüsü" seçenekleri (25 yaş ve üstü öğrenciler 6. seçeneği işaretler)
const SAGLIK_SECENEKLERI = [
  "Genel Sağlık sigortası kapsamındayım",
  "Anne-Baba üzerinden sağlık yardımı almaktayım",
  "Sosyal Sigortalar Kurumu'na tabi olarak çalıştığımdan kendi sağlık yardımım var",
  "Bağ-Kur'a tabi olarak çalıştığımdan kendi sağlık yardımım var",
  "Emekli Sandığı'na tabi olarak çalıştığımdan kendi sağlık yardımım var",
  "Herhangi bir sosyal güvencem olmadığından sağlık yardımı alamıyorum"
];

// [anahtar, etiket, input tipi, örnek]. İlk 12 alan test veri setinin sütunlarıdır.
// Etiketler Ek-16'daki yazılışlara göredir; öğrenci belgesine yazdığını aynen girer.
const FIELDS = [
  ["adSoyad", "Adı soyadı", "text"], ["tcNo", "T.C. kimlik no", "text", "Yabancıysa 99 ile başlar"],
  ["uyruk", "Uyruğu", "text", "T.C. veya ülke adı"], ["ogrenciNo", "Öğrenci numarası", "text"],
  ["dogumTarihi", "Doğum tarihi", "date"], ["adres", "İkametgâh adresi", "text", "Mah., sokak, no, daire, ilçe/il"],
  ["bolum", "Bölümü", "text", "Örn. Yazılım Mühendisliği"], ["teslimYili", "Öğretim yılı", "text", "Örn. 2025-2026"],
  ["eskiStajGun", "Daha önce yaptığı staj gün sayısı", "number", "Örn. 0"], ["email", "E-posta adresi", "email"],
  ["telefon", "Telefon no", "tel", "+90 5xx xxx xx xx"], ["saglikYardimi", "Sağlık yardım statüsü", "select"],
  ["stajBaslangic", "Staj başlangıç tarihi", "date"], ["stajBitis", "Staj bitiş tarihi", "date"]
];
const DATASET_KEYS = FIELDS.slice(0, 12).map(f => f[0]);
const fieldLabel = key => (FIELDS.find(f => f[0] === key) || [key, key])[1];

/* ---------- 2) MockDB (localStorage) ---------- */
const MOCK_SEED = {
  users: [
    { id: 1, role: ROLE_STUDENT, username: "ogrenci", email: "ogrenci@kocaeli.edu.tr", password: "123456" },
    { id: 2, role: ROLE_ADMIN, username: "admin", email: "admin@kocaeli.edu.tr", password: "123456" }
  ],
  dataset: [
    { adSoyad: "Ayşe Yılmaz", tcNo: "11111111111", uyruk: "T.C.", ogrenciNo: "210201001", dogumTarihi: "2003-04-12", adres: "İzmit / Kocaeli", bolum: "Yazılım Mühendisliği", teslimYili: "2025-2026", eskiStajGun: "0", email: "ogrenci@kocaeli.edu.tr", telefon: "+90 555 000 00 01", saglikYardimi: "Anne-Baba üzerinden sağlık yardımı almaktayım" },
    { adSoyad: "Can Demir", tcNo: "22222222222", uyruk: "T.C.", ogrenciNo: "210201014", dogumTarihi: "2002-09-30", adres: "Gebze / Kocaeli", bolum: "Bilgisayar Mühendisliği", teslimYili: "2025-2026", eskiStajGun: "20", email: "can@kocaeli.edu.tr", telefon: "+90 555 000 00 02", saglikYardimi: "Genel Sağlık sigortası kapsamındayım" }
  ],
  documents: [
    { id: 101, userId: 9, studentName: "Can Demir", studentNo: "210201014", fileName: "ek16_can.pdf", uploadedAt: "2026-11-10T09:30:00", status: "completed",
      data: { adSoyad: "Can Demir", tcNo: "22222222222", uyruk: "T.C.", ogrenciNo: "210201014", dogumTarihi: "2002-09-30", adres: "Gebze / Kocaeli", bolum: "Bilgisayar Mühendisliği", teslimYili: "2025-2026", eskiStajGun: "20", email: "can@kocaeli.edu.tr", telefon: "+90 555 000 00 02", saglikYardimi: "Genel Sağlık sigortası kapsamındayım", stajBaslangic: "", stajBitis: "" },
      comparison: { ready: true, note: "", mismatches: [] } }
  ],
  failed: {}, locked: {}
};

const MockDB = {
  KEY: "staj_evrak_mock_db_v2",
  load() {
    try { const raw = localStorage.getItem(this.KEY); if (raw) return JSON.parse(raw); } catch (e) { /* sıfırla */ }
    return JSON.parse(JSON.stringify(MOCK_SEED));
  },
  save(db) { localStorage.setItem(this.KEY, JSON.stringify(db)); },
  reset() { localStorage.removeItem(this.KEY); }
};

const wait = (ms = CONFIG.MOCK_DELAY) => new Promise(r => setTimeout(r, ms));
const norm = v => String(v == null ? "" : v).trim().toLocaleLowerCase("tr");
const extOf = name => (name.split(".").pop() || "").toLowerCase();

// Sadece sahte veri modunda; gerçekte karşılaştırmayı backend yapar.
function mockCompare(entered, extracted) {
  if (!extracted) return { ready: false, note: "Test veri setinde bu öğrenciye ait kayıt yok.", mismatches: [] };
  const mismatches = DATASET_KEYS
    .filter(k => norm(entered[k]) !== norm(extracted[k]))
    .map(k => ({ field: k, entered: entered[k], found: extracted[k], source: "veri seti" }));
  return { ready: mismatches.length === 0, note: "", mismatches };
}

/* ---------- 3) api: tüm sunucu çağrıları ---------- */
async function http(path, { method = "GET", body, isForm = false } = {}) {
  const headers = {};
  const token = Session.token();
  if (token) headers["Authorization"] = "Bearer " + token;
  if (body && !isForm) headers["Content-Type"] = "application/json";
  const res = await fetch(CONFIG.API_BASE + path, { method, headers, body: body ? (isForm ? body : JSON.stringify(body)) : undefined });
  const data = await res.json().catch(() => ({}));
  if (!res.ok) {
    // Oturum geçersizse veya rol yetmiyorsa 403 + redirectTo gelir: çıkış yapılıp login'e gidilir.
    // redirectTo yoksa sadece mesaj gösterilir.
    if (res.status === 403 && data.redirectTo) {
      Session.clear();
      window.location.href = "index.html?reason=forbidden";
    }
    throw new Error(errorText(data, res.status));
  }
  return data;
}

// Backend hata yanıtı: { message, remainingAttempts?, lockedUntil?, redirectTo? }
// 400 doğrulama hatası: { errors: { Alan: ["mesaj"] } }
function errorText(data, status) {
  if (data.errors) return Object.values(data.errors).flat().join(" ");
  let text = data.message || "Sunucu hatası (" + status + ")";
  if (data.remainingAttempts != null) text += " Kalan deneme hakkı: " + data.remainingAttempts + ".";
  if (data.lockedUntil) {
    const until = new Date(data.lockedUntil).toLocaleTimeString("tr-TR", { hour: "2-digit", minute: "2-digit" });
    text += " Tekrar deneme: " + until + ".";
  }
  return text;
}

// Backend'in belge yanıtını arayüzün kullandığı yapıya çevirir.
// comparisonReport JSON ise uyuşmayan alanlar tablo, düz metinse not olarak gösterilir.
function parseJson(text) { try { return text ? JSON.parse(text) : null; } catch (e) { return null; } }
function fromApiDoc(d) {
  const data = parseJson(d.manualFormData) || {};
  const report = parseJson(d.comparisonReport);
  return {
    id: d.id, userId: d.userId,
    studentName: data.adSoyad || d.username || "", studentNo: data.ogrenciNo || "",
    fileName: d.fileName, uploadedAt: d.uploadedAt,
    status: STATUS_FROM_API[d.status] || d.status,
    data,
    // Rapor yoksa sonuç alanı gösterilmez.
    comparison: !d.comparisonReport && !d.isReadyForSubmission ? null : {
      ready: d.isReadyForSubmission,
      // Rapor olmadan hazırsa bunu admin onayı sağlamıştır.
      byAdmin: d.isReadyForSubmission && !d.comparisonReport,
      note: report ? (report.note || "") : (d.comparisonReport || ""),
      mismatches: report && Array.isArray(report.mismatches) ? report.mismatches : []
    }
  };
}

// Backend'in veri seti kaydı -> arayüzdeki sütun anahtarları (kimlik no maskeli gelir).
function fromApiDataset(r) {
  return {
    adSoyad: r.fullName, tcNo: r.maskedTcNo, uyruk: r.nationality, ogrenciNo: r.studentNo,
    dogumTarihi: r.birthDate, adres: r.address, bolum: r.department, teslimYili: r.submissionYear,
    eskiStajGun: r.previousInternshipDays, email: r.email, telefon: r.phone, saglikYardimi: r.healthInsuranceStatus
  };
}
const maskTc = v => { const s = String(v || ""); return s.length <= 5 ? "*".repeat(s.length) : s.slice(0, 3) + "*".repeat(s.length - 5) + s.slice(-2); };

const api = {
  // POST /auth/login {identifier, password} -> {token, expiresAt, user:{id, username, email, role, dashboardView}}
  // Hatalı şifrede 401 + remainingAttempts, hesap kilitliyse 423 + lockedUntil, IP kilidinde 429.
  async login({ identifier, password }) {
    if (!CONFIG.USE_MOCK) return http("/auth/login", { method: "POST", body: { identifier, password } });
    await wait();
    const db = MockDB.load(), id = identifier.toLowerCase();
    const user = db.users.find(u => u.username.toLowerCase() === id || u.email === id);
    if (!user) throw new Error("Kullanıcı adı/e-posta veya şifre hatalı.");
    if (db.locked[user.id]) throw new Error("Hesabın 5 hatalı denemeden sonra kilitlendi.");
    if (user.password !== password) {
      db.failed[user.id] = (db.failed[user.id] || 0) + 1;
      if (db.failed[user.id] >= 5) db.locked[user.id] = true;
      MockDB.save(db);
      const left = 5 - db.failed[user.id];
      throw new Error(left > 0 ? "Kullanıcı adı/e-posta veya şifre hatalı. Kalan deneme hakkı: " + left + "." : "Hesabın 5 hatalı denemeden sonra kilitlendi.");
    }
    db.failed[user.id] = 0; MockDB.save(db);
    const { password: _pw, ...safe } = user;
    return { token: "mock-token-" + user.id, expiresAt: null, user: { ...safe, dashboardView: ROLE_HOME[user.role] } };
  },

  // POST /auth/register {username, email, password} -> 201 {id, username, email, role, dashboardView}
  // Kayıt sadece öğrenci oluşturur; admin hesabı backend'de seed ile oluşur.
  async register({ username, email, password }) {
    if (!CONFIG.USE_MOCK) return http("/auth/register", { method: "POST", body: { username, email, password } });
    await wait();
    const db = MockDB.load();
    if (db.users.some(u => u.username.toLowerCase() === username.toLowerCase())) throw new Error("Bu kullanıcı adı zaten kullanılıyor.");
    if (db.users.some(u => u.email === email.toLowerCase())) throw new Error("Bu e-posta zaten kayıtlı.");
    const user = { id: Date.now(), role: ROLE_STUDENT, username, email: email.toLowerCase(), password };
    db.users.push(user);
    MockDB.save(db);
    const { password: _pw, ...safe } = user;
    return safe;
  },

  // POST /auth/logout -> token'ı geçersiz kılar
  async logout() {
    if (!CONFIG.USE_MOCK) return http("/auth/logout", { method: "POST" });
  },

  // GET /documents (öğrenci: kendi belgeleri) -> [DocumentResponse]
  async getMyDocuments() {
    if (!CONFIG.USE_MOCK) return (await http("/documents")).map(fromApiDoc);
    await wait();
    return MockDB.load().documents.filter(d => d.userId === Session.user().id);
  },

  // Yeni evrak:   POST /documents/upload          (öğrenci)
  // Düzeltme:     POST /documents/:id/reupload
  // multipart: file, manualFormData (JSON metin) -> { document: DocumentResponse, parse: {isReadable, reason, ...} }
  // Belge tekrar "Yüklendi" olur. Format/içerik uymazsa 400 + mesaj.
  async uploadDocument({ data, file, replaceId }) {
    if (!CONFIG.USE_MOCK) {
      const fd = new FormData();
      fd.append("file", file); fd.append("manualFormData", JSON.stringify(data));
      const path = replaceId ? "/documents/" + replaceId + "/reupload" : "/documents/upload";
      const res = await http(path, { method: "POST", body: fd, isForm: true });
      return { ...fromApiDoc(res.document), parse: res.parse };
    }
    await wait();
    if (!ALLOWED_EXT.includes(extOf(file.name))) throw new Error("Desteklenmeyen dosya formatı.");
    const db = MockDB.load(), u = Session.user();
    if (replaceId) {
      const old = db.documents.find(d => d.id === replaceId && d.userId === u.id);
      if (old && old.status === "completed") throw new Error("Tamamlandı durumundaki belge değiştirilemez.");
      db.documents = db.documents.filter(d => d !== old);
    }
    // Veri setindeki kayıt e-posta ile bulunur.
    const extracted = db.dataset.find(r => norm(r.email) === norm(u.email));
    const comparison = mockCompare(data, extracted);
    const doc = { id: Date.now(), userId: u.id, studentName: data.adSoyad || u.username, studentNo: data.ogrenciNo || "", fileName: file.name,
      uploadedAt: new Date().toISOString(), status: comparison.ready ? "reviewing" : "faulty", data, comparison };
    db.documents.push(doc); MockDB.save(db);
    return doc;
  },

  // GET /documents/:id/file -> yüklenen dosyanın kendisi; yeni sekmede açılır
  async openDocumentFile(id, fileName) {
    if (CONFIG.USE_MOCK) throw new Error("Sahte veri modunda dosya saklanmaz.");
    const tab = window.open("", "_blank"); // tıklama anında açılır, yoksa tarayıcı engeller
    try {
      const res = await fetch(CONFIG.API_BASE + "/documents/" + id + "/file", { headers: { Authorization: "Bearer " + Session.token() } });
      if (!res.ok) {
        const data = await res.json().catch(() => ({}));
        if (res.status === 403 && data.redirectTo) { Session.clear(); window.location.href = "index.html?reason=forbidden"; }
        throw new Error(errorText(data, res.status));
      }
      const url = URL.createObjectURL(await res.blob());
      if (tab) { tab.location.href = url; return; }
      // Tarayıcı yeni sekmeyi engellediyse dosya indirilir.
      const a = document.createElement("a");
      a.href = url; a.download = fileName || "belge";
      document.body.appendChild(a); a.click(); a.remove();
    } catch (err) {
      if (tab) tab.close();
      throw err;
    }
  },

  // DELETE /documents/:id  (öğrenci: Tamamlandı ise 403; admin: her zaman)
  async deleteDocument(id) {
    if (!CONFIG.USE_MOCK) return http("/documents/" + id, { method: "DELETE" });
    await wait();
    const db = MockDB.load(), u = Session.user();
    const doc = db.documents.find(d => d.id === id);
    if (!doc) throw new Error("Belge bulunamadı.");
    if (u.role !== ROLE_ADMIN && (doc.userId !== u.id || doc.status === "completed")) throw new Error("Bu belgeyi silemezsin.");
    db.documents = db.documents.filter(d => d.id !== id); MockDB.save(db);
  },

  // GET /admin/documents (admin: tüm belgeler) -> [DocumentResponse]
  async getAllDocuments() {
    if (!CONFIG.USE_MOCK) return (await http("/admin/documents")).map(fromApiDoc);
    await wait();
    return MockDB.load().documents;
  },

  // Admin statü değişikliği (backend'de her geçiş ayrı endpoint):
  //   reviewing -> POST /admin/documents/:id/review          (Yüklendi -> İnceleniyor)
  //   completed -> POST /admin/documents/:id/approve         (İnceleniyor -> Onaylandı -> Tamamlandı)
  //   faulty    -> POST /admin/documents/:id/mark-defective {reason}  (İnceleniyor -> Hatalı/Eksik)
  // Akışa aykırı geçişte backend 409 döner.
  async updateDocumentStatus(id, status, reason) {
    const path = { reviewing: "review", completed: "approve", faulty: "mark-defective" }[status];
    if (!path) throw new Error("Bu durum değişikliği yapılamaz.");
    if (!CONFIG.USE_MOCK) {
      const body = status === "faulty" ? { reason } : undefined;
      return fromApiDoc(await http("/admin/documents/" + id + "/" + path, { method: "POST", body }));
    }
    await wait();
    const db = MockDB.load(), doc = db.documents.find(d => d.id === id);
    if (!doc) throw new Error("Belge bulunamadı.");
    if (!NEXT_STATUSES[doc.status].includes(status)) throw new Error("Belge '" + STATUS_LABEL[doc.status] + "' durumundayken bu işlem yapılamaz.");
    if (status === "faulty") {
      if (!reason || reason.trim().length < 3) throw new Error("Sebep en az 3 karakter olmalıdır.");
      doc.comparison = { ready: false, note: reason.trim(), mismatches: [] };
    }
    if (status === "completed") doc.comparison = { ...(doc.comparison || { note: "", mismatches: [] }), ready: true }; // onay = teslime hazır
    doc.status = status; MockDB.save(db);
    return doc;
  },

  // POST /admin/dataset/import (multipart: file .csv/.xlsx)
  //   -> { added, updated, errorCount, errors: [{ rowNumber, reason }] }
  // Kimlik no'ya göre upsert: kayıt varsa güncellenir, yoksa eklenir; hatalı satırlar atlanır.
  async uploadDataset(file) {
    if (!CONFIG.USE_MOCK) { const fd = new FormData(); fd.append("file", file); return http("/admin/dataset/import", { method: "POST", body: fd, isForm: true }); }
    await wait();
    if (extOf(file.name) !== "csv") throw new Error("Mock modda sadece CSV okunur. Excel'i backend okuyacak.");
    const lines = (await file.text()).split(/\r?\n/).filter(l => l.trim());
    if (lines.length < 2) throw new Error("Dosyada veri satırı yok.");
    const delim = lines[0].includes(";") ? ";" : ",";
    const db = MockDB.load(), result = { added: 0, updated: 0, errorCount: 0, errors: [] };
    lines.slice(1).forEach((l, i) => {
      const c = l.split(delim).map(x => x.trim()), o = {};
      DATASET_KEYS.forEach((k, j) => { o[k] = c[j] || ""; });
      if (!o.tcNo) { result.errors.push({ rowNumber: i + 2, reason: "Kimlik no boş olamaz." }); return; }
      const idx = db.dataset.findIndex(r => r.tcNo === o.tcNo);
      if (idx >= 0) { db.dataset[idx] = o; result.updated++; } else { db.dataset.push(o); result.added++; }
    });
    result.errorCount = result.errors.length;
    MockDB.save(db);
    return result;
  },

  // GET /admin/dataset -> [DatasetRecordResponse] (kimlik no maskeli)
  async getDataset() {
    if (!CONFIG.USE_MOCK) return (await http("/admin/dataset")).map(fromApiDataset);
    await wait();
    return MockDB.load().dataset.map(r => ({ ...r, tcNo: maskTc(r.tcNo) }));
  }
};

/* ---------- 4) Ortak yardımcılar ---------- */
function esc(v) { const d = document.createElement("div"); d.textContent = v == null ? "" : String(v); return d.innerHTML; }
function fmtDateTime(iso) { return iso ? new Date(iso).toLocaleString("tr-TR") : "-"; }
function statusBadge(s) { return '<span class="badge badge-' + esc(s) + '">' + esc(STATUS_LABEL[s] || s) + "</span>"; }

function toast(text, type) {
  const el = document.createElement("div");
  el.className = "toast " + (type === "error" ? "is-error" : "is-success");
  el.setAttribute("role", "status");
  el.textContent = text;
  document.body.appendChild(el);
  setTimeout(() => el.remove(), 3500);
}

// Karşılaştırma sonucu: teslime hazır mı, değilse hangi alanlar uyuşmuyor
function cmpHtml(c) {
  if (!c) return "";
  const note = c.note ? '<p class="note">' + esc(c.note) + "</p>" : "";
  if (c.ready) return '<p class="ready">' + (c.byAdmin
    ? "Teslime hazır: belge admin tarafından onaylandı."
    : "Teslime hazır: girilen bilgiler belgeyle ve veri setiyle uyuşuyor.") + "</p>" + note;
  let html = '<p class="not-ready">Teslime hazır değil.</p>' + note;
  if (c.mismatches && c.mismatches.length) {
    // Kaynak: uyuşmazlığın belgeden okunan bilgiyle mi, test veri setiyle mi olduğu
    html += '<div class="table-wrap"><table class="cmp"><thead><tr><th>Alan</th><th>Girilen</th><th>Bulunan</th><th>Kaynak</th></tr></thead><tbody>' +
      c.mismatches.map(m => "<tr><td>" + esc(m.label || fieldLabel(m.field)) + "</td><td>" + esc(m.entered) + "</td><td>" +
        esc(m.found) + "</td><td>" + esc(m.source === "belge" ? "Belge" : "Veri seti") + "</td></tr>").join("") +
      "</tbody></table></div>";
  }
  return html;
}

/* ---------- 5) Session ---------- */
const Session = {
  save({ token, user }) { sessionStorage.setItem("staj_session", JSON.stringify({ token, user })); },
  get() { try { return JSON.parse(sessionStorage.getItem("staj_session")); } catch (e) { return null; } },
  token() { const s = this.get(); return s ? s.token : null; },
  user() { const s = this.get(); return s ? s.user : null; },
  clear() { sessionStorage.removeItem("staj_session"); },
  // Rol uymazsa oturum kapatılıp login'e yönlendirilir.
  require(role) {
    const u = this.user();
    if (!u) { window.location.href = "index.html"; return null; }
    // Oturum silinmezse giriş sayfası kullanıcıyı geri gönderir ve 403 mesajı görünmez.
    if (u.role !== role) { this.clear(); window.location.href = "index.html?reason=forbidden"; return null; }
    return u;
  },
  async logout() {
    try { await api.logout(); } catch (e) { /* token zaten geçersiz olabilir */ }
    this.clear();
    window.location.href = "index.html";
  }
};

/* ---------- 6) Giriş / Kayıt sayfası ---------- */
(function initAuthPage() {
  const form = document.getElementById("auth-form");
  if (!form) return;
  const existing = Session.user();
  if (existing) { window.location.href = ROLE_HOME[existing.role]; return; }

  const title = document.getElementById("form-title");
  const btn = document.getElementById("submit-btn");
  const msg = document.getElementById("form-message");
  const tabs = document.querySelectorAll(".tab");
  let mode = "login";

  function show(text, type) {
    msg.textContent = text;
    msg.className = "message " + (type === "success" ? "is-success" : "is-error");
    msg.hidden = !text;
  }

  // data-only="login" | "register": alan sadece o modda görünür.
  function refresh() {
    const reg = mode === "register";
    form.querySelectorAll("[data-only]").forEach(el => { el.hidden = el.dataset.only !== mode; });
    title.textContent = reg ? "Öğrenci hesabı oluştur" : "Hesabına giriş yap";
    btn.textContent = reg ? "Kayıt ol" : "Giriş yap";
    form.elements.password.autocomplete = reg ? "new-password" : "current-password";
    tabs.forEach(t => { const on = t.dataset.mode === mode; t.classList.toggle("is-active", on); t.setAttribute("aria-selected", on); });
  }

  tabs.forEach(t => t.addEventListener("click", () => { mode = t.dataset.mode; refresh(); show(""); }));
  if (new URLSearchParams(location.search).get("reason") === "forbidden") {
    show("Bu sayfaya erişim yetkin yok (403). Lütfen giriş yap.", "error");
  }

  form.addEventListener("submit", async (e) => {
    e.preventDefault();
    const v = {
      identifier: form.elements.identifier.value.trim(),
      username: form.elements.username.value.trim(),
      email: form.elements.email.value.trim(),
      password: form.elements.password.value, password2: form.elements.password2.value
    };
    // Kurallar backend ile aynı
    if (mode === "login") {
      if (!v.identifier) return show("Kullanıcı adı veya e-posta gir.", "error");
      if (!v.password) return show("Şifreni gir.", "error");
    } else {
      if (v.username.length < 3 || v.username.length > 100) return show("Kullanıcı adı 3-100 karakter olmalı.", "error");
      if (!/^\S+@\S+\.\S+$/.test(v.email)) return show("Geçerli bir e-posta gir.", "error");
      if (v.password.length < 6) return show("Şifre en az 6 karakter olmalı.", "error");
      if (v.password !== v.password2) return show("Şifreler aynı değil.", "error");
    }
    btn.disabled = true; show("");
    try {
      if (mode === "login") {
        const session = await api.login(v);
        Session.save(session);
        window.location.href = ROLE_HOME[session.user.role];
      } else {
        await api.register(v);
        mode = "login"; refresh();
        form.elements.identifier.value = v.username;
        form.elements.password.value = ""; form.elements.password2.value = "";
        show("Kayıt başarılı. Şimdi giriş yapabilirsin.", "success");
      }
    } catch (err) {
      show(err.message, "error");
    } finally {
      btn.disabled = false;
    }
  });

  refresh();
})();