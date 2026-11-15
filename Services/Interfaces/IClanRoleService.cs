using GiaPha.BusinessObjects.DTOs.Clans;
using GiaPha.BusinessObjects.Entities;

namespace GiaPha.Services.Interfaces;

public interface IClanRoleService
{
    Task<IReadOnlyList<ClanRoleAssignmentDto>> GetByClanAsync(
        Guid clanId,
        CancellationToken cancellationToken = default);

    Task<ClanRoleAssignmentDto> AssignAsync(
        Guid clanId,
        AssignRoleRequest request,
        CancellationToken cancellationToken = default);

    Task RevokeAsync(
        Guid clanId,
        Guid userId,
        ClanRole role,
        CancellationToken cancellationToken = default);

    Task<bool> HasRoleAsync(
        Guid userId,
        Guid clanId,
        ClanRole role,
        CancellationToken cancellationToken = default);

    /// <summary>Bảo đảm người tạo dòng họ có role ClanAdmin (bootstrap).</summary>
    Task EnsureClanAdminAsync(
        Guid clanId,
        Guid userId,
        CancellationToken cancellationToken = default);
}
