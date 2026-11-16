using GiaPha.BusinessObjects.Entities;
namespace GiaPha.BusinessObjects.DTOs.Profiles;
public sealed record PagedResult<T>(IReadOnlyList<T> Items,int Page,int PageSize,int TotalCount,int TotalPages);
public class PersonSearchItemDto
{
 public Guid Id {get;set;}
 public Guid ClanId {get;set;}
 public Guid? BranchId {get;set;}
 public string BranchName {get;set;}="Chi gốc";
 public string FullName {get;set;}="";
 public string? HuyName {get;set;}
 public string? TuName {get;set;}
 public string? ThuyName {get;set;}
 public Gender Gender {get;set;}
 public int Generation {get;set;}
 public bool IsLiving {get;set;}
 public int? BirthYear {get;set;}
 public int? DeathYear {get;set;}
 public string? PortraitUrl {get;set;}
 public PrivacyLevel PrivacyLevel {get;set;}
 public bool IsRedacted {get;set;}
}
public sealed record FamilyMemberDto(Guid PersonId,string FullName,int Generation,RelationType? RelationType=null);
public sealed class PersonProfileDto : PersonSearchItemDto
{
 public ClanStatus ClanStatus {get;set;}
 public FlexibleDate? BirthDate {get;set;}
 public FlexibleDate? DeathDate {get;set;}
 public string BirthDateDisplay {get;set;}="";
 public string DeathDateDisplay {get;set;}="";
 public string? Biography {get;set;}
 public string? BurialPlace {get;set;}
 public string? BurialMapUrl {get;set;}
 public string? PhoneNumber {get;set;}
 public FamilyMemberDto? Father {get;set;}
 public IReadOnlyList<FamilyMemberDto> Spouses {get;set;}=[];
 public IReadOnlyList<FamilyMemberDto> Children {get;set;}=[];
 public IReadOnlyList<PersonPhoto> Photos {get;set;}=[];
 public int Version {get;set;}
 public bool CanDirectEdit {get;set;}
}
