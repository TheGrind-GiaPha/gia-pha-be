using System.Text;
using GiaPha.APIs.Filters;
using GiaPha.Services.Implementations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace GiaPha.APIs.Controllers;
[ApiController,Authorize,RequireClanMembership,Route("api/clans/{clanId:guid}")]
public sealed class AnniversariesController(AnniversaryService service):ControllerBase
{
 [HttpGet("anniversaries")]public async Task<IActionResult> Get(Guid clanId,int? year,int? lunarMonth,Guid? branchId,CancellationToken ct)=>Ok(await service.GetAsync(clanId,year,lunarMonth,branchId,ct));
 [HttpGet("anniversaries.ics")]public async Task<IActionResult> Export(Guid clanId,int? year,int? lunarMonth,Guid? branchId,CancellationToken ct){var data=await service.GetAsync(clanId,year,lunarMonth,branchId,ct);return File(Encoding.UTF8.GetBytes(AnniversaryService.ExportIcs(data)),"text/calendar; charset=utf-8",$"lich-gio-{clanId}-{data.SolarYear}.ics");}
}
