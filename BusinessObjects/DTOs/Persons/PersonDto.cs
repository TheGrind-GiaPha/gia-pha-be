using GiaPha.BusinessObjects.Entities;

namespace GiaPha.BusinessObjects.DTOs.Persons;

public sealed record PersonDto(
    Guid Id,
    Guid ClanId,
    string FullName,
    string? HuyName,
    Gender Gender,
    FlexibleDateDto? BirthDate,
    FlexibleDateDto? DeathDate,
    int Generation,
    string Path,
    string? PortraitUrl,
    bool HasHiddenChildren);
