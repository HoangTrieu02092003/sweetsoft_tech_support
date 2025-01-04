using NLog;

namespace admin_sweetsoft_tech_support.Attributes
{
    public class LogService
    {
        private static readonly Logger logger = LogManager.GetLogger("AdminSweetsoftTechSupport");
        private static readonly Logger loggerNoti = LogManager.GetLogger("Notifications");

        // Phương thức ghi log với Action, User và message
        public void LogAuditAction(string action, string user, string message, string module = "", string oldValue = "", string newValue = "")
        {
            // Tạo LogEventInfo mới
            var logEvent = new LogEventInfo(NLog.LogLevel.Info, logger.Name, message);

            // Gán các properties vào logEvent
            logEvent.Properties["Action"] = action;
            logEvent.Properties["User"] = user;
            logEvent.Properties["Module"] = module;
            logEvent.Properties["OldValue"] = oldValue;
            logEvent.Properties["NewValue"] = newValue;

            // Ghi log vào file
            logger.Log(logEvent);
        }
        public void LogNotificationAction(string reciver, string message, string status = "0")
        {
            // Tạo LogEventInfo mới
            var logEvent = new LogEventInfo(NLog.LogLevel.Info, loggerNoti.Name, message);

            // Gán các properties vào logEvent
            logEvent.Properties["Status"] = status;
            logEvent.Properties["Reciver"] = reciver;

            // Ghi log vào file
            loggerNoti.Log(logEvent);
        }
        public void LogActivityAction(string title, string action, string user)
        {
            // Tạo LogEventInfo mới
            var logEvent = new LogEventInfo(NLog.LogLevel.Info, logger.Name, title);

            // Gán các properties vào logEvent
            logEvent.Properties["Title"] = title;
            logEvent.Properties["Action"] = action;
            logEvent.Properties["User"] = user;

            // Ghi log vào file
            logger.Log(logEvent);
        }
    }
}
