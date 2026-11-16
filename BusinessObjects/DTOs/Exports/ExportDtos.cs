using GiaPha.BusinessObjects.Entities;

namespace GiaPha.BusinessObjects.DTOs.Exports;

public sealed class CreateExportRequest
{
    public ExportType Type { get; set; } = ExportType.Book;
    public Guid? BranchId { get; set; }
    public string? PageSize { get; set; } = "A4"; // A4, A3, A1, A0
}

public sealed class ExportJobDto
{
    public Guid Id { get; set; }
    public Guid ClanId { get; set; }
    public ExportType Type { get; set; }
    public Guid? BranchId { get; set; }
    public ExportJobStatus Status { get; set; }
    public int ProgressPercentage { get; set; }
    public string? FileName { get; set; }
    public long? FileSizeBytes { get; set; }
    public string? DownloadUrl { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
}
