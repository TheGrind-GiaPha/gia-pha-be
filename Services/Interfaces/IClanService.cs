using GiaPha.BusinessObjects.DTOs.Clans;

namespace GiaPha.Services.Interfaces;

public interface IClanService
{
    Task<IReadOnlyList<ClanDto>> GetAllAsync(
        string? search,
        CancellationToken cancellationToken = default);

    Task<ClanDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<ClanDto> CreateAsync(
        CreateClanRequest request,
        CancellationToken cancellationToken = default);

    Task<ClanDto?> UpdateAsync(
        Guid id,
        UpdateClanRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
