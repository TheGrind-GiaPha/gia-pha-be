using GiaPha.BusinessObjects.DTOs.Audits;
using GiaPha.BusinessObjects.DTOs.Common;
using GiaPha.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GiaPha.APIs.Controllers;

[ApiController]
[Route("api/clans/{clanId:guid}")]
[Produces("application/json")]
public sealed class AuditController : ControllerBase
{
    private readonly IAuditLogService _auditLogService;

    public AuditController(IAuditLogService auditLogService)
    {
        _auditLogService = auditLogService;
    }

    /// <summary>Lịch sử thay đổi của một dòng họ, có lọc và phân trang.</summary>
    [HttpGet("audit")]
    [ProducesResponseType(typeof(PagedResult<AuditLogDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PagedResult<AuditLogDto>>> GetAudit(
        Guid clanId,
        [FromQuery] AuditLogQuery query,
        CancellationToken cancellationToken)
        => Ok(await _auditLogService.GetByClanAsync(clanId, query, cancellationToken));

    /// <summary>Lịch sử thay đổi của một thành viên trong dòng họ.</summary>
    [HttpGet("persons/{personId:guid}/history")]
    [ProducesResponseType(typeof(PagedResult<AuditLogDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PagedResult<AuditLogDto>>> GetPersonHistory(
        Guid clanId,
        Guid personId,
        [FromQuery] AuditLogQuery query,
        CancellationToken cancellationToken)
        => Ok(await _auditLogService.GetPersonHistoryAsync(clanId, personId, query, cancellationToken));
}
