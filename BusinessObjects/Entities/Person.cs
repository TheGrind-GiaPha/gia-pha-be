namespace GiaPha.BusinessObjects.Entities;

/// <summary>
/// Một người trong gia phả. Đời (Generation) và đường dẫn (Path) được
/// tính tự động từ quan hệ cha - con (xem luồng 2 - logic của cây).
/// </summary>
public sealed class Person : BaseEntity, IClanScopedEntity
{
    public Guid ClanId { get; set; }

    public Guid? BranchId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string? HuyName { get; set; }

    public string? TuName { get; set; }

    public string? ThuyName { get; set; }

    public string? OtherNames { get; set; }

    public Gender Gender { get; set; }

    public FlexibleDate? BirthDate { get; set; }

    public FlexibleDate? DeathDate { get; set; }

    public bool IsLiving { get; set; }

    public int Generation { get; set; }

    public string Path { get; set; } = "/";

    public PersonStatus Status { get; set; } = PersonStatus.Active;

    public PrivacyLevel PrivacyLevel { get; set; } = PrivacyLevel.Public;

    public string? PortraitUrl { get; set; }

    public string? Biography { get; set; }

    public string? BurialPlace { get; set; }

    public string? BurialMapUrl { get; set; }

    /// <summary>Thông tin liên lạc (số điện thoại / email / Zalo...) của người này.</summary>
    public string? Contact { get; set; }

    /// <summary>Id của người mà bản ghi này đã bị gộp vào (khi Status = Deleted do merge).</summary>
    public Guid? MergedIntoId { get; set; }

    /// <summary>Người này còn con cháu lịch sử chưa đưa vào dữ liệu (UI hiện "…").</summary>
    public bool HasHiddenChildren { get; set; }

    public int Version { get; set; } = 1;
}
