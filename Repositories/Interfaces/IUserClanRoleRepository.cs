using GiaPha.BusinessObjects.Entities;

namespace GiaPha.Repositories.Interfaces;

public interface IUserClanRoleRepository : IRepositoryBase<UserClanRole>
{
    Task<IReadOnlyList<UserClanRole>> GetByClanAsync(
        Guid clanId,
        CancellationToken cancellationToken = default);

    Task<UserClanRole?> GetAsync(
        Guid userId,
        Guid clanId,
        ClanRole role,
        CancellationToken cancellationToken = default);

    Task<bool> HasRoleAsync(
        Guid userId,
        Guid clanId,
        ClanRole role,
        CancellationToken cancellationToken = default);
}
