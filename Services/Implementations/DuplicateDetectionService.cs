using System.Globalization;
using System.Text;
using GiaPha.BusinessObjects.Entities;
using GiaPha.Services.Interfaces;

namespace GiaPha.Services.Implementations;

public sealed class DuplicateDetectionService : IDuplicateDetectionService
{
    public (bool IsDuplicate, Guid? MatchedPersonId, string? Warning) CheckDuplicate(
        string candidateFullName,
        int? candidateBirthYear,
        string? candidateFatherName,
        Guid? candidateFatherId,
        IReadOnlyList<Person> existingPersons,
        IReadOnlyDictionary<Guid, Guid> existingParentChildMap)
    {
        var normalizedCandidateName = RemoveDiacritics(candidateFullName);
        var normalizedCandidateFatherName = string.IsNullOrWhiteSpace(candidateFatherName)
            ? null
            : RemoveDiacritics(candidateFatherName);

        var personById = existingPersons.ToDictionary(p => p.Id);

        foreach (var person in existingPersons)
        {
            var normalizedPersonName = RemoveDiacritics(person.FullName);
            if (normalizedPersonName != normalizedCandidateName)
            {
                continue;
            }

            // 1. Kiểm tra năm sinh (lệch tối đa 1 năm nếu cả 2 bên đều có năm sinh)
            var birthYearMatched = false;
            var personBirthYear = person.BirthDate?.Year;

            if (candidateBirthYear.HasValue && personBirthYear.HasValue)
            {
                birthYearMatched = Math.Abs(candidateBirthYear.Value - personBirthYear.Value) <= 1;
            }
            else if (!candidateBirthYear.HasValue && !personBirthYear.HasValue)
            {
                // Cả hai cùng không rõ năm sinh
                birthYearMatched = true;
            }

            if (!birthYearMatched)
            {
                continue;
            }

            // 2. Kiểm tra cha trùng
            var fatherMatched = false;
            if (existingParentChildMap.TryGetValue(person.Id, out var existingFatherId))
            {
                // Người trong DB có cha
                if (candidateFatherId.HasValue && candidateFatherId.Value == existingFatherId)
                {
                    fatherMatched = true;
                }
                else if (normalizedCandidateFatherName is not null && personById.TryGetValue(existingFatherId, out var existingFather))
                {
                    var normalizedExistingFatherName = RemoveDiacritics(existingFather.FullName);
                    if (normalizedExistingFatherName == normalizedCandidateFatherName)
                    {
                        fatherMatched = true;
                    }
                }
            }
            else
            {
                // Người trong DB không có cha (ví dụ là đời 1)
                if (!candidateFatherId.HasValue && string.IsNullOrWhiteSpace(normalizedCandidateFatherName))
                {
                    fatherMatched = true;
                }
            }

            if (fatherMatched)
            {
                var warning = $"Nghi trùng với '{person.FullName}' (Sinh năm: {person.BirthDate?.Year?.ToString() ?? "chưa rõ"}, Đời thứ: {person.Generation}) do trùng tên, năm sinh lệch <= 1 và cùng người cha.";
                return (true, person.Id, warning);
            }
        }

        return (false, null, null);
    }

    public string RemoveDiacritics(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;

        var normalizedString = text.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder();

        foreach (var c in normalizedString)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }

        return stringBuilder
            .ToString()
            .Normalize(NormalizationForm.FormC)
            .ToLowerInvariant()
            .Replace("đ", "d")
            .Replace("Đ", "d")
            .Trim();
    }
}
