namespace GiaPha.BusinessObjects.Entities;

/// <summary>
/// Entity mẫu để chứng minh luồng 3-layer + Repository + DAO.
/// Khi chốt ERD, Clan sẽ được bổ sung các trường: OriginStory, Rules,
/// AncestralHall, PrivacyLevel, Status, RootPersonId...
/// </summary>
public sealed class Clan : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
}
