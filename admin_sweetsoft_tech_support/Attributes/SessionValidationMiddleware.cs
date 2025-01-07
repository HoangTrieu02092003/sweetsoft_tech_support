using admin_sweetsoft_tech_support.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

public class SessionValidationMiddleware
{
    private readonly RequestDelegate _next;

    public SessionValidationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, RequestContext dbContext)
    {
        if (context.Request.Path.StartsWithSegments("/Đăng-nhập"))
        {
            await _next(context);
            return;
        }

        var userIdClaim = context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

        if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out var userId))
        {
            var session = await dbContext.TblSessions.FirstOrDefaultAsync(s => s.UserId == userId && s.ExpiresAt > DateTime.Now);

            if (session == null)
            {
                if (context.Request.Cookies.TryGetValue("UserSessionToken", out var sessionToken) && !string.IsNullOrEmpty(sessionToken))
                {
                    var cookieSession = await dbContext.TblSessions.FirstOrDefaultAsync(s => s.SessionToken == sessionToken);

                    if (cookieSession != null)
                    {
                        cookieSession.ExpiresAt = DateTime.Now.AddHours(2);
                        dbContext.Update(cookieSession);
                        await dbContext.SaveChangesAsync();
                    }
                    else
                    {
                        context.Response.Redirect("/Đăng-nhập");
                        return;
                    }
                }
                else
                {
                    context.Response.Redirect("/Đăng-nhập");
                    return;
                }
            }

            if (session != null && (session.ExpiresAt - DateTime.Now)?.TotalMinutes < 10)
            {
                session.ExpiresAt = DateTime.Now.AddHours(2);
                dbContext.Update(session);
                await dbContext.SaveChangesAsync();
            }
        }

        await _next(context);
    }
}