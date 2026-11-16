namespace GiaPha.BusinessObjects.DTOs.Audits;

/// <summary>
/// Một dòng lịch sử đã được diễn giải cho UI.
/// Không trả raw JSON để frontend không phải tự đoán nhãn tiếng Việt.
/// </summary>
public sealed record AuditLogDto(
    Guid Id,
    Guid ClanId,
    string EntityType,
    Guid EntityId,
    string Action,
    string ActionLabel,
    Guid? ActorId,
    string ActorLabel,
    Guid? ChangeRequestId,
    Guid BatchId,
    string Summary,
    IReadOnlyList<AuditChangeDto> Changes,
    DateTime OccurredAt);
