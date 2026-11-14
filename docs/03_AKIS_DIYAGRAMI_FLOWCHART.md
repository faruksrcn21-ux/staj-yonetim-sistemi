# 03 - Akış Diyagramı (Flowchart)

Giriş, evrak yükleme, karşılaştırma ve belge durum akışı.

```mermaid
flowchart TD
    A([Başla]) --> B[/"Kullanıcı adı veya e-posta ve şifre gir"/]
    B --> C{"Hesap kilitli mi?"}
    C -- Evet --> D["Kilit mesajı göster"]
    D --> Z([Bitir])
    C -- Hayır --> E{"Bilgiler doğru mu?"}
    E -- Hayır --> F["Hatalı denemeyi logla, sayacı artır"]
    F --> G{"5 hatalı deneme oldu mu?"}
    G -- Evet --> H["Hesabı kilitle"]
    H --> D
    G -- Hayır --> B
    E -- Evet --> I["JWT üret, rol token içinde"]
    I --> J{"Rol nedir?"}

    J -- ROLE_STUDENT --> S1["Öğrenci ekranı"]
    S1 --> S2["Ek-16 bilgilerini forma elle gir"]
    S2 --> S3["Evrak Yükle: dosya seç"]
    S3 --> S4{"Format .doc .docx .pdf .jpeg .png mi?"}
    S4 -- Hayır --> S5["Uyarı göster, dosyayı reddet"]
    S5 --> S3
    S4 -- Evet --> S6["Belge kaydedilir, durum: Yüklendi"]
    S6 --> S7["Belgeden metni oku .docx / PDF kütüphanesiyle"]
    S7 --> S8{"Belgedeki veri, forma girilen ve veri setiyle uyuşuyor mu?"}
    S8 -- Evet --> S9["Durum: İnceleniyor, teslime hazır"]
    S8 -- Hayır --> S10["Durum: Hatalı / Eksik, uyuşmayan alanları göster"]
    S10 --> S11["Öğrenci düzeltip yeniden yükler"]
    S11 --> S2
    S9 --> S12["Sonucu öğrenciye göster"]

    J -- ROLE_ADMIN --> K1["Admin ekranı"]
    K1 --> K2["Test veri setini yükle CSV veya Excel"]
    K2 --> K3["Veri veritabanına aktarılır"]
    K1 --> K4["Belgeleri listele, durumu güncelle"]
    K4 --> K5{"Durum Tamamlandı mı?"}
    K5 -- Evet --> K6["Belge Tamamlananlar sekmesine geçer, öğrenci değiştiremez"]
    K5 -- Hayır --> K7["Yüklenen belgeler sekmesinde kalır"]

    S12 --> Z
    K3 --> Z
    K6 --> Z
    K7 --> Z
```

## Kurallar

- 5 hatalı denemede hesap kilitlenir, her başarısız deneme loglanır.
- Rol JWT içinde taşınır, yetkisiz istek 403 döner ve login sayfasına yönlendirilir.
- Vizede sadece format/uzantı kontrolü zorunlu. OCR ve ayrıntılı alan doğrulamaları finalde.
- Öğrenci "Tamamlandı" belgeyi değiştiremez veya silemez, admin yapabilir.
