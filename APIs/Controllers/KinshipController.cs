using System.Security.Claims;
using GiaPha.APIs.Filters;
using GiaPha.Services.Implementations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace GiaPha.APIs.Controllers;
[ApiController,Authorize,RequireClanMembership,Route("api/clans/{clanId:guid}/kinship")]
public sealed class KinshipController(KinshipService service):ControllerBase
{
 [HttpGet]public async Task<IActionResult> Get(Guid clanId,Guid from,Guid to,CancellationToken ct)=>Ok(await service.GetAsync(clanId,from,to,Guid.Parse(User.FindFirstValue("sub")!),ct));
}
