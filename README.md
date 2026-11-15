# Staj Evrak Doğrulama ve Yönetim Sistemi

Kocaeli Üniversitesi Programlama Laboratuvarı III dersi projesi. Öğrenciler Ek-16 belgesini yükler ve bilgilerini forma girer. Sistem belgeyi okur; girilen bilgileri belgeyle ve admin'in yüklediği test veri setiyle karşılaştırıp belgenin teslime hazır olup olmadığını gösterir.

## Kurulum ve Çalıştırma

### Gereksinimler

- .NET 10 SDK
- Herhangi bir tarayıcı

### 1. Gizli ayarlar (.env)

JWT anahtarı ve admin şifresi repoda tutulmaz. `backend/StajEvrak.Api/.env.example` dosyasını aynı klasöre `.env` adıyla kopyalayıp doldurun:

```
Jwt__Key=<en az 32 karakterlik rastgele bir metin>
AdminSeed__Username=admin
AdminSeed__Email=admin@staj.local
AdminSeed__Password=<admin şifresi>
```

### 2. Backend

```
cd backend/StajEvrak.Api
dotnet run
```

API `http://localhost:5062` adresinde çalışır. İlk açılışta SQLite veritabanı ve tablolar otomatik oluşturulur, `.env`'deki bilgilerle admin hesabı açılır.

### 3. Frontend

`frontend/` klasörünü bir yerel sunucuyla açın. Örneğin:

- VS Code'da Live Server eklentisiyle `frontend/index.html`, ya da
- `cd frontend` ve `python -m http.server 5500`, ardından `http://localhost:5500`

Frontend `frontend/app.js` içindeki `API_BASE` adresine bağlanır. Backend olmadan arayüzü denemek için aynı dosyada `USE_MOCK: true` yapılabilir.

## Kullanım

1. Admin girişi yapın ve **Test veri seti** sekmesinden `ornek-veriler/test_veri_seti.xlsx` (veya `.csv`) dosyasını yükleyin.
2. Çıkış yapıp **Kayıt ol** sekmesinden öğrenci hesabı açın. E-posta, veri setindeki öğrencinin e-postası olmalıdır; öğrencinin bilgileri bu e-postayla eşleştirilir.
3. Öğrenci olarak **Evrak Yükle** ile formu doldurup Ek-16 belgesini yükleyin.
   - `ornek_ek16_ali_veli_uyumlu.docx` / `.pdf`: bilgiler uyuşur, belge **İnceleniyor** ve teslime hazırdır.
   - `ornek_ek16_ali_veli_uyusmazlik.docx`: öğrenci no ve telefon farklıdır, belge **Hatalı / Eksik** olur ve uyuşmayan alanlar listelenir.
4. Admin belgeyi inceler, **Dosyayı aç** ile yüklenen dosyayı görür ve onaylar. Belge **Tamamlandı** sekmesine geçer; öğrenci artık değiştiremez ve silemez.

## Belge Statü Akışı

```
Yüklendi > İnceleniyor > Hatalı / Eksik > (düzeltilip yeniden yüklenir) > Onaylandı > Tamamlandı
```

- Yüklenen belge okunur ve karşılaştırılır. Uyuşmazlık yoksa İnceleniyor olur ve admin onayını bekler; varsa Hatalı / Eksik olur.
- Admin onaylayınca belge Tamamlandı olur. Tamamlandı belgeyi öğrenci değiştiremez ve silemez; admin güncelleyip silebilir.
- Kabul edilen formatlar: `.doc`, `.docx`, `.pdf`, `.jpeg`, `.jpg`, `.png`. Görsel ve eski `.doc` dosyalarından metin okunamaz; bu belgelerde girilen bilgiler sadece veri setiyle karşılaştırılır ve belgeyi admin inceler.

## Proje Yapısı

```
backend/
├── StajEvrak.slnx
├── dotnet-tools.json
└── StajEvrak.Api/
    ├── Controllers/     API uç noktaları: Auth, Documents, DocumentUpload,
    │                    AdminDocuments, AdminDataset, RoleCheck
    ├── Models/          Varlıklar: BaseUser, Student, Admin, Document, DocumentStatus,
    │                    DocumentAccess, AdminTestDataset, LoginLog, RevokedToken, Roles
    ├── Services/        İş mantığı: Auth, Jwt, BCrypt şifre hash'leme, Document,
    │                    DocumentUpload, DatasetImport, StudentDatasetMatcher,
    │                    UploadedFileValidator
    ├── Comparison/      Ek-16 alanlarını okuma ve karşılaştırma
    ├── Parsing/         Belge okuyucuları: PDF, DOCX, okunamayan formatlar, factory
    ├── Readers/         Veri seti okuyucuları: CSV, Excel, factory
    ├── Repositories/    Veritabanı erişimi: Document, TestDataset
    ├── Storage/         Yüklenen dosyaların saklanması
    ├── Data/            AppDbContext, migration'lar, admin seed
    ├── Dtos/            İstek ve yanıt modelleri
    └── Extensions/      Servis kayıtları (Document ve Dataset modülleri)
frontend/                Giriş, öğrenci ve admin sayfaları (HTML, CSS, JS)
ornek-veriler/           Demo için test veri seti ve örnek belgeler
```

## Örnek veriler (`ornek-veriler/`)

| Dosya | Açıklama |
|---|---|
| `test_veri_seti.csv`, `test_veri_seti.xlsx` | 12 sütunlu test veri seti, 8 öğrenci (biri yabancı uyruklu) |
| `test_veri_seti_hatali_satirlar.csv` | 3 hatalı, 1 doğru satır; içe aktarmadaki satır uyarılarını gösterir |
| `ornek_ek16_ali_veli_uyumlu.docx` / `.pdf` | Veri setiyle uyumlu örnek belge |
| `ornek_ek16_ali_veli_uyusmazlik.docx` | Kasıtlı uyuşmazlık içeren örnek belge |

Örnek belgeler gerçek Ek-16 şablonu değildir; aynı etiketleri içeren sadeleştirilmiş formlardır. Bilgiler uydurmadır.

## Teknolojiler

- **Backend:** C# + ASP.NET Core Web API (.NET 10)
- **Veritabanı:** SQLite + Entity Framework Core
- **Kimlik doğrulama:** JWT (rol token içinde), şifreler BCrypt ile hash'lenir
- **Belge okuma:** PdfPig (PDF), DocumentFormat.OpenXml (Word); veri seti: CsvHelper, ClosedXML
- **Frontend:** Saf HTML, CSS ve JavaScript

## Ekip Görev Dağılımı

1. Geliştirici: veritabanı, kimlik doğrulama, yetkilendirme ve güvenlik
2. Geliştirici: evrak yükleme, belge okuma, veri karşılaştırma ve test veri seti
3. Geliştirici: arayüz, rapor ve diyagramlar
