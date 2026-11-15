using GiaPha.BusinessObjects.DTOs.Persons;
using GiaPha.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GiaPha.APIs.Controllers;

[ApiController]
[Route("api/clans/{clanId:guid}/persons")]
[Produces("application/json")]
public sealed class PersonsController : ControllerBase
{
    private readonly IPersonService _personService;

    public PersonsController(IPersonService personService)
    {
        _personService = personService;
    }

    /// <summary>Thêm một hoặc nhiều người con cho một người cha.</summary>
    [HttpPost("{fatherId:guid}/children")]
    [ProducesResponseType(typeof(IReadOnlyList<PersonDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<PersonDto>>> AddChildren(
        Guid clanId,
        Guid fatherId,
        [FromBody] AddChildrenRequest request,
        CancellationToken cancellationToken)
    {
        var children = await _personService.AddChildrenAsync(clanId, fatherId, request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, children);
    }

    /// <summary>Thêm quan hệ vợ chồng cho một người chồng.</summary>
    [HttpPost("{husbandId:guid}/unions")]
    [ProducesResponseType(typeof(UnionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UnionDto>> AddUnion(
        Guid clanId,
        Guid husbandId,
        [FromBody] AddUnionRequest request,
        CancellationToken cancellationToken)
    {
        var union = await _personService.AddUnionAsync(clanId, husbandId, request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, union);
    }
}
