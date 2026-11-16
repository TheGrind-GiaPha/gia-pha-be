namespace GiaPha.BusinessObjects.Entities;

/// <summary>
/// Đề xuất thay đổi dữ liệu gia phả. Sau khi họ đã công bố, mọi thay đổi
/// đều đi qua đây: Command → ChangeRequest → Approver duyệt → Apply → AuditLog.
/// </summary>
public sealed class ChangeRequest : BaseEntity
{
    public Guid ClanId { get; set; }

    public Guid? BranchId { get; set; }

    public Guid RequesterId { get; set; }

    /// <summary>Loại thay đổi, khớp với IChangeHandler.Type (AddPerson, MoveSubtree...).</summary>
    public string Type { get; set; } = string.Empty;

    public string PayloadJson { get; set; } = string.Empty;

    /// <summary>Tóm tắt bằng tiếng Việt cho người lớn tuổi đọc.</summary>
    public string Summary { get; set; } = string.Empty;

    public ChangeRequestStatus Status { get; set; } = ChangeRequestStatus.Pending;

    public Guid? ReviewerId { get; set; }

    public string? RejectReason { get; set; }

    public DateTime DueAt { get; set; }
}
