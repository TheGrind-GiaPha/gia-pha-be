namespace GiaPha.BusinessObjects.Entities;

/// <summary>Quan hệ cha - con giữa hai người trong gia phả.</summary>
public sealed class ParentChild : BaseEntity
{
    public Guid ParentId { get; set; }

    public Guid ChildId { get; set; }

    public RelationType RelationType { get; set; } = RelationType.Biological;

    public int SiblingOrder { get; set; }
}
