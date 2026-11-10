using GiaPha.BusinessObjects.Entities;

namespace GiaPha.Repositories.Interfaces;

public interface IClanRepository : IRepositoryBase<Clan>
{
    Task<IReadOnlyList<Clan>> GetAllAsync(
        string? search,
        bool trackChanges,
        CancellationToken cancellationToken = default);

    Task<Clan?> GetByIdAsync(
        Guid id,
        bool trackChanges,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByNameAsync(
        string name,
        CancellationToken cancellationToken = default);
}
