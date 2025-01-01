using NLog;

namespace admin_sweetsoft_tech_support.Attributes
{
    public class AuditLogService
    {
        private static readonly Logger logger = LogManager.GetLogger("AdminSweetsoftTechSupport");

        // Phương thức ghi log với Action, User và message
        public void LogAction(string action, string user, string message, string module = "", string oldValue = "", string newValue = "")
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
    }
}
