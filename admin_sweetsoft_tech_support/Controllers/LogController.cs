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
                return BadRequest(new { message = "Dữ liệu log không hợp lệ." });
            }

            try
            {
                // Xử lý Id và Timestamp tự động nếu không có
                if (string.IsNullOrEmpty(request.Id))
                {
                    request.Id = _logService.GenerateUniqueId();
                }

                if (request.Timestamp == default(DateTime))
                {
                    request.Timestamp = DateTime.UtcNow;
                }

                // Ghi log
                _logService.LogNotificationAction(request.User, request.Content, request.Status);

                // Trả về kết quả
                return Ok(new
                {
                    message = "Log saved successfully!",
                    logDetails = new
                    {
                        User = request.User,
                        Status = request.Status,
                        Id = request.Id,
                        Content = request.Content,
                        Timestamp = request.Timestamp
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "An error occurred while writing the log",
                    error = ex.Message
                });
            }
        }
    }
}
