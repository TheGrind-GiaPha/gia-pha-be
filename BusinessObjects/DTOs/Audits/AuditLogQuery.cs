namespace GiaPha.BusinessObjects.DTOs.Audits;

/// <summary>Query string cho API lịch sử thay đổi.</summary>
public sealed class AuditLogQuery
{
    public string? EntityType { get; init; }

    public Guid? EntityId { get; init; }

    public Guid? ActorId { get; init; }

    public DateTime? From { get; init; }

    public DateTime? To { get; init; }

    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 20;
}
