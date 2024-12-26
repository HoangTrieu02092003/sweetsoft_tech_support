using admin_sweetsoft_tech_support.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace admin_sweetsoft_tech_support.Attributes
{
    public class UnreadNotificationsViewComponent : ViewComponent
    {
        private readonly RequestContext _context;

        public UnreadNotificationsViewComponent(RequestContext context)
        {
            _context = context;
        }

        public IViewComponentResult Invoke()
        {
            var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            int unreadCount = 0;

            if (!string.IsNullOrEmpty(userId))
            {
                unreadCount = _context.TblNotifications
                    .Where(n => n.UserId == int.Parse(userId) && n.Status == 0)
                    .Count();
            }

            return View(unreadCount);
        }
    }
}
