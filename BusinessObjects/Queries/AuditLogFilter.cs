namespace GiaPha.BusinessObjects.Queries;

/// <summary>
/// Bộ lọc lịch sử thay đổi. Repository chỉ nhận filter đã chuẩn hóa,
/// không nhận DTO/query string trực tiếp từ controller.
/// </summary>
public sealed class AuditLogFilter
{
    public Guid ClanId { get; init; }

    public string? EntityType { get; init; }

    public Guid? EntityId { get; init; }

    public Guid? ActorId { get; init; }

    public DateTime? From { get; init; }

    public DateTime? To { get; init; }

    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 20;
}
