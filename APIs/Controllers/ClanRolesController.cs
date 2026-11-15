using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using GiaPha.BusinessObjects.DTOs.Clans;
using GiaPha.BusinessObjects.Entities;
using GiaPha.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GiaPha.APIs.Controllers;

[ApiController]
[Authorize]
[Route("api/clans/{clanId:guid}/roles")]
[Produces("application/json")]
public sealed class ClanRolesController : ControllerBase
{
    private readonly IClanRoleService _roleService;

    public ClanRolesController(IClanRoleService roleService)
    {
        _roleService = roleService;
    }

    /// <summary>Danh sách phân quyền của dòng họ.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ClanRoleAssignmentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ClanRoleAssignmentDto>>> Get(
        Guid clanId,
        CancellationToken cancellationToken)
        => Ok(await _roleService.GetByClanAsync(clanId, cancellationToken));

    /// <summary>Gán vai trò cho một người dùng (chỉ ClanAdmin).</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ClanRoleAssignmentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClanRoleAssignmentDto>> Assign(
        Guid clanId,
        [FromBody] AssignRoleRequest request,
        CancellationToken cancellationToken)
    {
        var currentUserId = GetUserId();
        if (currentUserId is null)
        {
            return Unauthorized();
        }

        if (!await _roleService.HasRoleAsync(currentUserId.Value, clanId, ClanRole.ClanAdmin, cancellationToken))
        {
            return Forbid();
        }

        var result = await _roleService.AssignAsync(clanId, request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    /// <summary>Thu hồi vai trò (chỉ ClanAdmin).</summary>
    [HttpDelete("{userId:guid}/{role}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Revoke(
        Guid clanId,
        Guid userId,
        ClanRole role,
        CancellationToken cancellationToken)
    {
        var currentUserId = GetUserId();
        if (currentUserId is null)
        {
            return Unauthorized();
        }

        if (!await _roleService.HasRoleAsync(currentUserId.Value, clanId, ClanRole.ClanAdmin, cancellationToken))
        {
            return Forbid();
        }

        await _roleService.RevokeAsync(clanId, userId, role, cancellationToken);
        return NoContent();
    }

    private Guid? GetUserId()
    {
        var sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        return sub is not null && Guid.TryParse(sub, out var id) ? id : null;
    }
}
