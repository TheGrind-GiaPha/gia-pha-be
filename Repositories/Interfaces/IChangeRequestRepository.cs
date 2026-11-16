using GiaPha.BusinessObjects.Entities;

namespace GiaPha.Repositories.Interfaces;

public interface IChangeRequestRepository : IRepositoryBase<ChangeRequest>
{
    Task<ChangeRequest?> GetByIdAsync(
        Guid id,
        bool trackChanges,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ChangeRequest>> GetByClanAsync(
        Guid clanId,
        ChangeRequestStatus? status,
        Guid? requesterId,
        bool trackChanges,
        CancellationToken cancellationToken = default);
}
