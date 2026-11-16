namespace GiaPha.BusinessObjects.Entities;
public sealed class PersonPhoto : BaseEntity
{
 public Guid ClanId { get; set; }
 public Guid PersonId { get; set; }
 public string ObjectKey { get; set; } = "";
 public string Url { get; set; } = "";
 public string? Caption { get; set; }
 public long FileSizeBytes { get; set; }
 public string ContentType { get; set; } = "";
 public bool SetAsPortrait { get; set; }
 public bool IsVerified { get; set; }
 public bool IsOrphan { get; set; } = true;
}
