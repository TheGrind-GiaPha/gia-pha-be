using GiaPha.BusinessObjects.Entities;

namespace GiaPha.Repositories.Interfaces;

public interface IParentChildRepository : IRepositoryBase<ParentChild>
{
    Task<int> GetMaxSiblingOrderAsync(
        Guid parentId,
        CancellationToken cancellationToken = default);

    Task<ParentChild?> GetByChildIdAsync(
        Guid childId,
        RelationType relationType,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByParentAsync(
        Guid parentId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ParentChild>> GetByParentIdsAsync(
        IReadOnlyCollection<Guid> parentIds,
        RelationType relationType,
        CancellationToken cancellationToken = default);
}
