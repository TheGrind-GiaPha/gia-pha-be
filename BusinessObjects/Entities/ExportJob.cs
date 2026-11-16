namespace GiaPha.BusinessObjects.Entities;

/// <summary>
/// Quản lý tiến trình và kết quả xuất tệp PDF phả ký hoặc phả đồ khổ lớn.
/// </summary>
public sealed class ExportJob : BaseEntity
{
    public Guid ClanId { get; set; }

    public ExportType Type { get; set; }

    /// <summary>Id của Chi/Nhánh nếu xuất riêng một chi họ; để trống nếu xuất toàn thể dòng họ.</summary>
    public Guid? BranchId { get; set; }

    public ExportJobStatus Status { get; set; } = ExportJobStatus.Queued;

    public int ProgressPercentage { get; set; }

    public string? FilePath { get; set; }

    public string? FileName { get; set; }

    public long? FileSizeBytes { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTime? FinishedAt { get; set; }
}
