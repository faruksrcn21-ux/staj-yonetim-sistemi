# Proje İsterleri ve Hocanın Soru-Cevapları (ESAS KAYNAK)

> Bu dosya hocanın proje dokümanının (26_27_guz_prolab_3_proje.pdf) özetini ve
> forumda paylaşılan soru-cevapları içerir. **Plan veya diyagramlarla çelişki
> olursa bu dosya esas alınır.**

---

## 1. Hocanın Soru-Cevapları (Forum Duyurusu)

### S1. Vize aşamasında alan değerlerinin okunması
**Soru:** Vizede Ek-16'daki alan değerleri sistem tarafından otomatik mi (.docx / PDF kütüphaneleriyle) okunmalı, yoksa kullanıcı arayüzdeki formdan manuel mi girmeli?

**Cevap:** İkisinin de yapılması istenmektedir. Kullanıcının sisteme girdiği bilgiler sistemde kaydedilerek saklanmalıdır. Belgenin taranmasıyla elde edilen bilgiler ile kullanıcının sisteme girdiği bilgiler karşılaştırılarak belgenin teslime hazır olup olmadığı kontrol edilmelidir.

### S2. Vizede "ona bağlı kontroller" kapsamı
**Soru:** Bu ifade yalnızca dosya formatı/uzantı kontrollerini mi kapsar, yoksa 6. bölümdeki doğrulama kuralları da vizede bekleniyor mu?

**Cevap:** Yalnızca dosya formatı ve uzantı kontrollerini kapsar.

### S3. "Tamamlandı" statüsü ve admin yetkisi
**Cevap:** Öğrenci "Tamamlandı" statüsündeki belgeyi hiçbir şekilde değiştiremez/silemez, admin bu belgeyi güncelleyip silebilir.

### S4. Kullanıcı hesaplarının oluşturulması ve ilk şifre
**Cevap:** Kullanıcılar hesaplarını kendileri oluşturabilmelidir. Şifrelerini kendileri belirleyebilmelidir. Test veri seti, sistemin admin arayüzünün doğru çalışıp çalışmadığını kontrol etmek içindir. Gruplar admin arayüzünü test etmek için bir veri seti oluşturacaktır (sütunlar dokümanda belirtilmiştir). Admin arayüzü tarafında sisteme yüklenen belgeler ile test veri setindeki bilgiler karşılaştırılacaktır.

---

## 2. Vize Sunumu İçin Beklenenler (Doküman)

- UML, akış, veritabanı ve class diyagramları (rapordan bakılacak)
- Oturum açma ve yetkilendirme kontrolleri
- Admin ve öğrenci sayfalarının oluşturulması
- Admin kısmında veri setinin yüklenmesi
- NYP prensiplerinin (Kapsülleme, Soyutlama, Kalıtım, Çok Biçimlilik) uygulanması ve gösterilmesi
- Evrak yükleme işlemi ve ona bağlı kontroller (**sadece format/uzantı**, bkz. S2)
- Rapor ve kodların teslimi

## 3. Final Sunumu İçin Beklenenler (Vizede YAPILMAYACAK)

- Google Form ve Ek-17 belgesinin otomatik oluşturulması
- Admin sayfasında raporların ve veri filtrelemelerinin görüntülenmesi
- OCR entegrasyonu ve ona bağlı kontroller
- Bölüm 6 doğrulama kuralları (adres, telefon, e-posta, TC/yabancı kimlik, tarih, staj günü, imza/kaşe, mühendis sayısı)

---

## 4. Vize İçin İlgili Gereksinimler (Doküman Özeti)

### 4.1 Oturum Açma ve Kimlik Doğrulama
- Kullanıcı adı / e-posta + şifre ile giriş.
- Şifreler veritabanında açık tutulmaz, hash'lenir.
- Başarısız giriş denemeleri loglanır. **5 hatalı kullanıcı adı / şifre denemesinde sistem kilitlenir.**
- Giriş sonrası rol bilgisi session veya JWT içinde taşınır.
- Çıkış işlemi token/session'ı geçersiz kılar.

### 4.2 Yetkilendirme Katmanı
- Her korumalı endpoint/sayfa rolü doğrular: `ROLE_STUDENT` → öğrenci ekranları, `ROLE_ADMIN` → admin paneli.
- Yetkisiz istekler **403** döner ve login sayfasına yönlendirilir.
- Admin: tüm başvuruları görüntüler; tamamlanan başvurular **ayrı sekmede** listelenir; test veri setini içe aktarır.

### 4.3 Kullanıcı (Test) Veri Seti
- Sütunlar: Ad soyad, TC kimlik no, uyruk, öğrenci no, doğum tarihi, ikametgâh adresi (açık adres), bölüm, Ek-16 teslim yılı (2025-2026), daha önce yapılan staj gün sayısı, e-posta, telefon no, sağlık yardımı statüsü.
- Sistem bu veri setini veritabanına kaydeder.
- Yüklenen Ek-16'daki öğrenci bilgileri, giriş yapan kullanıcının veritabanındaki bilgileriyle karşılaştırılır; uyuşmazlık raporda belirtilir.

### 4.4 Evrak Yükleme
- Arayüzde **"Evrak Yükle"** butonu.
- Kabul: Word (.doc/.docx), PDF, JPEG, PNG. Diğerleri reddedilir ve kullanıcıya uyarı verilir.

### 5. Belge Statü Akışı
Yüklendi → İnceleniyor → Hatalı / Eksik → (düzeltilip yeniden yüklenir) → Onaylandı → Tamamlandı
- Onaylanan belgeler Tamamlandı statüsüne geçer.
- Tamamlandı belgeleri öğrenci değiştiremez/silemez; yalnızca admin sayfasından güncelleme/silme yapılabilir (bkz. S3).

---

## 5. Teslim ve Sunum Kuralları

- Rapor **IEEE formatında, en az 4 sayfa**: Özet, giriş, yöntem (algoritma açıklaması), akış diyagramı, UML diyagramı, class diyagramı, kaba kod (pseudocode), sonuç.
- **Rapor içeriği öğrenciler tarafından hazırlanmalıdır. Doğrudan yapay zekâ ile oluşturulduğu tespit edilen raporlar değerlendirmeye alınmaz.**
- Demo sırasında kodun herhangi bir kısmının açıklanması ve algoritma mantığının anlatılması istenebilir.
- **Sunum günü tüm grup üyeleri bulunmalıdır; gelmeyen olursa direkt 0 girilir.**
- Sorular: edestek forumu (Araş. Gör. Melike BEKTAŞ KÖSESOY, Şevval ŞOLPAN, Beyza Nur CANSEVER).
