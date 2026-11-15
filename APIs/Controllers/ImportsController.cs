using System.Security.Claims;
using GiaPha.BusinessObjects.DTOs.Imports;
using GiaPha.BusinessObjects.Entities;
using GiaPha.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GiaPha.APIs.Controllers;

[ApiController]
[Authorize]
[Route("api/clans/{clanId:guid}")]
[Produces("application/json")]
public sealed class ImportsController : ControllerBase
{
    private readonly IImportService _importService;
    private readonly IExcelImportService _excelImportService;

    public ImportsController(
        IImportService importService,
        IExcelImportService excelImportService)
    {
        _importService = importService;
        _excelImportService = excelImportService;
    }

    /// <summary>Tải về tệp Excel mẫu 10 cột để nhập liệu.</summary>
    [HttpGet("imports/template")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public IActionResult DownloadTemplate()
    {
        var bytes = _excelImportService.GenerateTemplate();
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Genealogy_Import_Template.xlsx");
    }

    /// <summary>Tải lên và phân tích tệp Excel vào bảng Staging.</summary>
    [HttpPost("imports/excel")]
    [ProducesResponseType(typeof(ImportBatchDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ImportBatchDto>> UploadExcel(
        Guid clanId,
        IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest("Vui lòng đính kèm tệp Excel (.xlsx).");
        }

        var userId = GetCurrentUserId();
        using var stream = file.OpenReadStream();
        var result = await _importService.UploadExcelAsync(clanId, stream, file.FileName, userId, cancellationToken);
        return Ok(result);
    }

    /// <summary>Tải lên và bóc tách tệp Word bằng Gemini LLM vào bảng Staging.</summary>
    [HttpPost("imports/word")]
    [ProducesResponseType(typeof(ImportBatchDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ImportBatchDto>> UploadWord(
        Guid clanId,
        IFormFile file,
        [FromForm] bool consentGiven,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest("Vui lòng đính kèm tệp Word (.docx).");
        }

        if (!consentGiven)
        {
            return BadRequest("Bạn phải tích chọn đồng ý điều khoản bảo mật trước khi gửi nội dung sang AI trích xuất.");
        }

        var userId = GetCurrentUserId();
        using var stream = file.OpenReadStream();
        var result = await _importService.UploadWordAsync(clanId, stream, file.FileName, consentGiven, userId, cancellationToken);
        return Ok(result);
    }

    /// <summary>Xem chi tiết một lô nhập liệu cùng toàn bộ dòng staging.</summary>
    [HttpGet("imports/{batchId:guid}")]
    [ProducesResponseType(typeof(ImportBatchDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ImportBatchDto>> GetBatch(
        Guid clanId,
        Guid batchId,
        CancellationToken cancellationToken)
    {
        var batch = await _importService.GetBatchAsync(clanId, batchId, cancellationToken);
        return Ok(batch);
    }

    /// <summary>Lấy danh sách các lô nhập liệu của dòng họ.</summary>
    [HttpGet("imports")]
    [ProducesResponseType(typeof(IReadOnlyList<ImportBatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ImportBatchDto>>> GetBatches(
        Guid clanId,
        CancellationToken cancellationToken)
    {
        var batches = await _importService.GetBatchesByClanAsync(clanId, cancellationToken);
        return Ok(batches);
    }

    /// <summary>Chỉnh sửa trực tiếp dữ liệu một dòng staging trước khi commit.</summary>
    [HttpPut("imports/{batchId:guid}/rows/{rowId:guid}")]
    [ProducesResponseType(typeof(ImportRowDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ImportRowDto>> UpdateRow(
        Guid clanId,
        Guid batchId,
        Guid rowId,
        [FromBody] UpdateImportRowRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await _importService.UpdateRowAsync(
            clanId,
            batchId,
            rowId,
            request.ExtractedJson,
            request.Status,
            cancellationToken);

        return Ok(updated);
    }

    /// <summary>Phê duyệt và commit các dòng được chọn vào cây gia phả (4.6).</summary>
    [HttpPost("import-batches/{id:guid}/commit")]
    [HttpPost("imports/{id:guid}/commit")]
    [ProducesResponseType(typeof(CommitImportResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CommitImportResultDto>> CommitBatch(
        Guid clanId,
        Guid id,
        [FromBody] CommitImportBatchRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await _importService.CommitBatchAsync(clanId, id, request.AcceptedRowIds, userId, cancellationToken);
        return Ok(result);
    }

    private Guid GetCurrentUserId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        return Guid.TryParse(sub, out var userId) ? userId : Guid.Empty;
    }
}

public sealed class UpdateImportRowRequest
{
    public string ExtractedJson { get; set; } = "{}";
    public ImportRowStatus Status { get; set; } = ImportRowStatus.New;
}
