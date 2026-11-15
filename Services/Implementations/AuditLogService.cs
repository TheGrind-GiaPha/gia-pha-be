using GiaPha.BusinessObjects.DTOs.Audits;
using GiaPha.BusinessObjects.DTOs.Common;
using GiaPha.BusinessObjects.Exceptions;
using GiaPha.BusinessObjects.Queries;
using GiaPha.Repositories.Interfaces;
using GiaPha.Services.Auditing;
using GiaPha.Services.Interfaces;

namespace GiaPha.Services.Implementations;

public sealed class AuditLogService : IAuditLogService
{
    private readonly IRepositoryManager _repository;

    public AuditLogService(IRepositoryManager repository)
    {
        _repository = repository;
    }

    public async Task<PagedResult<AuditLogDto>> GetByClanAsync(
        Guid clanId,
        AuditLogQuery query,
        CancellationToken cancellationToken = default)
    {
        var clan = await _repository.Clan.GetByIdAsync(clanId, trackChanges: false, cancellationToken);
        if (clan is null)
        {
            throw new NotFoundException("Không tìm thấy dòng họ.");
        }

        return await QueryAsync(BuildFilter(clanId, query), cancellationToken);
    }

    public async Task<PagedResult<AuditLogDto>> GetPersonHistoryAsync(
        Guid clanId,
        Guid personId,
        AuditLogQuery query,
        CancellationToken cancellationToken = default)
    {
        var person = await _repository.Person.GetByIdAsync(personId, trackChanges: false, cancellationToken);
        if (person is null || person.ClanId != clanId)
        {
            throw new NotFoundException("Không tìm thấy thành viên trong dòng họ này.");
        }

        return await QueryAsync(
            BuildFilter(clanId, query, entityType: "Person", entityId: personId),
            cancellationToken);
    }

    private async Task<PagedResult<AuditLogDto>> QueryAsync(
        AuditLogFilter filter,
        CancellationToken cancellationToken)
    {
        var total = await _repository.AuditLog.CountByClanAsync(filter, cancellationToken);
        var logs = await _repository.AuditLog.GetByClanAsync(filter, trackChanges: false, cancellationToken);
        var items = logs.Select(AuditDiffFormatter.ToDto).ToList();

        return new PagedResult<AuditLogDto>(items, filter.Page, filter.PageSize, total);
    }

    private static AuditLogFilter BuildFilter(
        Guid clanId,
        AuditLogQuery query,
        string? entityType = null,
        Guid? entityId = null)
    {
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize switch
        {
            < 1 => 20,
            > 100 => 100,
            _ => query.PageSize
        };

        return new AuditLogFilter
        {
            ClanId = clanId,
            EntityType = entityType ?? NormalizeEntityType(query.EntityType),
            EntityId = entityId ?? query.EntityId,
            ActorId = query.ActorId,
            From = query.From,
            To = query.To,
            Page = page,
            PageSize = pageSize
        };
    }

    private static string? NormalizeEntityType(string? entityType)
    {
        if (string.IsNullOrWhiteSpace(entityType))
        {
            return null;
        }

        var normalized = entityType.Trim();
        return normalized.ToLowerInvariant() switch
        {
            "person" => "Person",
            "clan" => "Clan",
            "branch" => "Branch",
            _ => normalized
        };
    }
}
