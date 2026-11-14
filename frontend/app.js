/* =====================================================
   Staj Evrak Doğrulama ve Yönetim Sistemi - ortak JS
   1) CONFIG + sabitler   2) MockDB   3) api   4) yardımcılar
   5) Session             6) Giriş / Kayıt sayfası (index.html)
   ===================================================== */

/* ---------- 1) CONFIG + sabitler ---------- */
const CONFIG = {
<<<<<<< HEAD
  USE_MOCK: true,                         // backend hazır olunca false yap
  API_BASE: "http://localhost:5000/api",  // ASP.NET Core adresini 1. kişiden al
  MOCK_DELAY: 300
};

const ROLE_STUDENT = "ROLE_STUDENT";
const ROLE_ADMIN = "ROLE_ADMIN";
const ROLE_HOME = { [ROLE_STUDENT]: "student.html", [ROLE_ADMIN]: "admin.html" };

const ALLOWED_EXT = ["doc", "docx", "pdf", "jpeg", "jpg", "png"]; // evrak
const DATASET_EXT = ["csv", "xlsx", "xls"];                       // admin veri seti

// Belge akışı: Yüklendi > İnceleniyor > Hatalı/Eksik > Onaylandı > Tamamlandı
const STATUSES = ["uploaded", "reviewing", "faulty", "approved", "completed"];
const STATUS_LABEL = {
  uploaded: "Yüklendi", reviewing: "İnceleniyor", faulty: "Hatalı / Eksik",
  approved: "Onaylandı", completed: "Tamamlandı"
};

// [anahtar, etiket, input tipi]. İlk 12 alan test veri setinin sütunlarıdır.
const FIELDS = [
  ["adSoyad", "Ad soyad", "text"], ["tcNo", "TC kimlik no", "text"],
  ["uyruk", "Uyruk", "text"], ["ogrenciNo", "Öğrenci no", "text"],
  ["dogumTarihi", "Doğum tarihi", "date"], ["adres", "Adres", "text"],
  ["bolum", "Bölüm", "text"], ["teslimYili", "Ek-16 teslim yılı", "text"],
  ["eskiStajGun", "Önceki staj gün sayısı", "number"], ["email", "E-posta", "email"],
  ["telefon", "Telefon no", "tel"], ["saglikYardimi", "Sağlık yardımı statüsü", "text"],
  ["stajBaslangic", "Staj başlangıç tarihi", "date"], ["stajBitis", "Staj bitiş tarihi", "date"]
];
const DATASET_KEYS = FIELDS.slice(0, 12).map(f => f[0]);
const fieldLabel = key => (FIELDS.find(f => f[0] === key) || [key, key])[1];

