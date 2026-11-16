using GiaPha.BusinessObjects.DTOs.Profiles;
using GiaPha.BusinessObjects.Entities;
using GiaPha.BusinessObjects.Exceptions;
using GiaPha.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
namespace GiaPha.Services.Implementations;
public sealed class KinshipService(IF3Repository repo,KinshipResolver resolver,PrivacyFilterService privacy)
{
 public async Task<KinshipResultDto> GetAsync(Guid clan,Guid from,Guid to,Guid user,CancellationToken ct=default)
 {
 if(from==Guid.Empty||to==Guid.Empty)throw new DomainValidationException("MISSING_PERSON_ID","Vui lòng chọn đủ hai người.");
 var people=await repo.People.AsNoTracking().Where(p=>p.ClanId==clan&&p.Status==PersonStatus.Active).ToListAsync(ct);
 var a=people.SingleOrDefault(p=>p.Id==from)??throw new NotFoundException("Không tìm thấy thành viên.");var b=people.SingleOrDefault(p=>p.Id==to)??throw new NotFoundException("Không tìm thấy thành viên.");
 var ids=people.Select(p=>p.Id).ToArray();var relations=await repo.Parents.Where(r=>ids.Contains(r.ParentId)&&ids.Contains(r.ChildId)).ToListAsync(ct);var unions=await repo.Unions.Where(u=>ids.Contains(u.HusbandId)&&ids.Contains(u.WifeId)).ToListAsync(ct);
 var result=resolver.Resolve(a,b,people,relations,unions);var roles=await repo.Roles.Where(r=>r.UserId==user&&r.ClanId==clan).ToListAsync(ct);
 result.Path=result.Path.Select(node=>{var p=people.Single(p=>p.Id==node.PersonId);var dto=new PersonSearchItemDto{IsLiving=p.IsLiving,PrivacyLevel=p.PrivacyLevel,BranchId=p.BranchId,PortraitUrl=node.PortraitUrl};privacy.Apply(dto,roles);return node with {PortraitUrl=dto.PortraitUrl};}).ToArray();return result;
 }
}
