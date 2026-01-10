using System;
using API.Data;
using API.Extensions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace API.Helpers;

public class LogUserActivity : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var resultContext = await next();

        if (context.HttpContext.User.Identity?.IsAuthenticated != true) return;

        var memberId = resultContext.HttpContext.User.GetMemberId();

        var dbContext = resultContext.HttpContext.RequestServices
            .GetRequiredService<AppDbContext>();

        var member = await dbContext.Members.SingleOrDefaultAsync(x => x.Id == memberId);
        if (member != null)
        {
            member.LastActive = DateTime.UtcNow;
            await dbContext.SaveChangesAsync();
        }
    }
}