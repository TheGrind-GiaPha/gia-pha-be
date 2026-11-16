using System.ComponentModel.DataAnnotations;

namespace GiaPha.BusinessObjects.DTOs.Auth;

/// <summary>Yêu cầu gửi liên kết đăng nhập (magic link) tới email.</summary>
public sealed class MagicLinkRequest
{
    [Required(ErrorMessage = "Email là bắt buộc.")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
    [StringLength(320)]
    public string Email { get; init; } = string.Empty;
}

/// <summary>Xác nhận một liên kết magic link.</summary>
public sealed class MagicLinkVerifyRequest
{
    [Required(ErrorMessage = "Token là bắt buộc.")]
    public string Token { get; init; } = string.Empty;
}

/// <summary>Kết quả yêu cầu magic link. Link chỉ trả khi đang chạy ở môi trường dev để demo.</summary>
public sealed record MagicLinkResponse(string? Link, string? Message);
