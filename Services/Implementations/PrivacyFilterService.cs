using GiaPha.BusinessObjects.DTOs.Profiles;
using GiaPha.BusinessObjects.Entities;
namespace GiaPha.Services.Implementations;
public sealed class PrivacyFilterService
{
 public bool IsPrivileged(Guid? branch,IEnumerable<UserClanRole> roles) => roles.Any(r=>r.Role==ClanRole.ClanAdmin || ((r.Role==ClanRole.BranchEditor || r.Role==ClanRole.Approver) && r.BranchId==branch));
 public void Apply(PersonSearchItemDto p,IEnumerable<UserClanRole> roles)
 {
 if(!p.IsLiving || p.PrivacyLevel==PrivacyLevel.Public || IsPrivileged(p.BranchId,roles))return;
 p.IsRedacted=true;
 if(p.PrivacyLevel==PrivacyLevel.Private) {p.HuyName=null;p.TuName=null;p.ThuyName=null;p.BirthYear=null;p.PortraitUrl=null;}
 if(p is PersonProfileDto profile)
 {
 profile.PhoneNumber=null;
 if(p.PrivacyLevel==PrivacyLevel.Private){profile.BirthDate=null;profile.Biography=null;profile.Photos=[];}
 else if(profile.BirthDate is {} b)profile.BirthDate=new(){Year=b.Year,Calendar=b.Calendar};
 }
 }
}
