namespace GiaPha.BusinessObjects.Entities;

/// <summary>
/// Đề xuất thay đổi cây gia phả khi dòng họ đã ở trạng thái Published (yêu cầu phê duyệt).
/// </summary>
public sealed class ChangeRequest : BaseEntity
{
    public Guid ClanId { get; set; }

    /// <summary>Loại thay đổi (ví dụ: "ImportBatch", "AddPerson", "MoveSubtree"...).</summary>
    public string Type { get; set; } = string.Empty;

    public ChangeRequestStatus Status { get; set; } = ChangeRequestStatus.Pending;

    /// <summary>Dữ liệu JSON chi tiết của thay đổi để handler áp dụng khi được duyệt.</summary>
    public string Payload { get; set; } = "{}";

    public Guid CreatedByUserId { get; set; }

    public Guid? ReviewedByUserId { get; set; }

    public string? ReviewNote { get; set; }

    public DateTime? ReviewedAt { get; set; }
}
