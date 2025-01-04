using NLog;

namespace admin_sweetsoft_tech_support.Attributes
{
    public class LogService
    {
        private static readonly Logger logger = LogManager.GetLogger("AdminSweetsoftTechSupport");
        private static readonly Logger loggerNoti = LogManager.GetLogger("Notifications");
        private static readonly Logger loggerActi = LogManager.GetLogger("AdminSweetsoftTechSupport");

        // Phương thức ghi log với Action, User và message (Async)
        public async Task LogAuditAction(string action, string user, string message, string module = "", string oldValue = "", string newValue = "")
        {
            // Tạo LogEventInfo mới
            var logEvent = new LogEventInfo(NLog.LogLevel.Info, logger.Name, message);

            // Gán các properties vào logEvent
            logEvent.Properties["Action"] = action;
            logEvent.Properties["User"] = user;
            logEvent.Properties["Module"] = module;
            logEvent.Properties["OldValue"] = oldValue;
            logEvent.Properties["NewValue"] = newValue;

            // Ghi log vào file (Async)
            await Task.Run(() => logger.Log(logEvent)); // Ghi log trong Task
        }

        // Phương thức ghi log Notification Action (Async)
        public async Task LogNotificationAction(string user, string message, string status = "0")
        {
            try
            {
                // Tạo LogEventInfo mới
                var logEvent = new LogEventInfo(NLog.LogLevel.Info, loggerNoti.Name, message);

                // Gán các properties vào logEvent
                logEvent.Properties["Status"] = status;
                logEvent.Properties["User"] = user;

                // Ghi log vào file (Async)
                await Task.Run(() => loggerNoti.Log(logEvent)); // Ghi log trong Task
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Logging failed: " + ex.Message);
            }
        }

        // Phương thức ghi log Activity Action (Async)
        public async Task LogActivityAction(string title, string action, string user)
        {
            // Tạo LogEventInfo mới
            var logEvent = new LogEventInfo(NLog.LogLevel.Info, logger.Name, title);

            // Gán các properties vào logEvent
            logEvent.Properties["Title"] = title;
            logEvent.Properties["Action"] = action;
            logEvent.Properties["User"] = user;

            // Ghi log vào file (Async)
            await Task.Run(() => logger.Log(logEvent)); // Ghi log trong Task
        }
    }
}