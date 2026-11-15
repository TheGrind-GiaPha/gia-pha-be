using GiaPha.BusinessObjects.DTOs.Public;
using GiaPha.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GiaPha.APIs.Controllers;

/// <summary>API công khai: khách (không đăng nhập) xem các dòng họ đã bật Public.</summary>
[ApiController]
[Route("api/public/clans")]
[Produces("application/json")]
public sealed class PublicClansController : ControllerBase
{
    private readonly IPublicClanService _publicClanService;

    public PublicClansController(IPublicClanService publicClanService)
    {
        _publicClanService = publicClanService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<PublicClanDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PublicClanDto>>> GetClans(
        CancellationToken cancellationToken)
        => Ok(await _publicClanService.GetPublicClansAsync(cancellationToken));

    [HttpGet("{clanId:guid}/tree")]
    [ProducesResponseType(typeof(PublicPersonNodeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PublicPersonNodeDto>> GetTree(
        Guid clanId,
        [FromQuery] int depth = 20,
        CancellationToken cancellationToken = default)
    {
        var tree = await _publicClanService.GetPublicTreeAsync(clanId, depth, cancellationToken);
        return tree is null ? NotFound() : Ok(tree);
    }
}
