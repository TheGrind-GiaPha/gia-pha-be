using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using GiaPha.BusinessObjects.DTOs.Changes;
using GiaPha.BusinessObjects.Entities;
using GiaPha.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GiaPha.APIs.Controllers;

[ApiController]
[Authorize]
[Route("api/clans/{clanId:guid}/change-requests")]
[Produces("application/json")]
public sealed class ChangeRequestsController : ControllerBase
{
    private readonly IChangeRequestService _service;

    public ChangeRequestsController(IChangeRequestService service)
    {
        _service = service;
    }

    private Guid? GetUserId()
    {
        var sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        return sub is not null && Guid.TryParse(sub, out var id) ? id : null;
    }

    /// <summary>Tạo đề xuất thay đổi (Thành viên / Biên tập viên).</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ChangeRequestDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ChangeRequestDto>> Create(
        Guid clanId,
        [FromBody] CreateChangeRequestRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _service.CreateAsync(clanId, userId.Value, request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    /// <summary>Danh sách đề xuất. mine=true chỉ lấy của người đang đăng nhập.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ChangeRequestDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ChangeRequestDto>>> Get(
        Guid clanId,
        [FromQuery] ChangeRequestStatus? status,
        [FromQuery] bool mine = false,
        CancellationToken cancellationToken = default)
    {
        Guid? requesterId = null;
        if (mine)
        {
            var userId = GetUserId();
            if (userId is null)
            {
                return Unauthorized();
            }

            requesterId = userId.Value;
        }

        return Ok(await _service.GetByClanAsync(clanId, status, requesterId, cancellationToken));
    }

    /// <summary>Chi tiết một đề xuất.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ChangeRequestDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ChangeRequestDto>> GetById(
        Guid clanId,
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _service.GetByIdAsync(clanId, id, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Duyệt đề xuất (Người duyệt / Quản trị họ).</summary>
    [HttpPost("{id:guid}/approve")]
    [ProducesResponseType(typeof(ChangeRequestDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ChangeRequestDto>> Approve(
        Guid clanId,
        Guid id,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _service.ApproveAsync(clanId, id, userId.Value, cancellationToken);
        return Ok(result);
    }

    /// <summary>Từ chối đề xuất (bắt buộc lý do).</summary>
    [HttpPost("{id:guid}/reject")]
    [ProducesResponseType(typeof(ChangeRequestDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ChangeRequestDto>> Reject(
        Guid clanId,
        Guid id,
        [FromBody] RejectChangeRequestRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _service.RejectAsync(clanId, id, userId.Value, request.Reason, cancellationToken);
        return Ok(result);
    }
}
