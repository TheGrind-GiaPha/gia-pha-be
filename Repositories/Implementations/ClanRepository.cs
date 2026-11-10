using GiaPha.BusinessObjects.Entities;
using GiaPha.DataAccessObjects.DAOs;
using GiaPha.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GiaPha.Repositories.Implementations;

public sealed class ClanRepository : RepositoryBase<Clan>, IClanRepository
{
    public ClanRepository(IClanDao dao) : base(dao)
    {
    }

    public async Task<IReadOnlyList<Clan>> GetAllAsync(
        string? search,
        bool trackChanges,
        CancellationToken cancellationToken = default)
    {
        var query = FindAll(trackChanges);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var keyword = search.Trim();
            query = query.Where(x => x.Name.Contains(keyword));
        }

        return await query
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public Task<Clan?> GetByIdAsync(
        Guid id,
        bool trackChanges,
        CancellationToken cancellationToken = default)
        => FindByCondition(x => x.Id == id, trackChanges)
            .FirstOrDefaultAsync(cancellationToken);

    public Task<bool> ExistsByNameAsync(
        string name,
        CancellationToken cancellationToken = default)
        => FindByCondition(x => x.Name == name, trackChanges: false)
            .AnyAsync(cancellationToken);
}
