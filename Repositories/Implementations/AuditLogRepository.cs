using GiaPha.BusinessObjects.Entities;
using GiaPha.DataAccessObjects.DAOs;
using GiaPha.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GiaPha.Repositories.Implementations;

public sealed class AuditLogRepository : RepositoryBase<AuditLog>, IAuditLogRepository
{
    public AuditLogRepository(IGenericDao<AuditLog> dao) : base(dao)
    {
    }

    public async Task<IReadOnlyList<AuditLog>> GetByClanAsync(
        Guid clanId,
        CancellationToken cancellationToken = default)
        => await FindByCondition(x => x.ClanId == clanId, trackChanges: false)
            .OrderByDescending(x => x.OccurredAt)
            .ToListAsync(cancellationToken);
}
