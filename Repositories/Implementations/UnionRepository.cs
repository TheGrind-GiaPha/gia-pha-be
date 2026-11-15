using GiaPha.BusinessObjects.Entities;
using GiaPha.DataAccessObjects.DAOs;
using GiaPha.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GiaPha.Repositories.Implementations;

public sealed class UnionRepository : RepositoryBase<Union>, IUnionRepository
{
    public UnionRepository(IGenericDao<Union> dao) : base(dao)
    {
    }

    public Task<bool> ExistsByHusbandAndRankAsync(
        Guid husbandId,
        int rank,
        CancellationToken cancellationToken = default)
        => FindByCondition(x => x.HusbandId == husbandId && x.Rank == rank, trackChanges: false)
            .AnyAsync(cancellationToken);
}
