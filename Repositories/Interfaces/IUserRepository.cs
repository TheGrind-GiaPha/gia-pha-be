using GiaPha.BusinessObjects.Entities;

namespace GiaPha.Repositories.Interfaces;

public interface IUserRepository : IRepositoryBase<User>
{
    Task<User?> GetByIdAsync(
        Guid id,
        bool trackChanges,
        CancellationToken cancellationToken = default);

    Task<User?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByEmailAsync(
        string email,
        CancellationToken cancellationToken = default);
}
