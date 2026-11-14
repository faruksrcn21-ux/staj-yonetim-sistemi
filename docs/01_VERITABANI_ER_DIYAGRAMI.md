# 01 - Veritabanı ER Diyagramı

Staj Evrak Doğrulama ve Yönetim Sistemi (EF Core + SQLite).

> Tablo adları backend'deki model sınıflarından (`BaseUser`, `Student`, `Admin`, `AdminTestDataset`, `LoginLog`, `RevokedToken`, `Document`) türetildi. Alan adları isterlere göre yazıldı, `backend/StajEvrak.Api/Models/` ile karşılaştırılıp düzeltilmelidir.

```mermaid
erDiagram
    BASE_USER ||--o| STUDENT : "ogrenci ise"
    BASE_USER ||--o| ADMIN : "admin ise"
    BASE_USER ||--o{ LOGIN_LOG : "giris denemesi"
    BASE_USER ||--o{ REVOKED_TOKEN : "cikista iptal"
    STUDENT ||--o{ DOCUMENT : "yukler"
    ADMIN ||--o{ ADMIN_TEST_DATASET : "yukler"

    BASE_USER {
        int id PK
        string username UK
        string email UK
        string password_hash
        string role "ROLE_STUDENT | ROLE_ADMIN"
        int failed_attempts
        datetime locked_until
    }

    STUDENT {
        int id PK
        string student_no UK
        string tc_no UK
    }

    ADMIN {
        int id PK
    }

    ADMIN_TEST_DATASET {
        int id PK
        string ad_soyad
        string tc_no
        string uyruk
        string ogrenci_no
        date dogum_tarihi
        string adres
        string bolum
        int teslim_yili
        int eski_staj_gun
        string email
        string telefon
        string saglik_yardimi
    }

    DOCUMENT {
        int id PK
        int student_id FK
        string file_name
        string file_path
        string status "Yuklendi | Inceleniyor | Hatali | Onaylandi | Tamamlandi"
        string form_data "ogrencinin girdigi Ek-16 bilgileri"
        string comparison_result "karsilastirma ve teslime hazir mi"
        datetime uploaded_at
    }

    LOGIN_LOG {
        int id PK
        int user_id FK
        boolean success
        datetime attempted_at
    }

    REVOKED_TOKEN {
        int id PK
        int user_id FK
        string token_id
        datetime expires_at
    }
```

## Açıklamalar

- **BASE_USER:** Öğrenci ve admin ortak tablosu. `role` alanı JWT içinde taşınır. `failed_attempts` 5 olunca hesap kilitlenir.
- **STUDENT / ADMIN:** Role özel bilgiler. Kalıtım yapısı sınıf diyagramında.
- **ADMIN_TEST_DATASET:** Adminin yüklediği CSV/Excel verisi. Belgedeki bilgilerle `ogrenci_no` / `tc_no` üzerinden eşleştirilir (yabancı anahtar yok, eşleştirme servis katmanında).
- **DOCUMENT:** Durum akışı Yüklendi, İnceleniyor, Hatalı/Eksik, Onaylandı, Tamamlandı.
- **LOGIN_LOG:** Başarısız girişler loglanır.
- **REVOKED_TOKEN:** Çıkış yapılınca token geçersiz kılınır.
