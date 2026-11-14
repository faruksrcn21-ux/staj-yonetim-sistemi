namespace StajEvrak.Api.Readers;

//Dosyadaki sütun sırası . İki reader da aynı sırayı kullanır.
public static class StudentDatasetColumns
{
    public const int Count = 12;

    public const int FullName = 0, TcNo = 1, Nationality = 2, StudentNo = 3, BirthDate = 4,
                     Address = 5, Department = 6, SubmissionYear = 7, PreviousInternshipDays = 8,
                     Email = 9, Phone = 10, HealthInsuranceStatus = 11;
}