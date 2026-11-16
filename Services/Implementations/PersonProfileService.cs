using GiaPha.BusinessObjects.DTOs.Profiles;
using GiaPha.BusinessObjects.Entities;
using GiaPha.BusinessObjects.Exceptions;
using GiaPha.Repositories.Interfaces;
using GiaPha.Services.Helpers;
using GiaPha.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
namespace GiaPha.Services.Implementations;
public sealed class PersonProfileService(IF3Repository repo,PrivacyFilterService privacy,ILunarConverter lunar)
{
 public async Task<PagedResult<PersonSearchItemDto>> SearchAsync(Guid clan,Guid user,string? q,Guid? branch,int? generation,bool? living,int page,int size,CancellationToken ct=default)
 {
 if(page<1 || size<1 || size>100)throw new DomainValidationException("INVALID_PAGINATION","Số trang phải >= 1 và kích thước trang trong khoảng 1..100.");
 var rows=await repo.People.AsNoTracking().Where(p=>p.ClanId==clan && p.Status==PersonStatus.Active && (!branch.HasValue || p.BranchId==branch) && (!generation.HasValue || p.Generation==generation) && (!living.HasValue || p.IsLiving==living))
 .Select(p=>new PersonSearchItemDto{Id=p.Id,ClanId=p.ClanId,BranchId=p.BranchId,FullName=p.FullName,HuyName=p.HuyName,TuName=p.TuName,ThuyName=p.ThuyName,Gender=p.Gender,Generation=p.Generation,IsLiving=p.IsLiving,BirthYear=p.BirthDate==null?null:p.BirthDate.Year,DeathYear=p.DeathDate==null?null:p.DeathDate.Year,PortraitUrl=p.PortraitUrl,PrivacyLevel=p.PrivacyLevel}).ToListAsync(ct);
 var term=VietnameseTextNormalizer.Normalize(q);
 var roles=await repo.Roles.Where(r=>r.ClanId==clan && r.UserId==user).ToListAsync(ct);
 // Apply privacy before searching so hidden names cannot be inferred through search hits.
 foreach(var row in rows)privacy.Apply(row,roles);
 var matches=rows.Where(p=>new[]{p.FullName,p.HuyName,p.TuName,p.ThuyName}.Any(n=>VietnameseTextNormalizer.Normalize(n).Contains(term))).OrderByDescending(p=>VietnameseTextNormalizer.Normalize(p.FullName).StartsWith(term)).ThenBy(p=>p.Generation).ThenBy(p=>p.FullName).ToList();
 var branches=await repo.Branches.Where(b=>b.ClanId==clan).ToDictionaryAsync(b=>b.Id,b=>b.Name,ct);
 foreach(var p in matches)if(p.BranchId is {} id && branches.TryGetValue(id,out var name))p.BranchName=name;
 return new(matches.Skip((page-1)*size).Take(size).ToArray(),page,size,matches.Count,(int)Math.Ceiling(matches.Count/(double)size));
 }
 public async Task<PersonProfileDto> GetAsync(Guid clan,Guid id,Guid user,CancellationToken ct=default)
 {
 var p=await repo.People.AsNoTracking().SingleOrDefaultAsync(x=>x.Id==id && x.ClanId==clan && x.Status==PersonStatus.Active,ct)??throw new NotFoundException("Không tìm thấy thành viên trong dòng họ này.");
 var ext=await repo.Profiles.AsNoTracking().SingleOrDefaultAsync(x=>x.PersonId==id && x.ClanId==clan,ct);
 var roles=await repo.Roles.Where(r=>r.ClanId==clan && r.UserId==user).ToListAsync(ct);
 var status=await repo.Clans.Where(c=>c.Id==clan).Select(c=>c.Status).SingleAsync(ct);
 var dto=new PersonProfileDto{Id=id,ClanId=clan,ClanStatus=status,BranchId=p.BranchId,FullName=p.FullName,HuyName=p.HuyName,TuName=p.TuName,ThuyName=p.ThuyName,Gender=p.Gender,Generation=p.Generation,IsLiving=p.IsLiving,PrivacyLevel=p.PrivacyLevel,BirthYear=p.BirthDate?.Year,DeathYear=p.DeathDate?.Year,BirthDate=p.BirthDate,DeathDate=p.DeathDate,PortraitUrl=p.PortraitUrl,Biography=ext?.Biography,BurialPlace=ext?.BurialPlace,BurialMapUrl=ext?.BurialMapUrl,PhoneNumber=ext?.PhoneNumber,Version=p.Version,CanDirectEdit=status==ClanStatus.Draft && roles.Any(r=>r.Role==ClanRole.ClanAdmin || r.Role==ClanRole.BranchEditor && r.BranchId==p.BranchId)};
 if(p.BranchId is {} branch)dto.BranchName=await repo.Branches.Where(b=>b.Id==branch && b.ClanId==clan).Select(b=>b.Name).FirstOrDefaultAsync(ct)??"Chi gốc";
 dto.Photos=await repo.Photos.AsNoTracking().Where(x=>x.ClanId==clan && x.PersonId==id && !x.IsOrphan && x.IsVerified).ToListAsync(ct);
 var people=repo.People.Where(x=>x.ClanId==clan && x.Status==PersonStatus.Active);
 dto.Father=await (from rel in repo.Parents join x in people on rel.ParentId equals x.Id where rel.ChildId==id && rel.RelationType==RelationType.Biological select new FamilyMemberDto(x.Id,x.FullName,x.Generation,rel.RelationType)).FirstOrDefaultAsync(ct);
 dto.Children=await (from rel in repo.Parents join x in people on rel.ChildId equals x.Id where rel.ParentId==id orderby rel.SiblingOrder select new FamilyMemberDto(x.Id,x.FullName,x.Generation,rel.RelationType)).ToListAsync(ct);
 var spouses=await repo.Unions.Where(u=>u.HusbandId==id || u.WifeId==id).Select(u=>u.HusbandId==id?u.WifeId:u.HusbandId).ToListAsync(ct);
 dto.Spouses=await people.Where(x=>spouses.Contains(x.Id)).Select(x=>new FamilyMemberDto(x.Id,x.FullName,x.Generation,null)).ToListAsync(ct);
 privacy.Apply(dto,roles);dto.BirthDateDisplay=Display(dto.BirthDate);dto.DeathDateDisplay=Display(dto.DeathDate);return dto;
 }
 private string Display(FlexibleDate? date)
 {
 if(date==null)return "";
 var solar=string.Join("/",new[]{date.Day?.ToString("D2"),date.Month?.ToString("D2"),date.Year?.ToString()}.Where(x=>x!=null));
 var text=lunar.FormatLunarDate(date);return date.Calendar==CalendarType.Solar && text.EndsWith("(ÂL)")?solar+" (DL) — "+text:text;
 }
}
