using admin_sweetsoft_tech_support.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace admin_sweetsoft_tech_support.Attributes
{
    public class NotificationViewComponent : ViewComponent
    {
        private readonly RequestContext _context; 
        public NotificationViewComponent(RequestContext context)
        {
            _context = context;
        }

        public IViewComponentResult Invoke() { var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier); var notifications = _context.TblNotifications.Where(n => n.UserId == int.Parse(userId)).ToList(); return View(notifications); }
    }
}
