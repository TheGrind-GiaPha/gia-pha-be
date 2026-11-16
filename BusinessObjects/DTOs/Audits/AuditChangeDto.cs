namespace GiaPha.BusinessObjects.DTOs.Audits;

/// <summary>Một field thay đổi đã được diễn giải sang tiếng Việt.</summary>
public sealed record AuditChangeDto(
    string Field,
    string Label,
    string Before,
    string After);
