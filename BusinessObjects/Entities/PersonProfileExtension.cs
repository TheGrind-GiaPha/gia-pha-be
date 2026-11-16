namespace GiaPha.BusinessObjects.Entities;
public sealed class PersonProfileExtension
{
 public Guid PersonId { get; set; }
 public Guid ClanId { get; set; }
 public string? Biography { get; set; }
 public string? BurialPlace { get; set; }
 public string? BurialMapUrl { get; set; }
 public string? PhoneNumber { get; set; }
 public DateTime? UpdatedAt { get; set; }
}
