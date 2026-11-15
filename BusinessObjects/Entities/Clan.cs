namespace GiaPha.BusinessObjects.Entities;

/// <summary>
/// Dòng họ: không gian làm việc riêng biệt của từng dòng họ.
/// </summary>
public sealed class Clan : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? OriginStory { get; set; }

    public string? Rules { get; set; }

    public ClanStatus Status { get; set; } = ClanStatus.Draft;

    public ClanVisibility Visibility { get; set; } = ClanVisibility.Private;

    public DateTime? PublishedAt { get; set; }
}
