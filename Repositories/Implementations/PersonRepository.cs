using GiaPha.BusinessObjects.Entities;
using GiaPha.DataAccessObjects.DAOs;
using GiaPha.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GiaPha.Repositories.Implementations;

public sealed class PersonRepository : RepositoryBase<Person>, IPersonRepository
{
    public PersonRepository(IGenericDao<Person> dao) : base(dao)
    {
    }

    public Task<Person?> GetByIdAsync(
        Guid id,
        bool trackChanges,
        CancellationToken cancellationToken = default)
        => FindByCondition(x => x.Id == id, trackChanges)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<Person>> GetByClanAsync(
        Guid clanId,
        bool trackChanges,
        CancellationToken cancellationToken = default)
        => await FindByCondition(x => x.ClanId == clanId && x.Status == PersonStatus.Active, trackChanges)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Person>> GetByPathPrefixAsync(
        string pathPrefix,
        bool trackChanges,
        CancellationToken cancellationToken = default)
        => await FindByCondition(x => x.Path.StartsWith(pathPrefix) && x.Status == PersonStatus.Active, trackChanges)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Person>> GetByIdsAsync(
        IReadOnlyCollection<Guid> ids,
        bool trackChanges,
        CancellationToken cancellationToken = default)
        => await FindByCondition(x => ids.Contains(x.Id) && x.Status == PersonStatus.Active, trackChanges)
            .ToListAsync(cancellationToken);
}