/* ---------- 2) MockDB (localStorage) ---------- */
const MOCK_SEED = {
  users: [
    { id: 1, role: ROLE_STUDENT, fullName: "Ayşe Yılmaz", studentNo: "210201001", email: "ogrenci@kocaeli.edu.tr", password: "123456" },
    { id: 2, role: ROLE_ADMIN, fullName: "Sistem Yöneticisi", email: "admin@kocaeli.edu.tr", password: "123456" }
=======
  USE_MOCK: true,                          // backend hazır olunca false yap
  API_BASE: "http://localhost:5062/api",   // backend: backend/StajEvrak.Api -> "dotnet run"
  MOCK_DELAY: 300                          // sahte gecikme (ms)
};

// Roller backend ile aynı (hocanın dokümanı: ROLE_STUDENT / ROLE_ADMIN).
// Rol kullanıcıdan alınmaz; girişte sunucudan (JWT) gelir.
const ROLES = { STUDENT: "ROLE_STUDENT", ADMIN: "ROLE_ADMIN" };

const ROLE_HOME = {
  [ROLES.STUDENT]: "student.html",
  [ROLES.ADMIN]: "admin.html"
};

/* ---------- 2) MOCK_DB ---------- */
// Kullanıcılar backend'deki yapıyla aynı: { id, username, email, role }
// Durum değerleri: "pending" | "approved" | "rejected"
const MOCK_SEED = {
  users: [
    { id: 1, role: ROLES.STUDENT, username: "ogrenci", email: "ogrenci@kocaeli.edu.tr", password: "123456" },
    { id: 2, role: ROLES.ADMIN,   username: "admin",   email: "admin@kocaeli.edu.tr",   password: "123456" }
>>>>>>> origin/main
  ],
  dataset: [
    { adSoyad: "Ayşe Yılmaz", tcNo: "11111111111", uyruk: "T.C.", ogrenciNo: "210201001", dogumTarihi: "2003-04-12", adres: "İzmit / Kocaeli", bolum: "Yazılım Mühendisliği", teslimYili: "2025", eskiStajGun: "0", email: "ogrenci@kocaeli.edu.tr", telefon: "5550000001", saglikYardimi: "Var" },
    { adSoyad: "Can Demir", tcNo: "22222222222", uyruk: "T.C.", ogrenciNo: "210201014", dogumTarihi: "2002-09-30", adres: "Gebze / Kocaeli", bolum: "Bilgisayar Mühendisliği", teslimYili: "2026", eskiStajGun: "20", email: "can@kocaeli.edu.tr", telefon: "5550000002", saglikYardimi: "Yok" }
  ],
  documents: [
    { id: 101, userId: 9, studentName: "Can Demir", studentNo: "210201014", fileName: "ek16_can.pdf", uploadedAt: "2026-11-10T09:30:00", status: "completed",
      data: { adSoyad: "Can Demir", tcNo: "22222222222", uyruk: "T.C.", ogrenciNo: "210201014", dogumTarihi: "2002-09-30", adres: "Gebze / Kocaeli", bolum: "Bilgisayar Mühendisliği", teslimYili: "2026", eskiStajGun: "20", email: "can@kocaeli.edu.tr", telefon: "5550000002", saglikYardimi: "Yok", stajBaslangic: "", stajBitis: "" },
      comparison: { ready: true, note: "", mismatches: [] } }
  ],
  failed: {}, locked: {}
};

const MockDB = {
<<<<<<< HEAD
  KEY: "staj_evrak_mock_db",
=======
  KEY: "staj_mock_db_v2", // v2: kullanıcı yapısı backend'e uyarlandı (eski kayıtlar okunmaz)
>>>>>>> origin/main
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

// SADECE MOCK: gerçekte belgeyi 2. kişinin parser'ı okur, karşılaştırmayı backend yapar.
function mockCompare(entered, extracted) {
  if (!extracted) return { ready: false, note: "Test veri setinde bu öğrenciye ait kayıt yok.", mismatches: [] };
  const mismatches = DATASET_KEYS
    .filter(k => norm(entered[k]) !== norm(extracted[k]))
    .map(k => ({ field: k, entered: entered[k], found: extracted[k] }));
  return { ready: mismatches.length === 0, note: "", mismatches };
}

/* ---------- 3) api: TÜM sunucu çağrıları burada ---------- */
async function http(path, { method = "GET", body, isForm = false } = {}) {
  const headers = {};
  const token = Session.token();
  if (token) headers["Authorization"] = "Bearer " + token;
  if (body && !isForm) headers["Content-Type"] = "application/json";
  const res = await fetch(CONFIG.API_BASE + path, { method, headers, body: body ? (isForm ? body : JSON.stringify(body)) : undefined });
  const data = await res.json().catch(() => ({}));
<<<<<<< HEAD
  // Yetkisiz istek: 403 -> oturumu sil, login'e yönlendir (login isteğinde değil)
  if (res.status === 403 && path !== "/auth/login") {
    Session.clear();
    window.location.href = "index.html?reason=forbidden";
    throw new Error("Bu işlem için yetkin yok (403).");
  }
  if (!res.ok) throw new Error(data.message || "Sunucu hatası (" + res.status + ")");
=======

  if (!res.ok) {
    // Backend yetkisiz isteklerde 403 + redirectTo:"/login" döner (hoca: "403 döner ve
    // login sayfasına yönlendirilir"). Oturumu temizleyip giriş sayfasına git.
    if (res.status === 403 && data.redirectTo) {
      Session.clear();
      window.location.href = "index.html";
    }
    throw new Error(errorText(data, res.status));
  }
>>>>>>> origin/main
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

const api = {
<<<<<<< HEAD
  // POST /auth/login {email,password} -> {token, user:{id,fullName,email,role}}
  // Hesap kilitliyse backend'in 423 + message döndürmesini bekliyoruz.
  async login({ email, password }) {
    if (!CONFIG.USE_MOCK) return http("/auth/login", { method: "POST", body: { email, password } });
    await wait();
    const db = MockDB.load(), mail = email.toLowerCase();
    if (db.locked[mail]) throw new Error("Hesabın 5 hatalı denemeden sonra kilitlendi.");
    const user = db.users.find(u => u.email === mail);
    if (!user) throw new Error("E-posta veya şifre hatalı.");
    if (user.password !== password) {
      db.failed[mail] = (db.failed[mail] || 0) + 1;
      if (db.failed[mail] >= 5) db.locked[mail] = true;
      MockDB.save(db);
      const left = 5 - db.failed[mail];
      throw new Error(left > 0 ? "E-posta veya şifre hatalı. Kalan deneme: " + left : "Hesabın 5 hatalı denemeden sonra kilitlendi.");
    }
    db.failed[mail] = 0; MockDB.save(db);
    const { password: _pw, ...safe } = user;
    return { token: "mock-token-" + user.id, user: safe };
  },

  // POST /auth/register {fullName,studentNo,email,password} -> {message}
  async register({ fullName, studentNo, email, password }) {
    if (!CONFIG.USE_MOCK) return http("/auth/register", { method: "POST", body: { fullName, studentNo, email, password } });
    await wait();
    const db = MockDB.load();
    if (db.users.some(u => u.email === email.toLowerCase())) throw new Error("Bu e-posta ile zaten bir hesap var.");
    db.users.push({ id: Date.now(), role: ROLE_STUDENT, fullName, studentNo, email: email.toLowerCase(), password });
=======
  // POST /auth/login  {identifier, password} -> {token, expiresAt, user:{id, username, email, role, dashboardView}}
  // identifier: kullanıcı adı veya e-posta (hoca: "Kullanıcı adı / e-posta + şifre")
  async login({ identifier, password }) {
    if (!CONFIG.USE_MOCK) return http("/auth/login", { method: "POST", body: { identifier, password } });
    await wait();
    const id = identifier.trim().toLowerCase();
    const user = MockDB.load().users.find(u =>
      (u.username.toLowerCase() === id || u.email === id) && u.password === password);
    if (!user) throw new Error("Kullanıcı adı/e-posta veya şifre hatalı.");
    const { password: _pw, ...safeUser } = user;
    return { token: "mock-token-" + user.id, expiresAt: null, user: { ...safeUser, dashboardView: ROLE_HOME[user.role] } };
  },

  // POST /auth/register  {username, email, password} -> 201 {id, username, email, role, dashboardView}
  // Kayıt sadece öğrenci oluşturur; admin hesabı backend'de seed ile oluşur.
  async register({ username, email, password }) {
    if (!CONFIG.USE_MOCK) return http("/auth/register", { method: "POST", body: { username, email, password } });
    await wait();
    const db = MockDB.load();
    if (db.users.some(u => u.username === username)) throw new Error("Bu kullanıcı adı zaten kullanılıyor.");
    if (db.users.some(u => u.email === email.toLowerCase())) throw new Error("Bu e-posta zaten kayıtlı.");
    const user = { id: Date.now(), role: ROLES.STUDENT, username, email: email.toLowerCase(), password };
    db.users.push(user);
>>>>>>> origin/main
    MockDB.save(db);
    const { password: _pw, ...safeUser } = user;
    return safeUser;
  },

  // POST /auth/logout  (Authorization: Bearer <token>) -> {message}
  // Hoca: "Çıkış işlemi token/session geçersiz kılar" -> token sunucuda iptal edilir.
  async logout() {
    if (!CONFIG.USE_MOCK) return http("/auth/logout", { method: "POST" });
    await wait(0);
    return { message: "Çıkış yapıldı." };
  },

  // POST /auth/logout -> token'ı geçersiz kılar
  async logout() {
    if (!CONFIG.USE_MOCK) return http("/auth/logout", { method: "POST" });
  },

  // GET /documents/my -> [document]
  async getMyDocuments() {
    if (!CONFIG.USE_MOCK) return http("/documents/my");
    await wait();
    return MockDB.load().documents.filter(d => d.userId === Session.user().id);
  },

  // POST /documents (multipart: file, data(JSON), replaceId?) -> document
  async uploadDocument({ data, file, replaceId }) {
    if (!CONFIG.USE_MOCK) {
      const fd = new FormData();
      fd.append("file", file); fd.append("data", JSON.stringify(data));
      if (replaceId) fd.append("replaceId", replaceId);
      return http("/documents", { method: "POST", body: fd, isForm: true });
    }
    await wait();
    if (!ALLOWED_EXT.includes(extOf(file.name))) throw new Error("Desteklenmeyen dosya formatı.");
    const db = MockDB.load(), u = Session.user();
<<<<<<< HEAD
    if (replaceId) {
      const old = db.documents.find(d => d.id === replaceId && d.userId === u.id);
      if (old && old.status === "completed") throw new Error("Tamamlandı durumundaki belge değiştirilemez.");
      db.documents = db.documents.filter(d => d !== old);
    }
    const extracted = db.dataset.find(r => norm(r.ogrenciNo) === norm(u.studentNo)); // belgeden okunan (simülasyon)
    const comparison = mockCompare(data, extracted);
    const doc = { id: Date.now(), userId: u.id, studentName: u.fullName, studentNo: u.studentNo || "", fileName: file.name,
      uploadedAt: new Date().toISOString(), status: comparison.ready ? "reviewing" : "faulty", data, comparison };
    db.documents.push(doc); MockDB.save(db);
    return doc;
=======
    const app = { id: Date.now(), studentId: u.id, studentName: u.username, studentNo: u.studentNo || "", company, startDate, endDate, status: "pending", feedback: "", documents: files.map(f => f.name) };
    db.applications.push(app);
    MockDB.save(db);
    return app;
>>>>>>> origin/main
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

  // GET /documents  (admin) -> [document]
  async getAllDocuments() {
    if (!CONFIG.USE_MOCK) return http("/documents");
    await wait();
    return MockDB.load().documents;
  },

  // PATCH /documents/:id/status {status} (admin)
  async updateDocumentStatus(id, status) {
    if (!CONFIG.USE_MOCK) return http("/documents/" + id + "/status", { method: "PATCH", body: { status } });
    await wait();
    const db = MockDB.load(), doc = db.documents.find(d => d.id === id);
    if (!doc) throw new Error("Belge bulunamadı.");
    doc.status = status; MockDB.save(db);
    return doc;
  },

  // POST /admin/dataset (multipart: file) -> {imported}
  async uploadDataset(file) {
    if (!CONFIG.USE_MOCK) { const fd = new FormData(); fd.append("file", file); return http("/admin/dataset", { method: "POST", body: fd, isForm: true }); }
    await wait();
    if (extOf(file.name) !== "csv") throw new Error("Mock modda sadece CSV okunur. Excel'i backend okuyacak.");
    const lines = (await file.text()).split(/\r?\n/).filter(l => l.trim());
    if (lines.length < 2) throw new Error("Dosyada veri satırı yok.");
    const delim = lines[0].includes(";") ? ";" : ",";
    const rows = lines.slice(1).map(l => {
      const c = l.split(delim).map(x => x.trim()), o = {};
      DATASET_KEYS.forEach((k, i) => { o[k] = c[i] || ""; });
      return o;
    });
    const db = MockDB.load(); db.dataset = rows; MockDB.save(db);
    return { imported: rows.length };
  },

  // GET /admin/dataset -> [row]
  async getDataset() {
    if (!CONFIG.USE_MOCK) return http("/admin/dataset");
    await wait();
    return MockDB.load().dataset;
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
  if (c.ready) return '<p class="ready">Teslime hazır: belgedeki bilgiler girdiklerinle uyuşuyor.</p>';
  let html = '<p class="not-ready">Teslime hazır değil.' + (c.note ? " " + esc(c.note) : "") + "</p>";
  if (c.mismatches && c.mismatches.length) {
    html += '<div class="table-wrap"><table class="cmp"><thead><tr><th>Alan</th><th>Girilen</th><th>Belgeden okunan</th></tr></thead><tbody>' +
      c.mismatches.map(m => "<tr><td>" + esc(fieldLabel(m.field)) + "</td><td>" + esc(m.entered) + "</td><td>" + esc(m.found) + "</td></tr>").join("") +
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
<<<<<<< HEAD
  // Sayfa başında: Session.require(ROLE_ADMIN). Rol uymazsa login'e (403 gibi) yönlendirir.
  require(role) {
=======
  // Dashboard sayfalarının başında çağır: Session.require([ROLES.STUDENT])
  require(roles) {
>>>>>>> origin/main
    const u = this.user();
    if (!u) { window.location.href = "index.html"; return null; }
    if (u.role !== role) { window.location.href = "index.html?reason=forbidden"; return null; }
    return u;
  },
<<<<<<< HEAD
=======
  // Önce sunucuda token iptal edilir, sonra tarayıcıdaki oturum silinir.
  // Sunucuya ulaşılamasa bile kullanıcı çıkış yapmış sayılır.
>>>>>>> origin/main
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

<<<<<<< HEAD
  function show(text, type) {
=======
  function showMessage(text, type) {
>>>>>>> origin/main
    msg.textContent = text;
    msg.className = "message " + (type === "success" ? "is-success" : "is-error");
    msg.hidden = !text;
  }

<<<<<<< HEAD
  function refresh() {
    const reg = mode === "register";
    form.querySelectorAll("[data-only]").forEach(el => { el.hidden = !reg; });
    title.textContent = reg ? "Öğrenci hesabı oluştur" : "Hesabına giriş yap";
    btn.textContent = reg ? "Kayıt ol" : "Giriş yap";
    form.elements.password.autocomplete = reg ? "new-password" : "current-password";
    tabs.forEach(t => { const on = t.dataset.mode === mode; t.classList.toggle("is-active", on); t.setAttribute("aria-selected", on); });
  }

  tabs.forEach(t => t.addEventListener("click", () => { mode = t.dataset.mode; refresh(); show(""); }));
  if (new URLSearchParams(location.search).get("reason") === "forbidden") {
    show("Bu sayfaya erişim yetkin yok (403). Lütfen giriş yap.", "error");
=======
  // Moda göre alanları göster/gizle (data-only="login" | "register")
  // Giriş : kullanıcı adı veya e-posta + şifre
  // Kayıt : kullanıcı adı + e-posta + şifre (sadece öğrenci; rol seçilmez)
  function refresh() {
    const isRegister = mode === "register";
    form.querySelectorAll("[data-only]").forEach(el => {
      el.hidden = el.dataset.only !== mode;
    });
    title.textContent = isRegister ? "Öğrenci hesabı oluştur" : "Hesabına giriş yap";
    submitBtn.textContent = isRegister ? "Kayıt ol" : "Giriş yap";
    passwordInput.autocomplete = isRegister ? "new-password" : "current-password";

    tabs.forEach(t => {
      const active = t.dataset.mode === mode;
      t.classList.toggle("is-active", active);
      t.setAttribute("aria-selected", active);
    });
  }

  tabs.forEach(t => t.addEventListener("click", () => { mode = t.dataset.mode; showMessage(""); refresh(); }));

  // Kurallar backend ile aynı (Dtos/AuthDtos.cs)
  function validate(v) {
    if (mode === "login") {
      if (!v.identifier) return "Kullanıcı adı veya e-posta gir.";
      if (!v.password) return "Şifreni gir.";
      return "";
    }
    if (v.username.length < 3 || v.username.length > 100) return "Kullanıcı adı 3-100 karakter olmalı.";
    if (!/^\S+@\S+\.\S+$/.test(v.email)) return "Geçerli bir e-posta gir.";
    if (v.password.length < 6) return "Şifre en az 6 karakter olmalı.";
    return "";
>>>>>>> origin/main
  }

  form.addEventListener("submit", async (e) => {
    e.preventDefault();
    const v = {
<<<<<<< HEAD
      fullName: form.elements.fullName.value.trim(), studentNo: form.elements.studentNo.value.trim(),
      email: form.elements.email.value.trim(), password: form.elements.password.value, password2: form.elements.password2.value
=======
      identifier: form.elements.identifier.value.trim(),
      username: form.elements.username.value.trim(),
      email: form.elements.email.value.trim(),
      password: form.elements.password.value
>>>>>>> origin/main
    };
    if (!/^\S+@\S+\.\S+$/.test(v.email)) return show("Geçerli bir e-posta gir.", "error");
    if (v.password.length < 6) return show("Şifre en az 6 karakter olmalı.", "error");
    if (mode === "register") {
      if (v.fullName.length < 3) return show("Ad soyadını gir.", "error");
      if (!v.studentNo) return show("Öğrenci numaranı gir.", "error");
      if (v.password !== v.password2) return show("Şifreler aynı değil.", "error");
    }
    btn.disabled = true; show("");
    try {
      if (mode === "login") {
        const session = await api.login(v);
        Session.save(session);
        window.location.href = ROLE_HOME[session.user.role];
      } else {
<<<<<<< HEAD
        const res = await api.register(v);
        mode = "login"; refresh(); show(res.message, "success");
=======
        await api.register(v);
        mode = "login";
        refresh();
        form.elements.identifier.value = v.username; // kolaylık: yeni kullanıcı adını giriş alanına yaz
        form.elements.password.value = "";
        showMessage("Kayıt başarılı. Şimdi giriş yapabilirsin.", "success");
>>>>>>> origin/main
      }
    } catch (err) {
      show(err.message, "error");
    } finally {
<<<<<<< HEAD
      btn.disabled = false;
=======
      submitBtn.textContent = mode === "register" ? "Kayıt ol" : "Giriş yap";
      submitBtn.disabled = false;
>>>>>>> origin/main
    }
  });

  refresh();
})();