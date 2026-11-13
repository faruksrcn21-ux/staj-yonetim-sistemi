# Sistem Akış Diyagramı (Flowchart) Taslağı - Vize Süreci

Bu diyagram, kullanıcının sisteme girişinden itibaren Öğrenci ve Admin rollerinin vize kapsamındaki tüm işlem adımlarını gösterir.

## 1. Adım Adım Süreç Akışı
1. **Kimlik Doğrulama:** Kullanıcı kayıt olur veya giriş yapar. Başarısız denemeler loglanır; üst üste 5 hatalı şifrede hesap 15 dakika kilitlenir.
2. **Rol Yönlendirmesi:**
   * `ROLE_STUDENT`: Öğrenci paneline yönlendirilir.
   * `ROLE_ADMIN`: Admin paneline yönlendirilir.
   * Yetkisiz erişimlerde HTTP 403 hatası verilir.
3. **Öğrenci Süreci:**
   * Ek-16 dosyası seçilir + arayüzdeki formdan manuel bilgiler girilir.
   * **Uzantı Kontrolü:** Dosya `.doc`, `.docx`, `.pdf`, `.jpeg`, `.png` formatında değilse işlem reddedilir ve uyarı gösterilir.
   * **Belge Okuma:** `.docx`/PDF kütüphanesi ile belgedeki alanlar otomatik okunur (OCR yok).
   * **Karşılaştırma:** Belgeden okunan veriler, manuel girilen form verileri ve veritabanındaki Admin test veri seti karşılaştırılır.
   * Belgenin teslime hazır olup olmadığı raporlanır ve kaydedilir.
4. **Admin Süreci:**
   * Test veri seti yüklenir ve veritabanına işlenir.
   * "Tamamlandı" statüsündeki belgeler görüntülenir, güncellenir veya silinir (Öğrenci bu statüdeki belgeye dokunamaz).

---

## 2. Görsel Akış Diyagramı (Mermaid)

```mermaid
flowchart TD
    Start([Başla]) --> HasAccount{Hesabı Var mı?}
    HasAccount -- Hayır --> Register[Kayıt Ol Ekranı: Kullanıcı Şifresini Belirler]
    Register --> Login[Giriş Ekranı: E-posta/Kullanıcı Adı + Şifre]
    HasAccount -- Evet --> Login

    Login --> AuthCheck{Bilgiler Doğru mu?}
    AuthCheck -- Hayır --> LogFail[Başarısız Girişi Logla & Sayacı Artır]
    LogFail --> LockCheck{Hata Sayısı == 5 mi?}
    LockCheck -- Evet --> LockAccount[Hesabı 15 dk Kilitle] --> End([Bitir])
    LockCheck -- Hayır --> Login

    AuthCheck -- Evet --> RoleCheck{Kullanıcı Rolü Nedir?}

    %% ÖĞRENCİ AKIŞI
    RoleCheck -- ROLE_STUDENT --> StudentPanel[Öğrenci Paneli]
    StudentPanel --> UploadAction[Ek-16 Belgesi Seç & Manuel Formu Doldur]
    UploadAction --> ExtCheck{Uzantı Geçerli mi?<br/>.doc/.docx/.pdf/.jpeg/.png}
    ExtCheck -- Hayır --> ExtError[Hata Uyarısı Göster ve Reddet] --> StudentPanel
    ExtCheck -- Evet --> ParseDoc[Kütüphane ile Belgedeki Alanları Otomatik Oku]
    ParseDoc --> CompareData[Belge Verisi + Manuel Form + Test Veri Setini Karşılaştır]
    CompareData --> SaveResult[Sonucu Kaydet & Teslime Hazır mı Raporla]
    SaveResult --> StudentPanel

    %% ADMİN AKIŞI
    RoleCheck -- ROLE_ADMIN --> AdminPanel[Admin Paneli]
    AdminPanel --> AdminChoice{Admin İşlemi Seç}
    AdminChoice -- Test Veri Seti Yükle --> ImportDataset[Veri Setini Oku & Veritabanına Kaydet] --> AdminPanel
    AdminChoice -- Belgeleri Yönet --> ManageDocs[Tamamlandı Belgeleri Görüntüle / Güncelle / Sil] --> AdminPanel
```