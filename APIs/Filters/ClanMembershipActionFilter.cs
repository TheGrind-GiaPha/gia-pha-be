using System.Security.Claims;
using GiaPha.BusinessObjects.Exceptions;
using GiaPha.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
namespace GiaPha.APIs.Filters;
public sealed class ClanMembershipActionFilter(IRepositoryManager repo) : IAsyncActionFilter
{
 public async Task OnActionExecutionAsync(ActionExecutingContext context,ActionExecutionDelegate next)
 {
 if(!Guid.TryParse(context.RouteData.Values["clanId"]?.ToString(),out var clan) || !Guid.TryParse(context.HttpContext.User.FindFirstValue("sub"),out var user) || !(await repo.UserClanRole.GetByClanAsync(clan,context.HttpContext.RequestAborted)).Any(r=>r.UserId==user))throw new NotFoundException("Không tìm thấy dòng họ hoặc thành viên.");
 await next();
 }
}
public sealed class RequireClanMembershipAttribute : TypeFilterAttribute
{
 public RequireClanMembershipAttribute():base(typeof(ClanMembershipActionFilter)){}
}
