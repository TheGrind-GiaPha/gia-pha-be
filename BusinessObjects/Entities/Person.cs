namespace GiaPha.BusinessObjects.Entities;

/// <summary>
/// Một người trong gia phả. Đời (Generation) và đường dẫn (Path) được
/// tính tự động từ quan hệ cha - con (xem luồng 2 - logic của cây).
/// </summary>
public sealed class Person : BaseEntity
{
    public Guid ClanId { get; set; }

    public Guid? BranchId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string? HuyName { get; set; }

    public string? TuName { get; set; }

    public string? ThuyName { get; set; }

    public Gender Gender { get; set; }

    public FlexibleDate? BirthDate { get; set; }

    public FlexibleDate? DeathDate { get; set; }

    public bool IsLiving { get; set; }

    public int Generation { get; set; }

    public string Path { get; set; } = "/";

    public PersonStatus Status { get; set; } = PersonStatus.Active;

    public PrivacyLevel PrivacyLevel { get; set; } = PrivacyLevel.Public;

    public string? PortraitUrl { get; set; }

    public int Version { get; set; } = 1;
}
