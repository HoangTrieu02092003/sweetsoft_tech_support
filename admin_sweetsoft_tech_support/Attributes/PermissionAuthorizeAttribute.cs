using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using admin_sweetsoft_tech_support.Models;
using System.Linq;

namespace admin_sweetsoft_tech_support.Attributes
{
    public class PermissionAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string _permission;

        public PermissionAuthorizeAttribute(string permission)
        {
            _permission = permission;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // Lấy HttpContext
            var httpContext = context.HttpContext;
            var username = httpContext.User.Identity?.Name; // Lấy tên user hiện tại

            if (string.IsNullOrEmpty(username))
            {
                context.Result = new ForbidResult(); // Cấm truy cập
                return;
            }

            using (var dbContext = new RequestContext())
            {
                // Tìm user và kiểm tra quyền
                var user = dbContext.TblUsers
                    .Include(u => u.TblUserPermissions)
                    .ThenInclude(up => up.Permission)
                    .FirstOrDefault(u => u.Username == username);

                if (user == null || !user.TblUserPermissions.Any(p => p.Permission.PermissionName == _permission))
                {
                    context.HttpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
                    context.Result = new RedirectToActionResult("AccessDenied", "Home", null);
                }
            }
        }
    }
}