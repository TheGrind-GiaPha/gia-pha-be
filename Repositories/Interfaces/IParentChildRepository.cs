using GiaPha.BusinessObjects.Entities;

namespace GiaPha.Repositories.Interfaces;

public interface IParentChildRepository : IRepositoryBase<ParentChild>
{
    Task<int> GetMaxSiblingOrderAsync(
        Guid parentId,
        CancellationToken cancellationToken = default);
}
