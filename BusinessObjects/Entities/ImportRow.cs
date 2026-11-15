namespace GiaPha.BusinessObjects.Entities;

/// <summary>
/// Dòng dữ liệu staging trong lô nhập liệu.
/// Dữ liệu import không bao giờ ghi thẳng vào Person cho đến khi được review và commit.
/// </summary>
public sealed class ImportRow : BaseEntity
{
    public Guid BatchId { get; set; }

    public int RowIndex { get; set; }

    /// <summary>Dữ liệu JSON thô đã bóc tách từ Excel hoặc LLM.</summary>
    public string ExtractedJson { get; set; } = "{}";

    public ImportRowStatus Status { get; set; } = ImportRowStatus.New;

    /// <summary>Id của Person bị nghi trùng trong cây gia phả hiện tại.</summary>
    public Guid? MatchedPersonId { get; set; }

    /// <summary>Cảnh báo lỗi định dạng hoặc chi tiết nghi trùng.</summary>
    public string? Warnings { get; set; }

    // Navigation
    public ImportBatch? Batch { get; set; }
}
