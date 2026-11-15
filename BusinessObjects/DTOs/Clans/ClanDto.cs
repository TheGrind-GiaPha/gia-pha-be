using GiaPha.BusinessObjects.Entities;

namespace GiaPha.BusinessObjects.DTOs.Clans;

public sealed record ClanDto(
    Guid Id,
    string Name,
    string? Description,
    ClanStatus Status,
    DateTime? PublishedAt,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
