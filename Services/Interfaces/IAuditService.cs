using GiaPha.BusinessObjects.DTOs.Changes;

namespace GiaPha.Services.Interfaces;

public interface IAuditService
{
    Task<IReadOnlyList<AuditLogDto>> GetByClanAsync(
        Guid clanId,
        CancellationToken cancellationToken = default);
}
