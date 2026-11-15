using GiaPha.BusinessObjects.Entities;

namespace GiaPha.Services.Interfaces;

public interface IDuplicateDetectionService
{
    /// <summary>
    /// So khớp một người dự kiến import với danh sách người hiện có trong dòng họ.
    /// Trả về (isDuplicate, matchedPersonId, warningMessage).
    /// </summary>
    (bool IsDuplicate, Guid? MatchedPersonId, string? Warning) CheckDuplicate(
        string candidateFullName,
        int? candidateBirthYear,
        string? candidateFatherName,
        Guid? candidateFatherId,
        IReadOnlyList<Person> existingPersons,
        IReadOnlyDictionary<Guid, Guid> existingParentChildMap);

    /// <summary>
    /// Loại bỏ dấu tiếng Việt và đưa về chữ thường để so sánh.
    /// </summary>
    string RemoveDiacritics(string text);
}
