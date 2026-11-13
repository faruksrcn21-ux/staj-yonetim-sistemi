# Nesne Yönelimli Programlama (NYP) Class Diyagramı Taslağı

Bu yapı, projede zorunlu tutulan 4 temel NYP prensibini (**Kapsülleme, Soyutlama, Kalıtım, Çok Biçimlilik**) kod mimarisine doğrudan entegre etmek için hazırlanmıştır.

## 1. NYP Prensiplerinin Uygulanma Yerleri
1. **Kapsülleme (Encapsulation):** Tüm sınıf değişkenleri `private` (`-`) tanımlanır; erişim kontrollü `public` (`+`) getter/setter ve iş metotları üzerinden sağlanır.
2. **Soyutlama (Abstraction):** `BaseUser` soyut sınıfı (`abstract class`) ve `IDocumentParser` arayüzü (`interface`) üzerinden ortak sözleşmeler tanımlanır.
3. **Kalıtım (Inheritance):** `Student` ve `Admin` sınıfları `BaseUser` sınıfından türetilir. `PdfParser` ve `DocxParser` sınıfları `IDocumentParser` arayüzünü uygular.
4. **Çok Biçimlilik (Polymorphism):** `DocumentService` sınıfı, dosyanın türüne göre arka planda hangi parser çalışırsa çalışsın hepsini tek bir `IDocumentParser` tipi üzerinden çağırır (`parser.extractFields(file)`).

---

## 2. Sınıf Diyagramı (Mermaid)

```mermaid
classDiagram
    class BaseUser {
        <<Abstract>>
        -int id
        -string username
        -string email
        -string passwordHash
        -string role
        -int failedAttempts
        -DateTime? lockedUntil
        +isLocked(now) boolean
        +registerFailedAttempt(now) void
        +resetFailedAttempts() void
        +getDashboardView()* string
    }

    class AuthService {
        +register(username, email, password, studentNo) Student
        +login(identifier, password, ip) Token
        +logout(jti) void
    }

    class Student {
        -string studentNo
        +uploadDocument(file, manualFormData) DocumentReport
        +getMyDocuments() List~Document~
        +getDashboardView() string
    }

    class Admin {
        +importTestDataset(datasetFile) boolean
        +getAllDocuments() List~Document~
        +updateCompletedDocument(docId, newData) boolean
        +deleteCompletedDocument(docId) boolean
        +getDashboardView() string
    }

    class IDocumentParser {
        <<Interface>>
        +validateFormat(fileName) boolean
        +extractFields(filePath) Map~string, string~
    }

    class PdfDocumentParser {
        -string[] allowedExtensions
        +validateFormat(fileName) boolean
        +extractFields(filePath) Map~string, string~
    }

    class DocxDocumentParser {
        -string[] allowedExtensions
        +validateFormat(fileName) boolean
        +extractFields(filePath) Map~string, string~
    }

    class DocumentComparisonService {
        +compareFormAndFile(manualData, parsedData) boolean
        +compareWithTestDataset(studentData, datasetRecord) boolean
        +generateReport() string
    }

    BaseUser <|-- Student : Inheritance
    BaseUser <|-- Admin : Inheritance
    AuthService --> BaseUser : Authenticates
    IDocumentParser <|.. PdfDocumentParser : Implements
    IDocumentParser <|.. DocxDocumentParser : Implements
    Student --> IDocumentParser : Uses (Polymorphism)
    Student --> DocumentComparisonService : Uses
```