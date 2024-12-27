using admin_sweetsoft_tech_support.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace admin_sweetsoft_tech_support.Attributes
{
    public class SessionValidationMiddleware
    {
        private readonly RequestDelegate _next;

        public SessionValidationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, RequestContext dbContext)
        {
            // Lấy userId từ Claims
            var userIdClaim = context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out var userId))
            {
                // Kiểm tra phiên hợp lệ
                var session = await dbContext.TblSessions.FirstOrDefaultAsync(s => s.UserId == userId && s.ExpiresAt > DateTime.Now);

                if (session == null && !context.Request.Path.StartsWithSegments("/Admin/Login"))
                {
                    // Nếu không có phiên hợp lệ và không ở trang Login, chuyển hướng
                    context.Response.Redirect("/Admin/Login");
                    return;
                }

                if (session != null)
                {
                    // Làm mới thời gian hết hạn nếu gần hết hạn
                    if ((session.ExpiresAt - DateTime.Now)?.TotalMinutes < 10) // Làm mới trước khi hết hạn 10 phút
                    {
                        session.ExpiresAt = DateTime.Now.AddHours(2);
                        dbContext.Update(session);
                        await dbContext.SaveChangesAsync();
                    }
                }
            }

            // Tiếp tục xử lý request
            await _next(context);
        }
    }
}