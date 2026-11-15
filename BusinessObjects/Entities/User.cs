namespace GiaPha.BusinessObjects.Entities;

/// <summary>Tài khoản người dùng hệ thống (đăng nhập bằng email + mật khẩu).</summary>
public sealed class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;
}
