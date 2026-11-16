namespace GiaPha.BusinessObjects.Entities;

/// <summary>
/// Lô nhập liệu: quản lý một phiên tải lên tệp Excel hoặc Word để đưa vào cây gia phả có kiểm soát.
/// </summary>
public sealed class ImportBatch : BaseEntity
{
    public Guid ClanId { get; set; }

    public ImportSource Source { get; set; }

    public ImportBatchStatus Status { get; set; } = ImportBatchStatus.Uploaded;

    public string FileName { get; set; } = string.Empty;

    public int TotalRows { get; set; }

    public int AcceptedRowsCount { get; set; }

    /// <summary>Người dùng đại diện dòng họ đã xác nhận đồng ý gửi dữ liệu ra ngoài (cho LLM).</summary>
    public bool ConsentGiven { get; set; }

    public Guid CreatedByUserId { get; set; }

    // Navigation
    public ICollection<ImportRow> Rows { get; set; } = new List<ImportRow>();
}
