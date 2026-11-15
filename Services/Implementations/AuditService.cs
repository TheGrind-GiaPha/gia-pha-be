using GiaPha.BusinessObjects.DTOs.Changes;
using GiaPha.Repositories.Interfaces;
using GiaPha.Services.Interfaces;

namespace GiaPha.Services.Implementations;

public sealed class AuditService : IAuditService
{
    private readonly IRepositoryManager _repository;

    public AuditService(IRepositoryManager repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<AuditLogDto>> GetByClanAsync(
        Guid clanId,
        CancellationToken cancellationToken = default)
    {
        var logs = await _repository.AuditLog.GetByClanAsync(clanId, cancellationToken);
        return logs
            .Select(l => new AuditLogDto(
                l.Id, l.EntityType, l.Action, l.ActorId, l.ChangeRequestId, l.AfterJson, l.OccurredAt))
            .ToList();
    }
}
