using GiaPha.BusinessObjects.Entities;

namespace GiaPha.Repositories.Interfaces;

public interface IAuditLogRepository : IRepositoryBase<AuditLog>
{
    Task<IReadOnlyList<AuditLog>> GetByClanAsync(
        Guid clanId,
        CancellationToken cancellationToken = default);
}
