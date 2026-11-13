/* =====================================================
   Staj Takip ve Yönetim Sistemi - ortak JS
   Bölümler:
   1) CONFIG        -> mock/gerçek API geçişi
   2) MOCK_DB       -> sahte veri (backend gelene kadar)
   3) api           -> TÜM sunucu çağrıları burada
   4) Session       -> oturum yardımcıları
   5) Giriş sayfası -> sadece index.html'de çalışır
   ===================================================== */

/* ---------- 1) CONFIG ---------- */
const CONFIG = {
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
  ],
  applications: [
    { id: 1, studentId: 1, studentName: "Ayşe Yılmaz", studentNo: "210201001", company: "Tofaş Ar-Ge", startDate: "2026-07-06", endDate: "2026-08-14", status: "pending", feedback: "", documents: ["staj_basvuru_formu.pdf"] },
    { id: 2, studentId: 4, studentName: "Can Demir", studentNo: "210201014", company: "Ford Otosan", startDate: "2026-07-01", endDate: "2026-08-12", status: "approved", feedback: "Belgeler eksiksiz.", documents: ["basvuru.pdf", "sgk_giris.pdf"] },
    { id: 3, studentId: 5, studentName: "Elif Şahin", studentNo: "210201027", company: "Arçelik", startDate: "2026-07-13", endDate: "2026-08-21", status: "rejected", feedback: "SGK belgesi eksik, tekrar yükleyin.", documents: ["basvuru.pdf"] }
  ]
};

// localStorage'da saklanır; sayfa yenilense de veri kaybolmaz
const MockDB = {
  KEY: "staj_mock_db_v2", // v2: kullanıcı yapısı backend'e uyarlandı (eski kayıtlar okunmaz)
  load() {
    try {
      const raw = localStorage.getItem(this.KEY);
      if (raw) return JSON.parse(raw);
    } catch (e) { /* bozuk veri: sıfırla */ }
    return JSON.parse(JSON.stringify(MOCK_SEED));
  },
  save(db) { localStorage.setItem(this.KEY, JSON.stringify(db)); },
  reset() { localStorage.removeItem(this.KEY); }
};

const wait = (ms = CONFIG.MOCK_DELAY) => new Promise(r => setTimeout(r, ms));

/* ---------- 3) api ----------
   Her fonksiyonun altında beklenen gerçek endpoint yazıyor.
   Backend hazır olunca CONFIG.USE_MOCK = false yapmak yeterli;
   endpoint/alan adları farklıysa sadece http() çağrılarını düzenle. */
