using GiaPha.BusinessObjects.Entities;
using GiaPha.DataAccessObjects.DAOs;
using GiaPha.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GiaPha.Repositories.Implementations;

public sealed class UserRepository : RepositoryBase<User>, IUserRepository
{
    public UserRepository(IGenericDao<User> dao) : base(dao)
    {
    }

    public Task<User?> GetByIdAsync(
        Guid id,
        bool trackChanges,
        CancellationToken cancellationToken = default)
        => FindByCondition(x => x.Id == id, trackChanges)
            .FirstOrDefaultAsync(cancellationToken);

    public Task<User?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
        => FindByCondition(x => x.Email == email, trackChanges: false)
            .FirstOrDefaultAsync(cancellationToken);

    public Task<bool> ExistsByEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
        => FindByCondition(x => x.Email == email, trackChanges: false)
            .AnyAsync(cancellationToken);

    public async Task<IReadOnlyList<User>> GetByIdsAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken = default)
        => await FindByCondition(x => ids.Contains(x.Id), trackChanges: false)
            .ToListAsync(cancellationToken);
}
