using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using GiaPha.BusinessObjects.Entities;

namespace GiaPha.BusinessObjects.DTOs.Changes;

/// <summary>Yêu cầu tạo đề xuất thay đổi. Payload là JSON tùy theo Type.</summary>
public sealed class CreateChangeRequestRequest
{
    [Required(ErrorMessage = "Loại thay đổi là bắt buộc.")]
    public string Type { get; init; } = string.Empty;

    public JsonElement? Payload { get; init; }
}

/// <summary>Yêu cầu từ chối một đề xuất (bắt buộc lý do).</summary>
public sealed class RejectChangeRequestRequest
{
    [Required(ErrorMessage = "Lý do từ chối là bắt buộc.")]
    [StringLength(2000)]
    public string Reason { get; init; } = string.Empty;
}

public sealed record ChangeRequestDto(
    Guid Id,
    Guid ClanId,
    string Type,
    string Summary,
    ChangeRequestStatus Status,
    Guid RequesterId,
    DateTime CreatedAt,
    DateTime DueAt,
    string? RejectReason);
