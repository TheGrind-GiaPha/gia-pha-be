using GiaPha.BusinessObjects.Entities;
using GiaPha.BusinessObjects.Queries;

namespace GiaPha.Repositories.Interfaces;

public interface IAuditLogRepository : IRepositoryBase<AuditLog>
{
    Task<IReadOnlyList<AuditLog>> GetByClanAsync(
        AuditLogFilter filter,
        bool trackChanges,
        CancellationToken cancellationToken = default);

    Task<int> CountByClanAsync(
        AuditLogFilter filter,
        CancellationToken cancellationToken = default);

    Task<AuditLog?> GetByIdAsync(
        Guid id,
        bool trackChanges,
        CancellationToken cancellationToken = default);
}
