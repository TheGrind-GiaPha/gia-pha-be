using System.ComponentModel.DataAnnotations;
using GiaPha.BusinessObjects.Entities;

namespace GiaPha.BusinessObjects.DTOs.Clans;

public sealed record ClanRoleAssignmentDto(
    Guid Id,
    Guid UserId,
    string Email,
    string DisplayName,
    Guid ClanId,
    Guid? BranchId,
    ClanRole Role);

public sealed class AssignRoleRequest
{
    [Required(ErrorMessage = "Email là bắt buộc.")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
    public string Email { get; init; } = string.Empty;

    public ClanRole Role { get; init; } = ClanRole.Member;

    public Guid? BranchId { get; init; }
}
