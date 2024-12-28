using admin_sweetsoft_tech_support.Models;

namespace admin_sweetsoft_tech_support.Attributes
{
    public class ActivityLogService
    {
        private readonly RequestContext _context;

        public ActivityLogService(RequestContext context)
        {
            _context = context;
        }
        // Phương thức log vào file
        public async Task LogActionToFile(int userId, int requestId, string action)
        {
            var logMessage = $"{DateTime.Now}: UserId = {userId}, RequestId = {requestId}, Action = {action}";

            var logFilePath = Path.Combine(Directory.GetCurrentDirectory(), "logs", "activity.log");

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
        public void LogActionToDb(int userId, int requestId, string action)
        {
            var log = new TblActivityLog
            {
                UserId = userId,
                RequestId = requestId,
                Action = action,
                CreatedAt = DateTime.Now
            };

            _context.TblActivityLogs.Add(log);
            _context.SaveChanges();
        }
    }
}