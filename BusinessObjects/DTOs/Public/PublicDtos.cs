using GiaPha.BusinessObjects.DTOs.Persons;
using GiaPha.BusinessObjects.Entities;

namespace GiaPha.BusinessObjects.DTOs.Public;

public sealed record PublicClanDto(
    Guid Id,
    string Name,
    string? Description);

/// <summary>Thông tin người trong cây công khai (đã lọc theo privacy).</summary>
public sealed record PublicPersonDto(
    Guid Id,
    string FullName,
    Gender Gender,
    int Generation,
    bool IsLiving,
    FlexibleDateDto? BirthDate,
    FlexibleDateDto? DeathDate,
    string? PortraitUrl);

public sealed record PublicPersonNodeDto(
    PublicPersonDto Person,
    IReadOnlyList<PublicPersonNodeDto> Children);
