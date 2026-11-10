namespace GiaPha.BusinessObjects.Entities;

/// <summary>
/// Base entity cho toàn bộ entity nghiệp vụ.
/// Id dùng Guid để tránh phụ thuộc vào auto-increment của từng DB.
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}
