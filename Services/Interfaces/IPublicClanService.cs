using GiaPha.BusinessObjects.DTOs.Public;

namespace GiaPha.Services.Interfaces;

public interface IPublicClanService
{
    Task<IReadOnlyList<PublicClanDto>> GetPublicClansAsync(
        CancellationToken cancellationToken = default);

    Task<PublicPersonNodeDto?> GetPublicTreeAsync(
        Guid clanId,
        int depth = 20,
        CancellationToken cancellationToken = default);
}
