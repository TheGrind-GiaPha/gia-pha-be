namespace GiaPha.BusinessObjects.Entities;

/// <summary>Chi/nhánh của một dòng họ, bắt đầu từ người sáng lập chi.</summary>
public sealed class Branch : BaseEntity
{
    public Guid ClanId { get; set; }

    public string Name { get; set; } = string.Empty;

    public Guid? FounderPersonId { get; set; }
}
