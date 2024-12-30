using admin_sweetsoft_tech_support.Models;

namespace admin_sweetsoft_tech_support.Attributes
{
    public class NotificationService
    {
        private readonly RequestContext _context;

        public NotificationService(RequestContext context)
        {
            _context = context;
        }

        // Phương thức log vào file
        public async Task LogActionToFile(int userId, string message, short? status = 0)
        {
            var logMessage = $"{DateTime.UtcNow}: UserId = {userId}, Message = {message}, Status = {status}";

            var logFilePath = Path.Combine(Directory.GetCurrentDirectory(), "logs", "notification.log");

            // Đảm bảo thư mục logs tồn tại
            if (!Directory.Exists(Path.GetDirectoryName(logFilePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(logFilePath));
            }

            // Ghi log vào file (Append nếu file đã tồn tại)
            using (var writer = new StreamWriter(logFilePath, append: true))
            {
                await writer.WriteLineAsync(logMessage);
            }
        }

        // Phương thức log vào Db
        public async Task LogActionToDb(int userId, string message, short? status = 0)
        {
            var notificationLog = new TblNotification
            {
                UserId = userId,
                Message = message,
                Status = status,
                CreatedAt = DateTime.UtcNow,
            };

            _context.TblNotifications.Add(notificationLog);
            await _context.SaveChangesAsync();
        }
    }
}
