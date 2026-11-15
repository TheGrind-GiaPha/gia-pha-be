namespace GiaPha.BusinessObjects.DTOs.Persons;

/// <summary>Kết quả ghép cây con: cây đã ghép + danh sách ứng viên trùng (để gộp qua 2.6).</summary>
public sealed record GraftResultDto(
    PersonDto MovedRoot,
    IReadOnlyList<PersonDto> DuplicateCandidates);
