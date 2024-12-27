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
                    .Where(n => n.UserId == int.Parse(userId))
                    .Count();
            }
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Logs", "notification.log");

            if (File.Exists(filePath))
            {
                var logLines = File.ReadAllLines(filePath);

                foreach (var line in logLines)
                {
                    var logParts = line.Split(new string[] { ": " }, StringSplitOptions.None);

                    if (logParts.Length == 2)
                    {
                        var logDetails = logParts[1].Split(", ");
                        var userIdLog = logDetails.FirstOrDefault(detail => detail.StartsWith("UserId"))?.Split('=')[1].Trim();
                        var status = logDetails.FirstOrDefault(detail => detail.StartsWith("Status"))?.Split('=')[1].Trim();

                        if (userIdLog == userId)
                        {
                            unreadCount++;
                        }
                    }
                }
            }
            return View(unreadCount);
        }
    }
}
