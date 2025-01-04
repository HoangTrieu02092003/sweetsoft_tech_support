using admin_sweetsoft_tech_support.Models;
using Microsoft.AspNetCore.Mvc;
using NuGet.Packaging.Signing;
using System.Globalization;
using System.Security.Claims;

namespace admin_sweetsoft_tech_support.Attributes
{
    public class NotificationViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var logs = new List<dynamic>();
            var logDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Notifications");

            if (Directory.Exists(logDirectory))
            {
                var logFiles = Directory.GetFiles(logDirectory, "*.log", SearchOption.AllDirectories);
                foreach (var filePath in logFiles)
                {
                    var logLines = File.ReadAllLines(filePath);
                    foreach (var line in logLines)
                    {
                        var logParts = line.Split(", ");

                        var userIdLog = logParts[2];
                        var message = logParts[3];

                        if (userIdLog == userId)
                        {
                            logs.Add(new
                            {
                                Message = message,
                            });
                        }
                    }
                }
            }

            return View(logs);
        }
    }
}