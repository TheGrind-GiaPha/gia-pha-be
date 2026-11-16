namespace GiaPha.BusinessObjects.Entities;

/// <summary>
/// Bản ghi lịch sử bất biến: ai đã làm gì, khi nào, liên quan đề xuất nào.
/// Được ghi tự động bởi AuditSaveChangesInterceptor; ChangeRequest ghi thêm
/// một dòng tóm tắt cùng bảng.
/// </summary>
public sealed class AuditLog : BaseEntity
{
    public Guid ClanId { get; set; }

    public string EntityType { get; set; } = string.Empty;

    public Guid EntityId { get; set; }

    public AuditAction Action { get; set; }

    public Guid? ActorId { get; set; }

    public Guid? ChangeRequestId { get; set; }

    public Guid BatchId { get; set; } = Guid.NewGuid();

    public string? Before { get; set; }

    public string? After { get; set; }

    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
}
