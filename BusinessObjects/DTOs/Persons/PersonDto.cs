using GiaPha.BusinessObjects.Entities;

namespace GiaPha.BusinessObjects.DTOs.Persons;

public sealed record PersonDto(
    Guid Id,
    Guid ClanId,
    string FullName,
    Gender Gender,
    int Generation,
    string Path);
