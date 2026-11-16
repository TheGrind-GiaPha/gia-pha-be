using GiaPha.BusinessObjects.DTOs.Audits;
using GiaPha.BusinessObjects.DTOs.Common;

namespace GiaPha.Services.Interfaces;

public interface IAuditLogService
{
    Task<PagedResult<AuditLogDto>> GetByClanAsync(
        Guid clanId,
        AuditLogQuery query,
        CancellationToken cancellationToken = default);

    Task<PagedResult<AuditLogDto>> GetPersonHistoryAsync(
        Guid clanId,
        Guid personId,
        AuditLogQuery query,
        CancellationToken cancellationToken = default);
}
