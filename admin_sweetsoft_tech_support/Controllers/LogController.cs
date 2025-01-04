using admin_sweetsoft_tech_support.Attributes;
using admin_sweetsoft_tech_support.Models;
using Microsoft.AspNetCore.Mvc;
using NLog;

namespace admin_sweetsoft_tech_support.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LogController : ControllerBase
    {
        private readonly LogService _logService;

        public LogController(LogService logService)
        {
            _logService = logService;
        }

        [HttpPost("write-log")]
        public IActionResult WriteLog([FromBody] NotificationEntry request)
        {
            if (request == null)
            {
                return BadRequest("Dữ liệu log không hợp lệ.");
            }
            try
            {
                DateTime date = DateTime.Now;
                string adminLogPath = Path.Combine(Directory.GetCurrentDirectory(), "Notifications",date.Year.ToString(), date.ToString("MM"),date.ToString("dd"));

                // Tạo thư mục nếu chưa có
                if (!Directory.Exists(adminLogPath))
                {
                    Directory.CreateDirectory(adminLogPath);
                }

                // Định dạng đường dẫn log file
                string logFilePath = Path.Combine(adminLogPath, $"{DateTime.Now.ToString("yyyy-MM-dd-HH")}.log");

                // Ghi log vào file
                using (StreamWriter writer = new StreamWriter(logFilePath, append: true))
                {
                    _logService.LogNotificationAction(request.User, request.Content, request.Status);
                    //return Ok(new { message = "Log saved successfully!" });
                }

                return Ok(new { message = $"Log saved successfully! {request.User},{request.Status},{request.Content}" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while writing the log" });
            }
        }
    }
}
