using GiaPha.BusinessObjects.Entities;

namespace GiaPha.Repositories.Interfaces;

public interface IUnionRepository : IRepositoryBase<Union>
{
    Task<bool> ExistsByHusbandAndRankAsync(
        Guid husbandId,
        int rank,
        CancellationToken cancellationToken = default);
}
