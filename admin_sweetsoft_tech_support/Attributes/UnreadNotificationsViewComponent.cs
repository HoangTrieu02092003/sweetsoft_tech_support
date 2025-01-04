using admin_sweetsoft_tech_support.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace admin_sweetsoft_tech_support.Attributes
{
    public class UnreadNotificationsViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return View(0); 
            }

            int unreadCount = 0;

            var logDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Notifications");

            if (Directory.Exists(logDirectory))
            {
                var logFiles = Directory.GetFiles(logDirectory, "*.log", SearchOption.AllDirectories);

                foreach (var filePath in logFiles)
                {
                    var logLines = File.ReadAllLines(filePath);
                    foreach (var line in logLines)
                    {
                        var logParts = line.Split(", ", StringSplitOptions.RemoveEmptyEntries);

                        var status = logParts[1];
                        var userIdLog = logParts[2];

                        if (userIdLog == userId && status == "0")
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
