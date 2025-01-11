using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
                    var logParts = line.Split(", ");
                    if (logParts.Length <= 7)
                    {
                        var isDelete = logParts[5];
                        var statusLog = logParts[1];
                        var userIdLog = logParts[2];

                        if (userIdLog == userId && isDelete == "0" && statusLog == "0")
                        {
                            unreadCount++;
                        }
                    }
                }
            }
        }
        return View(unreadCount);
    }
}