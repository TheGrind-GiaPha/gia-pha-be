using System.ComponentModel.DataAnnotations;

namespace GiaPha.BusinessObjects.DTOs.Auth;

public sealed class LoginRequest
{
    [Required(ErrorMessage = "Email là bắt buộc.")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
    public string Email { get; init; } = string.Empty;

    [Required(ErrorMessage = "Mật khẩu là bắt buộc.")]
    public string Password { get; init; } = string.Empty;
}
