using GiaPha.BusinessObjects.Entities;
using GiaPha.DataAccessObjects.DAOs;
using GiaPha.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GiaPha.Repositories.Implementations;

public sealed class UserClanRoleRepository : RepositoryBase<UserClanRole>, IUserClanRoleRepository
{
    public UserClanRoleRepository(IGenericDao<UserClanRole> dao) : base(dao)
    {
    }

    public async Task<IReadOnlyList<UserClanRole>> GetByClanAsync(
        Guid clanId,
        CancellationToken cancellationToken = default)
        => await FindByCondition(x => x.ClanId == clanId, trackChanges: false)
            .ToListAsync(cancellationToken);

    public Task<UserClanRole?> GetAsync(
        Guid userId,
        Guid clanId,
        ClanRole role,
        CancellationToken cancellationToken = default)
        => FindByCondition(
                x => x.UserId == userId && x.ClanId == clanId && x.Role == role,
                trackChanges: false)
            .FirstOrDefaultAsync(cancellationToken);

    public Task<bool> HasRoleAsync(
        Guid userId,
        Guid clanId,
        ClanRole role,
        CancellationToken cancellationToken = default)
        => FindByCondition(
                x => x.UserId == userId && x.ClanId == clanId && x.Role == role,
                trackChanges: false)
            .AnyAsync(cancellationToken);
}
