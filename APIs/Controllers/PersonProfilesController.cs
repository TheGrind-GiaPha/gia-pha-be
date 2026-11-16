using System.Security.Claims;
using GiaPha.APIs.Filters;
using GiaPha.Services.Implementations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace GiaPha.APIs.Controllers;
[ApiController,Authorize,RequireClanMembership,Route("api/clans/{clanId:guid}/persons")]
public sealed class PersonProfilesController(PersonProfileService service):ControllerBase
{
 private Guid UserId=>Guid.Parse(User.FindFirstValue("sub")!);
 [HttpGet("search")]
 public async Task<IActionResult> Search(Guid clanId,string? q,Guid? branchId,int? generation,bool? isLiving,int page=1,int pageSize=20,CancellationToken ct=default)=>Ok(await service.SearchAsync(clanId,UserId,q,branchId,generation,isLiving,page,pageSize,ct));
 [HttpGet("{personId:guid}/profile")]
 public async Task<IActionResult> Get(Guid clanId,Guid personId,CancellationToken ct)=>Ok(await service.GetAsync(clanId,personId,UserId,ct));
}
