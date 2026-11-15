using GiaPha.BusinessObjects.DTOs.Changes;
using GiaPha.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GiaPha.APIs.Controllers;

[ApiController]
[Authorize]
[Route("api/clans/{clanId:guid}/audit")]
[Produces("application/json")]
public sealed class AuditController : ControllerBase
{
    private readonly IAuditService _auditService;

    public AuditController(IAuditService auditService)
    {
        _auditService = auditService;
    }

    /// <summary>Lịch sử thay đổi của dòng họ (mới nhất trước).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<AuditLogDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AuditLogDto>>> Get(
        Guid clanId,
        CancellationToken cancellationToken)
        => Ok(await _auditService.GetByClanAsync(clanId, cancellationToken));
}
