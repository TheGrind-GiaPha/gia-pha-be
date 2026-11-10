using GiaPha.BusinessObjects.DTOs.Clans;
using GiaPha.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GiaPha.APIs.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public sealed class ClansController : ControllerBase
{
    private readonly IClanService _clanService;

    public ClansController(IClanService clanService)
    {
        _clanService = clanService;
    }

    /// <summary>Lấy danh sách dòng họ, có thể tìm theo tên.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ClanDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ClanDto>>> GetAll(
        [FromQuery] string? search,
        CancellationToken cancellationToken)
        => Ok(await _clanService.GetAllAsync(search, cancellationToken));

    /// <summary>Lấy chi tiết một dòng họ.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ClanDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClanDto>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var clan = await _clanService.GetByIdAsync(id, cancellationToken);
        return clan is null ? NotFound() : Ok(clan);
    }

    /// <summary>Tạo dòng họ mới.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ClanDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ClanDto>> Create(
        [FromBody] CreateClanRequest request,
        CancellationToken cancellationToken)
    {
        var clan = await _clanService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = clan.Id }, clan);
    }

    /// <summary>Cập nhật dòng họ.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ClanDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ClanDto>> Update(
        Guid id,
        [FromBody] UpdateClanRequest request,
        CancellationToken cancellationToken)
    {
        var clan = await _clanService.UpdateAsync(id, request, cancellationToken);
        return clan is null ? NotFound() : Ok(clan);
    }

    /// <summary>Xóa dòng họ.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
        => await _clanService.DeleteAsync(id, cancellationToken)
            ? NoContent()
            : NotFound();
}
