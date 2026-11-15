namespace GiaPha.BusinessObjects.Entities;

/// <summary>
/// Quan hệ vợ chồng. Rank là thứ tự vợ (vợ cả, vợ hai...),
/// duy nhất trong cùng một người chồng.
/// </summary>
public sealed class Union : BaseEntity
{
    public Guid HusbandId { get; set; }

    public Guid WifeId { get; set; }

    public int Rank { get; set; } = 1;
}
