namespace GiaPha.BusinessObjects.DTOs.Persons;

/// <summary>Một node trong cây gia phả (dùng cho API đọc cây).</summary>
public sealed record PersonNodeDto(
    PersonDto Person,
    IReadOnlyList<PersonNodeDto> Children);