async function http(path, { method = "GET", body, isForm = false } = {}) {
  const headers = {};
  const token = Session.token();
  if (token) headers["Authorization"] = "Bearer " + token;
  if (body && !isForm) headers["Content-Type"] = "application/json";

  const res = await fetch(CONFIG.API_BASE + path, {
    method,
    headers,
    body: body ? (isForm ? body : JSON.stringify(body)) : undefined
  });
  const data = await res.json().catch(() => ({}));

  if (!res.ok) {
    // Backend yetkisiz isteklerde 403 + redirectTo:"/login" döner (hoca: "403 döner ve
    // login sayfasına yönlendirilir"). Oturumu temizleyip giriş sayfasına git.
    if (res.status === 403 && data.redirectTo) {
      Session.clear();
      window.location.href = "index.html";
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

const api = {
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

  // GET /applications/me -> [application]
  async getMyApplications() {
    if (!CONFIG.USE_MOCK) return http("/applications/me");
    await wait();
    return MockDB.load().applications.filter(a => a.studentId === Session.user().id);
  },

  // POST /applications  (FormData: company, startDate, endDate, files) -> application
  async submitApplication({ company, startDate, endDate, files = [] }) {
    if (!CONFIG.USE_MOCK) {
      const fd = new FormData();
      fd.append("company", company); fd.append("startDate", startDate); fd.append("endDate", endDate);
      files.forEach(f => fd.append("files", f));
      return http("/applications", { method: "POST", body: fd, isForm: true });
    }
    await wait();
    const db = MockDB.load(), u = Session.user();
    const app = { id: Date.now(), studentId: u.id, studentName: u.username, studentNo: u.studentNo || "", company, startDate, endDate, status: "pending", feedback: "", documents: files.map(f => f.name) };
    db.applications.push(app);
    MockDB.save(db);
    return app;
  },

  // GET /applications -> [application]   (komisyon / yönetici)
  async getAllApplications() {
    if (!CONFIG.USE_MOCK) return http("/applications");
    await wait();
    return MockDB.load().applications;
  },

  // PATCH /applications/:id/review  {status, feedback} -> application
  async reviewApplication(id, { status, feedback }) {
    if (!CONFIG.USE_MOCK) return http("/applications/" + id + "/review", { method: "PATCH", body: { status, feedback } });
    await wait();
    const db = MockDB.load();
    const app = db.applications.find(a => a.id === id);
    if (!app) throw new Error("Başvuru bulunamadı.");
    app.status = status;
    app.feedback = feedback;
    MockDB.save(db);
    return app;
  }
};

/* ---------- Ortak yardımcılar (dashboard sayfaları kullanır) ---------- */
const STATUS_LABEL = { pending: "Beklemede", approved: "Onaylandı", rejected: "Reddedildi" };

function esc(value) {
  const d = document.createElement("div");
  d.textContent = value == null ? "" : String(value);
  return d.innerHTML;
}

function fmtDate(iso) {
  if (!iso) return "-";
  const [y, m, d] = iso.split("-");
  return d + "." + m + "." + y;
}

function statusBadge(status) {
  return '<span class="badge badge-' + esc(status) + '">' + esc(STATUS_LABEL[status] || status) + "</span>";
}

function toast(text, type) {
  const el = document.createElement("div");
  el.className = "toast " + (type === "error" ? "is-error" : "is-success");
  el.setAttribute("role", "status");
  el.textContent = text;
  document.body.appendChild(el);
  setTimeout(() => el.remove(), 3500);
}

// Belge adresi. Gerçek backend'de dosya endpoint'i token isteyebilir;
// bunu takım arkadaşlarıyla netleştir (cookie ya da ?token= gibi).
function documentUrl(appId, fileName) {
  if (CONFIG.USE_MOCK) return null;
  return CONFIG.API_BASE + "/applications/" + appId + "/documents/" + encodeURIComponent(fileName);
}

/* ---------- 4) Session ---------- */
const Session = {
  save({ token, user }) { sessionStorage.setItem("staj_session", JSON.stringify({ token, user })); },
  get() { try { return JSON.parse(sessionStorage.getItem("staj_session")); } catch (e) { return null; } },
  token() { const s = this.get(); return s ? s.token : null; },
  user() { const s = this.get(); return s ? s.user : null; },
  clear() { sessionStorage.removeItem("staj_session"); },
  // Dashboard sayfalarının başında çağır: Session.require([ROLES.STUDENT])
  require(roles) {
    const u = this.user();
    if (!u || !roles.includes(u.role)) { window.location.href = "index.html"; return null; }
    return u;
  },
  // Önce sunucuda token iptal edilir, sonra tarayıcıdaki oturum silinir.
  // Sunucuya ulaşılamasa bile kullanıcı çıkış yapmış sayılır.
  async logout() {
    try { await api.logout(); } catch (e) { /* token zaten geçersiz olabilir */ }
    this.clear();
    window.location.href = "index.html";
  }
};

/* ---------- 5) Giriş / Kayıt sayfası ---------- */
(function initAuthPage() {
  const form = document.getElementById("auth-form");
  if (!form) return; // başka sayfadayız

  // Zaten girişliyse doğrudan panele gönder
  const existing = Session.user();
  if (existing) { window.location.href = ROLE_HOME[existing.role]; return; }

  const title = document.getElementById("form-title");
  const submitBtn = document.getElementById("submit-btn");
  const msg = document.getElementById("form-message");
  const tabs = document.querySelectorAll(".tab");
  const passwordInput = form.elements.password;
  let mode = "login";

  function showMessage(text, type) {
    msg.textContent = text;
    msg.className = "message " + (type === "success" ? "is-success" : "is-error");
    msg.hidden = !text;
  }

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
  }

  form.addEventListener("submit", async (e) => {
    e.preventDefault();
    const v = {
      identifier: form.elements.identifier.value.trim(),
      username: form.elements.username.value.trim(),
      email: form.elements.email.value.trim(),
      password: form.elements.password.value
    };

    const error = validate(v);
    if (error) return showMessage(error, "error");

    submitBtn.disabled = true;
    const label = submitBtn.textContent;
    submitBtn.textContent = "Lütfen bekle...";
    showMessage("");

    try {
      if (mode === "login") {
        const session = await api.login(v);
        Session.save(session);
        window.location.href = ROLE_HOME[session.user.role];
      } else {
        await api.register(v);
        mode = "login";
        refresh();
        form.elements.identifier.value = v.username; // kolaylık: yeni kullanıcı adını giriş alanına yaz
        form.elements.password.value = "";
        showMessage("Kayıt başarılı. Şimdi giriş yapabilirsin.", "success");
      }
    } catch (err) {
      showMessage(err.message, "error");
    } finally {
      submitBtn.textContent = mode === "register" ? "Kayıt ol" : "Giriş yap";
      submitBtn.disabled = false;
    }
  });

  refresh();
})();