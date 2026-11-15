using GiaPha.BusinessObjects.DTOs.Audits;

namespace GiaPha.Services.Interfaces;

public interface IAuditRestoreService
{
    Task<RestoreResultDto> RestoreAsync(
        Guid clanId,
        Guid logId,
        CancellationToken cancellationToken = default);
}
