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
    private readonly IWebHostEnvironment _environment;

    public PersonsController(IPersonService personService, IWebHostEnvironment environment)
    {
        _personService = personService;
        _environment = environment;
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

    /// <summary>Di chuyển cây con (một người + toàn bộ hậu duệ) sang người cha mới.</summary>
    [HttpPost("{personId:guid}/move")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> MoveSubtree(
        Guid clanId,
        Guid personId,
        [FromBody] MoveSubtreeRequest request,
        CancellationToken cancellationToken)
    {
        await _personService.MoveSubtreeAsync(clanId, personId, request.NewFatherId, cancellationToken);
        return NoContent();
    }

    /// <summary>Gộp 2 người trùng (giữ keepId, gộp removeId vào).</summary>
    [HttpPost("{keepId:guid}/merge/{removeId:guid}")]
    [ProducesResponseType(typeof(PersonDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PersonDto>> MergePersons(
        Guid clanId,
        Guid keepId,
        Guid removeId,
        CancellationToken cancellationToken)
    {
        var merged = await _personService.MergePersonsAsync(clanId, keepId, removeId, cancellationToken);
        return Ok(merged);
    }

    /// <summary>Ghép cây con (di chuyển + phát hiện trùng tên/năm sinh).</summary>
    [HttpPost("{rootId:guid}/graft")]
    [ProducesResponseType(typeof(GraftResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<GraftResultDto>> Graft(
        Guid clanId,
        Guid rootId,
        [FromBody] MoveSubtreeRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _personService.GraftAsync(clanId, rootId, request.NewFatherId, cancellationToken);
        return Ok(result);
    }

    /// <summary>Xóa người (soft-delete) theo chiến lược block/cascade/reparent.</summary>
    [HttpDelete("{personId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(
        Guid clanId,
        Guid personId,
        [FromQuery] string strategy = "block",
        [FromQuery] Guid? newFatherId = null,
        CancellationToken cancellationToken = default)
    {
        await _personService.DeletePersonAsync(clanId, personId, strategy, newFatherId, cancellationToken);
        return NoContent();
    }

    /// <summary>Lấy danh sách tổ tiên của một người.</summary>
    [HttpGet("{personId:guid}/ancestors")]
    [ProducesResponseType(typeof(IReadOnlyList<PersonDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<PersonDto>>> GetAncestors(
        Guid clanId,
        Guid personId,
        CancellationToken cancellationToken)
    {
        var ancestors = await _personService.GetAncestorsAsync(clanId, personId, cancellationToken);
        return Ok(ancestors);
    }

    /// <summary>Lấy danh sách hậu duệ của một người.</summary>
    [HttpGet("{personId:guid}/descendants")]
    [ProducesResponseType(typeof(IReadOnlyList<PersonDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<PersonDto>>> GetDescendants(
        Guid clanId,
        Guid personId,
        CancellationToken cancellationToken)
    {
        var descendants = await _personService.GetDescendantsAsync(clanId, personId, cancellationToken);
        return Ok(descendants);
    }

    /// <summary>Upload ảnh chân dung (jpg/png, tối đa 5MB) cho một người.</summary>
    [HttpPost("{personId:guid}/portrait")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<string>> UploadPortrait(
        Guid clanId,
        Guid personId,
        IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest("File ảnh không được để trống.");
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (extension != ".jpg" && extension != ".jpeg" && extension != ".png")
        {
            return BadRequest("Chỉ chấp nhận ảnh .jpg hoặc .png.");
        }

        if (file.Length > 5 * 1024 * 1024)
        {
            return BadRequest("Ảnh tối đa 5MB.");
        }

        var webRoot = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
        var folder = Path.Combine(webRoot, "portraits");
        Directory.CreateDirectory(folder);

        var fileName = $"{personId}{extension}";
        var filePath = Path.Combine(folder, fileName);

        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        var url = $"/portraits/{fileName}";
        await _personService.SetPortraitAsync(clanId, personId, url, cancellationToken);

        return Ok(url);
    }
}
