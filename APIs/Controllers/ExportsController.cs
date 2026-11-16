using GiaPha.BusinessObjects.DTOs.Exports;
using GiaPha.Repositories.Interfaces;
using GiaPha.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GiaPha.APIs.Controllers;

[ApiController]
[Authorize]
[Produces("application/json")]
public sealed class ExportsController : ControllerBase
{
    private readonly IExportService _exportService;
    private readonly IRepositoryManager _repository;

    public ExportsController(
        IExportService exportService,
        IRepositoryManager repository)
    {
        _exportService = exportService;
        _repository = repository;
    }

    /// <summary>Tạo tác vụ xuất Sách Phả Ký hoặc Phả Đồ Khổ Lớn chạy ngầm qua Hangfire (4.9).</summary>
    [HttpPost("api/clans/{clanId:guid}/exports")]
    [ProducesResponseType(typeof(ExportJobDto), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ExportJobDto>> CreateExport(
        Guid clanId,
        [FromBody] CreateExportRequest request,
        CancellationToken cancellationToken)
    {
        var job = await _exportService.CreateExportJobAsync(clanId, request, cancellationToken);
        return AcceptedAtAction(nameof(GetExportStatus), new { id = job.Id }, job);
    }

    /// <summary>Xem trạng thái tiến trình và link tải của một tác vụ xuất file (4.9).</summary>
    [HttpGet("api/exports/{id:guid}")]
    [ProducesResponseType(typeof(ExportJobDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ExportJobDto>> GetExportStatus(
        Guid id,
        CancellationToken cancellationToken)
    {
        var job = await _exportService.GetExportJobAsync(id, cancellationToken);
        return Ok(job);
    }

    /// <summary>Lấy danh sách các bản xuất tài liệu của dòng họ.</summary>
    [HttpGet("api/clans/{clanId:guid}/exports")]
    [ProducesResponseType(typeof(IReadOnlyList<ExportJobDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ExportJobDto>>> GetClanExports(
        Guid clanId,
        CancellationToken cancellationToken)
    {
        var jobs = await _repository.ExportJob.GetByClanAsync(clanId, trackChanges: false, cancellationToken);
        var dtos = jobs.Select(j => new ExportJobDto
        {
            Id = j.Id,
            ClanId = j.ClanId,
            Type = j.Type,
            BranchId = j.BranchId,
            Status = j.Status,
            ProgressPercentage = j.ProgressPercentage,
            FileName = j.FileName,
            FileSizeBytes = j.FileSizeBytes,
            DownloadUrl = j.Status == BusinessObjects.Entities.ExportJobStatus.Completed ? $"/api/exports/{j.Id}/download" : null,
            ErrorMessage = j.ErrorMessage,
            CreatedAt = j.CreatedAt,
            FinishedAt = j.FinishedAt
        }).ToList();

        return Ok(dtos);
    }

    /// <summary>Tải về tệp PDF hoàn chỉnh khi tác vụ đã hoàn thành.</summary>
    [HttpGet("api/exports/{id:guid}/download")]
    [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DownloadFile(
        Guid id,
        CancellationToken cancellationToken)
    {
        var job = await _exportService.GetExportJobAsync(id, cancellationToken);
        if (job.Status != BusinessObjects.Entities.ExportJobStatus.Completed)
        {
            return BadRequest("Tác vụ xuất tệp chưa hoàn tất hoặc đã xảy ra lỗi.");
        }

        var exportEntity = await _repository.ExportJob.GetByIdAsync(id, trackChanges: false, cancellationToken);
        if (exportEntity?.FilePath is null || !System.IO.File.Exists(exportEntity.FilePath))
        {
            return NotFound("Không tìm thấy tệp kết quả trên máy chủ.");
        }

        var stream = System.IO.File.OpenRead(exportEntity.FilePath);
        return File(stream, "application/pdf", job.FileName ?? "GiaPha.pdf");
    }
}
