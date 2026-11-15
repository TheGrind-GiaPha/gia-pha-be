using GiaPha.BusinessObjects.Entities;

namespace GiaPha.Repositories.Interfaces;

public interface IPersonRepository : IRepositoryBase<Person>
{
    Task<Person?> GetByIdAsync(
        Guid id,
        bool trackChanges,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Person>> GetByClanAsync(
        Guid clanId,
        bool trackChanges,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Person>> GetByPathPrefixAsync(
        string pathPrefix,
        bool trackChanges,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Person>> GetByIdsAsync(
        IReadOnlyCollection<Guid> ids,
        bool trackChanges,
        CancellationToken cancellationToken = default);
}
