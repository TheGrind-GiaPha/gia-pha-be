namespace GiaPha.BusinessObjects.DTOs.Clans;

public sealed record ClanDto(
    Guid Id,
    string Name,
    string? Description,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
