# 04 - UML Use Case Diyagramı

> Mermaid'de doğrudan use case diyagramı yoktur, bu yüzden `flowchart` ile çizildi: yuvarlak kutular aktörleri, oval şekiller kullanım senaryolarını, kesikli oklar `include` / `extend` ilişkilerini gösterir.

```mermaid
flowchart LR
    Ogrenci(["Öğrenci"])
    Admin(["Admin"])

    subgraph SISTEM ["Staj Evrak Doğrulama ve Yönetim Sistemi"]
        UC1(("Kayıt ol"))
        UC2(("Giriş yap"))
        UC3(("Çıkış yap"))
        UC4(("Evrak yükle"))
        UC5(("Ek-16 bilgilerini gir"))
        UC6(("Dosya formatını kontrol et"))
        UC7(("Belge verisini karşılaştır"))
        UC8(("Karşılaştırma sonucunu gör"))
        UC9(("Belgeyi düzelt veya sil"))
        UC10(("Test veri setini yükle"))
        UC11(("Belgeleri listele"))
        UC12(("Belge durumunu güncelle"))
        UC13(("Belgeyi sil"))
    end

    Ogrenci --- UC1
    Ogrenci --- UC2
    Ogrenci --- UC3
    Ogrenci --- UC4
    Ogrenci --- UC8
    Ogrenci --- UC9

    Admin --- UC2
    Admin --- UC3
    Admin --- UC10
    Admin --- UC11
    Admin --- UC12
    Admin --- UC13

    UC4 -.->|include| UC5
    UC4 -.->|include| UC6
    UC4 -.->|include| UC7
    UC9 -.->|extend: Tamamlandı değilse| UC4
```

## Aktörler

- **Öğrenci:** Kendi hesabını oluşturur, Ek-16 evrakını yükler, bilgilerini elle girer, karşılaştırma sonucunu ve teslime hazır durumunu görür.
- **Admin:** Test veri setini yükler, tüm belgeleri listeler, durumu günceller, belgeyi siler. Tamamlanan belgeleri de değiştirebilir.

## İlişkiler

- **Evrak yükle (include):** Bilgi girme, format kontrolü ve karşılaştırma yüklemenin parçasıdır.
- **Belgeyi düzelt veya sil (extend):** Sadece belge "Tamamlandı" değilse yapılabilir.
