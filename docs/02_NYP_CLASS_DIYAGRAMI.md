# 02 - NYP Sınıf Diyagramı

> Sınıf adları backend dosyalarından alındı. Üye (alan/metot) adları isterlere göre yazıldı, kodla karşılaştırıp düzelt.

```mermaid
classDiagram
    class BaseUser {
        <<abstract>>
        -int id
        -string username
        -string email
        -string passwordHash
        -int failedAttempts
        -datetime lockedUntil
        +string Role*
        +RegisterFailedAttempt() void
        +IsLocked() bool
    }

    class Student {
        -string studentNo
        -string tcNo
        +string Role
    }

    class Admin {
        +string Role
    }

    class IPasswordHasher {
        <<interface>>
        +Hash(password) string
        +Verify(password, hash) bool
    }

    class BCryptPasswordHasher {
        +Hash(password) string
        +Verify(password, hash) bool
    }

    class AuthService {
        +Register(request) void
        +Login(request) string
        +Logout(token) void
    }

    class JwtTokenService {
        +CreateToken(user) string
    }

    class DocumentService {
        +Upload(student, file, formData) Document
        +Delete(user, documentId) void
        +UpdateStatus(admin, documentId, status) void
    }

    class StudentDatasetMatcher {
        +Compare(formData, datasetRecord) ComparisonResult
    }

    class Document {
        -int id
        -string fileName
        -DocumentStatus status
        -string formData
        -string comparisonResult
        -datetime uploadedAt
    }

    class DocumentStatus {
        <<enumeration>>
        Uploaded
        Reviewing
        Faulty
        Approved
        Completed
    }

    class AdminTestDataset {
        -string adSoyad
        -string tcNo
        -string ogrenciNo
        -string email
    }

    BaseUser <|-- Student
    BaseUser <|-- Admin
    IPasswordHasher <|.. BCryptPasswordHasher
    AuthService ..> IPasswordHasher : kullanir
    AuthService ..> JwtTokenService : kullanir
    DocumentService ..> StudentDatasetMatcher : kullanir
    StudentDatasetMatcher ..> AdminTestDataset : okur
    Student "1" --> "0..*" Document : yukler
    Document --> DocumentStatus
```

## NYP prensipleri

- **Kapsülleme (Encapsulation):** Şifre hash'i ve deneme sayısı `private`, dışarıdan sadece `RegisterFailedAttempt()` ve `IsLocked()` ile yönetilir.
- **Soyutlama (Abstraction):** `BaseUser` soyut sınıf, doğrudan nesnesi oluşturulmaz. `IPasswordHasher` arayüzü hash algoritmasının ayrıntısını gizler.
- **Kalıtım (Inheritance):** `Student` ve `Admin`, `BaseUser`'dan türer.
- **Çok biçimlilik (Polymorphism):** `Role` her alt sınıfta farklı değer döner. `AuthService`, `IPasswordHasher` üzerinden çalışır, algoritma değişse (BCrypt yerine Argon2) servis değişmez.
