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

    public Task<ParentChild?> GetByChildIdAsync(
        Guid childId,
        RelationType relationType,
        CancellationToken cancellationToken = default)
        => FindByCondition(x => x.ChildId == childId && x.RelationType == relationType, trackChanges: true)
            .FirstOrDefaultAsync(cancellationToken);

    public Task<bool> ExistsByParentAsync(
        Guid parentId,
        CancellationToken cancellationToken = default)
        => FindByCondition(x => x.ParentId == parentId, trackChanges: false)
            .AnyAsync(cancellationToken);

    public async Task<IReadOnlyList<ParentChild>> GetByParentIdsAsync(
        IReadOnlyCollection<Guid> parentIds,
        RelationType relationType,
        CancellationToken cancellationToken = default)
        => await FindByCondition(x => parentIds.Contains(x.ParentId) && x.RelationType == relationType, trackChanges: false)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<ParentChild>> GetAllByChildIdAsync(
        Guid childId,
        CancellationToken cancellationToken = default)
        => await FindByCondition(x => x.ChildId == childId, trackChanges: true)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<ParentChild>> GetByChildIdsAsync(
        IReadOnlyCollection<Guid> childIds,
        bool trackChanges,
        CancellationToken cancellationToken = default)
        => await FindByCondition(x => childIds.Contains(x.ChildId), trackChanges)
            .ToListAsync(cancellationToken);
}
