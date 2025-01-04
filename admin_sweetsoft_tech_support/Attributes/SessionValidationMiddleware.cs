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
        var userIdClaim = context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

        if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out var userId))
        {
            // Lấy session hiện tại từ database
            var session = await dbContext.TblSessions.FirstOrDefaultAsync(s => s.UserId == userId && s.ExpiresAt > DateTime.Now);

            if (session == null)
            {
                // Nếu session hết hạn, kiểm tra cookie
                if (context.Request.Cookies.TryGetValue("UserSessionToken", out var sessionToken))
                {
                    var cookieSession = await dbContext.TblSessions.FirstOrDefaultAsync(s => s.SessionToken == sessionToken);

                    if (cookieSession != null)
                    {
                        // Tái tạo session nếu token hợp lệ
                        cookieSession.ExpiresAt = DateTime.Now.AddHours(2);
                        dbContext.Update(cookieSession);
                        await dbContext.SaveChangesAsync();
                    }
                    else
                    {
                        // Nếu không có session hợp lệ, chuyển hướng đến trang đăng nhập
                        context.Response.Redirect("/Admin/Login");
                        return;
                    }
                }
                else
                {
                    // Nếu không có cookie, chuyển hướng đến trang đăng nhập
                    context.Response.Redirect("/Admin/Login");
                    return;
                }
            }

            // Cập nhật thời gian hết hạn nếu session gần hết hạn
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