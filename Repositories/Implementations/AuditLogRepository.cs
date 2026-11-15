using GiaPha.BusinessObjects.Entities;
using GiaPha.BusinessObjects.Queries;
using GiaPha.DataAccessObjects.DAOs;
using GiaPha.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GiaPha.Repositories.Implementations;

public sealed class AuditLogRepository : RepositoryBase<AuditLog>, IAuditLogRepository
{
    public AuditLogRepository(IAuditLogDao dao) : base(dao)
    {
    }

    public async Task<IReadOnlyList<AuditLog>> GetByClanAsync(
        AuditLogFilter filter,
        bool trackChanges,
        CancellationToken cancellationToken = default)
        => await BuildQuery(filter, trackChanges)
            .OrderByDescending(x => x.OccurredAt)
            .ThenByDescending(x => x.Id)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountByClanAsync(
        AuditLogFilter filter,
        CancellationToken cancellationToken = default)
        => BuildQuery(filter, trackChanges: false).CountAsync(cancellationToken);

    public Task<AuditLog?> GetByIdAsync(
        Guid id,
        bool trackChanges,
        CancellationToken cancellationToken = default)
        => FindByCondition(x => x.Id == id, trackChanges)
            .FirstOrDefaultAsync(cancellationToken);

    private IQueryable<AuditLog> BuildQuery(AuditLogFilter filter, bool trackChanges)
    {
        var query = FindByCondition(x => x.ClanId == filter.ClanId, trackChanges);

        if (!string.IsNullOrWhiteSpace(filter.EntityType))
        {
            var entityType = filter.EntityType.Trim();
            query = query.Where(x => x.EntityType == entityType);
        }

        if (filter.EntityId is not null)
        {
            query = query.Where(x => x.EntityId == filter.EntityId);
        }

        if (filter.ActorId is not null)
        {
            query = query.Where(x => x.ActorId == filter.ActorId);
        }

        if (filter.From is not null)
        {
            query = query.Where(x => x.OccurredAt >= filter.From);
        }

        if (filter.To is not null)
        {
            query = query.Where(x => x.OccurredAt <= filter.To);
        }

        return query;
    }
}
