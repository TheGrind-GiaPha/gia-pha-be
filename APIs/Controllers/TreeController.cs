using GiaPha.BusinessObjects.DTOs.Persons;
using GiaPha.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GiaPha.APIs.Controllers;

[ApiController]
[Route("api/clans/{clanId:guid}/tree")]
[Produces("application/json")]
public sealed class TreeController : ControllerBase
{
    private readonly IPersonService _personService;

    public TreeController(IPersonService personService)
    {
        _personService = personService;
    }

    /// <summary>Lấy cây gia phả (lazy load theo độ sâu).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PersonNodeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PersonNodeDto>> GetTree(
        Guid clanId,
        [FromQuery] Guid? rootId,
        [FromQuery] int depth = 3,
        CancellationToken cancellationToken = default)
    {
        var tree = await _personService.GetTreeAsync(clanId, rootId, depth, cancellationToken);
        return tree is null ? NotFound() : Ok(tree);
    }
}
