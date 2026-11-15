using System.ComponentModel.DataAnnotations;

namespace GiaPha.BusinessObjects.DTOs.Auth;

public sealed class RegisterRequest
{
    [Required(ErrorMessage = "Email là bắt buộc.")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
    [StringLength(320)]
    public string Email { get; init; } = string.Empty;

    [Required(ErrorMessage = "Tên hiển thị là bắt buộc.")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Tên hiển thị phải từ 2 đến 200 ký tự.")]
    public string DisplayName { get; init; } = string.Empty;

    [Required(ErrorMessage = "Mật khẩu là bắt buộc.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải từ 6 ký tự.")]
    public string Password { get; init; } = string.Empty;
}
