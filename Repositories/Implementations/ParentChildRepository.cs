using GiaPha.BusinessObjects.Entities;
using GiaPha.DataAccessObjects.DAOs;
using GiaPha.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GiaPha.Repositories.Implementations;

public sealed class ParentChildRepository : RepositoryBase<ParentChild>, IParentChildRepository
{
    public ParentChildRepository(IGenericDao<ParentChild> dao) : base(dao)
    {
    }

    public async Task<int> GetMaxSiblingOrderAsync(
        Guid parentId,
        CancellationToken cancellationToken = default)
    {
        var max = await FindByCondition(x => x.ParentId == parentId, trackChanges: false)
            .MaxAsync(x => (int?)x.SiblingOrder, cancellationToken);

        return max ?? 0;
    }
}
