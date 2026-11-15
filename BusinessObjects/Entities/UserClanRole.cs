namespace GiaPha.BusinessObjects.Entities;

/// <summary>Gán vai trò của một người dùng trong một dòng họ (có thể giới hạn theo chi).</summary>
public sealed class UserClanRole : BaseEntity
{
    public Guid UserId { get; set; }

    public Guid ClanId { get; set; }

    public Guid? BranchId { get; set; }

    public ClanRole Role { get; set; }
}
