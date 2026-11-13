# Staj Takip ve Yönetim Sistemi

Bu proje, Kocaeli Üniversitesi Programlama Laboratuvarı 3 dersi kapsamında geliştirilen **Staj Takip ve Yönetim Sistemi** projesidir.

---

## 📁 Proje Dizin Yapısı

```text
prolab3-staj-sistemi/
├── backend/                  # 1. ve 2. Kişinin geliştireceği Backend (API / Veritabanı / İş Mantığı)
├── frontend/                 # 3. Kişinin geliştireceği Kullanıcı Arayüzü (Web / UI)
├── docs/                     # Proje dokümantasyonu, diyagramlar ve iş bölümü
│   ├── 00_PROJE_ISTERLERI_VE_SSS.md   # Hocanın isterleri + soru-cevaplar (ESAS KAYNAK)
│   ├── 01_VERITABANI_ER_DIYAGRAMI.md
│   ├── 02_NYP_CLASS_DIYAGRAMI.md
│   ├── 03_AKIS_DIYAGRAMI_FLOWCHART.md
│   ├── 04_UML_USE_CASE_DIYAGRAMI.md
│   └── prolab3_vize_is_bolumu.txt
├── .gitignore
└── README.md
```

---

## 🛠️ Teknolojiler

- **Backend:** C# + ASP.NET Core Web API (.NET 10)
- **Veritabanı:** SQLite + Entity Framework Core (migration'lar repoda)
- **Kimlik Doğrulama:** JWT (rol claim'i) + BCrypt şifre hash'leme

---

## 👥 Ekip Görev Dağılımı

- **1. ve 2. Geliştirici:** Backend mimarisi, veritabanı tasarımı, yetkilendirme, iş kuralları ve API servisleri (`backend/`).
- **3. Geliştirici:** Kullanıcı arayüzü tasarımı, bileşenler ve API entegrasyonu (`frontend/`).

---