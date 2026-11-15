using GiaPha.BusinessObjects.Entities;
using GiaPha.DataAccessObjects.DAOs;
using GiaPha.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GiaPha.Repositories.Implementations;

public sealed class ChangeRequestRepository : RepositoryBase<ChangeRequest>, IChangeRequestRepository
{
    public ChangeRequestRepository(IGenericDao<ChangeRequest> dao) : base(dao)
    {
    }

    public Task<ChangeRequest?> GetByIdAsync(
        Guid id,
        bool trackChanges,
        CancellationToken cancellationToken = default)
        => FindByCondition(x => x.Id == id, trackChanges)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<ChangeRequest>> GetByClanAsync(
        Guid clanId,
        ChangeRequestStatus? status,
        Guid? requesterId,
        bool trackChanges,
        CancellationToken cancellationToken = default)
    {
        var query = FindByCondition(x => x.ClanId == clanId, trackChanges);

        if (status is not null)
        {
            query = query.Where(x => x.Status == status);
        }

        if (requesterId is not null)
        {
            query = query.Where(x => x.RequesterId == requesterId);
        }

        return await query
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
