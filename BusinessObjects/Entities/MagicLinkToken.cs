namespace GiaPha.BusinessObjects.Entities;

/// <summary>
/// Token đăng nhập không cần mật khẩu (magic link) cho người duyệt lớn tuổi.
/// Chỉ lưu hash của token, không lưu token gốc. Dùng một lần, hết hạn sau 48 giờ.
/// </summary>
public sealed class MagicLinkToken : BaseEntity
{
    public Guid UserId { get; set; }

    /// <summary>Đề xuất cần duyệt (nếu link dùng để duyệt 1 phiếu cụ thể).</summary>
    public Guid? ChangeRequestId { get; set; }

    public string TokenHash { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }

    public DateTime? UsedAt { get; set; }
}
