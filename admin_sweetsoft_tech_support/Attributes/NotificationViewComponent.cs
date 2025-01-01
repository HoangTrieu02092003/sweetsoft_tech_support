using admin_sweetsoft_tech_support.Models;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
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

        public IViewComponentResult Invoke() 
        {
            var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            var logs = new List<dynamic>();
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Logs", "notification.log");

            if (File.Exists(filePath))
            {
                var logLines = File.ReadAllLines(filePath);

                foreach (var line in logLines)
                {
                    // Tách các phần từ log
                    var logParts = line.Split(new string[] { ": " }, StringSplitOptions.None);

                    if (logParts.Length == 2)
                    {
                        var dateTime = logParts[0];
                        var logDetails = logParts[1].Split(", ");

                        var userIdLog = logDetails.FirstOrDefault(detail => detail.StartsWith("UserId"))?.Split('=')[1].Trim();
                        var message = logDetails.FirstOrDefault(detail => detail.StartsWith("Message"))?.Split('=')[1].Trim();
                        var status = logDetails.FirstOrDefault(detail => detail.StartsWith("status"))?.Split('=')[1].Trim();
                        
                        if (userIdLog == userId)
                        {
                            DateTime parsedDateTime;
                            if (DateTime.TryParseExact(dateTime, "MM/dd/yyyy h:mm:ss tt", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDateTime))
                            {
                                logs.Add(new
                                {
                                    UserId = userIdLog,
                                    Message = message,
                                    Status = status,
                                    CreatedAt = parsedDateTime,
                                });
                            }
                        }
                    }
                }
            }
            

            if (!string.IsNullOrEmpty(userId)) 
            { 
                var notifications = _context.TblNotifications.Where(n => n.UserId == int.Parse(userId)).ToList(); 
                var allLogs = notifications.Concat(logs).ToList(); 
                return View(allLogs); 
            }
            else
            { // Handle the case where userId is null
              return View(new List<dynamic>()); }
            }
    }
}
