using GiaPha.BusinessObjects.DTOs.Clans;
using GiaPha.BusinessObjects.Entities;
using GiaPha.BusinessObjects.Exceptions;
using GiaPha.Repositories.Interfaces;
using GiaPha.Services.Interfaces;
using GiaPha.Services.Logging;

namespace GiaPha.Services.Implementations;

public sealed class ClanRoleService : IClanRoleService
{
    private readonly IRepositoryManager _repository;
    private readonly ILoggerManager _logger;

    public ClanRoleService(IRepositoryManager repository, ILoggerManager logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<IReadOnlyList<ClanRoleAssignmentDto>> GetByClanAsync(
        Guid clanId,
        CancellationToken cancellationToken = default)
    {
        var roles = await _repository.UserClanRole.GetByClanAsync(clanId, cancellationToken);
        var userIds = roles.Select(r => r.UserId).Distinct().ToList();
        var users = await _repository.User.GetByIdsAsync(userIds, cancellationToken);
        var byId = users.ToDictionary(u => u.Id);

        return roles
            .Select(r =>
            {
                var user = byId.GetValueOrDefault(r.UserId);
                return new ClanRoleAssignmentDto(
                    r.Id,
                    r.UserId,
                    user?.Email ?? string.Empty,
                    user?.DisplayName ?? string.Empty,
                    r.ClanId,
                    r.BranchId,
                    r.Role);
            })
            .ToList();
    }

    public async Task<ClanRoleAssignmentDto> AssignAsync(
        Guid clanId,
        AssignRoleRequest request,
        CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _repository.User.GetByEmailAsync(email, cancellationToken)
            ?? throw new NotFoundException("Không tìm thấy người dùng với email này.");

        var existing = await _repository.UserClanRole.GetAsync(user.Id, clanId, request.Role, cancellationToken);
        if (existing is not null)
        {
            existing.BranchId = request.BranchId;
            existing.UpdatedAt = DateTime.UtcNow;
            _repository.UserClanRole.Update(existing);
            await _repository.SaveAsync(cancellationToken);
            return Map(existing, user);
        }

        var assignment = new UserClanRole
        {
            UserId = user.Id,
            ClanId = clanId,
            BranchId = request.BranchId,
            Role = request.Role
        };

        _repository.UserClanRole.Create(assignment);
        await _repository.SaveAsync(cancellationToken);

        _logger.LogInfo($"Đã gán role {request.Role} cho {email} trong clan {clanId}.");
        return Map(assignment, user);
    }

    public async Task RevokeAsync(
        Guid clanId,
        Guid userId,
        ClanRole role,
        CancellationToken cancellationToken = default)
    {
        var assignment = await _repository.UserClanRole.GetAsync(userId, clanId, role, cancellationToken)
            ?? throw new NotFoundException("Không tìm thấy phân quyền cần thu hồi.");

        _repository.UserClanRole.Delete(assignment);
        await _repository.SaveAsync(cancellationToken);
        _logger.LogInfo($"Đã thu hồi role {role} của user {userId} trong clan {clanId}.");
    }

    public Task<bool> HasRoleAsync(
        Guid userId,
        Guid clanId,
        ClanRole role,
        CancellationToken cancellationToken = default)
        => _repository.UserClanRole.HasRoleAsync(userId, clanId, role, cancellationToken);

    public async Task EnsureClanAdminAsync(
        Guid clanId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        if (await _repository.UserClanRole.HasRoleAsync(userId, clanId, ClanRole.ClanAdmin, cancellationToken))
        {
            return;
        }

        _repository.UserClanRole.Create(new UserClanRole
        {
            UserId = userId,
            ClanId = clanId,
            Role = ClanRole.ClanAdmin
        });

        await _repository.SaveAsync(cancellationToken);
    }

    private static ClanRoleAssignmentDto Map(UserClanRole role, User user)
        => new(role.Id, role.UserId, user.Email, user.DisplayName, role.ClanId, role.BranchId, role.Role);
}
