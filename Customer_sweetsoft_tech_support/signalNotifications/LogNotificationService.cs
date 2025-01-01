using NLog;
using NLog.Config;
using NLog.Targets;
using System;
using System.IO;

namespace Customer_sweetsoft_tech_support.signalNotifications
{
    public class LogNotificationService
    {
        private readonly Logger logger;

        public LogNotificationService()
        {
            var date = DateTime.Now;
            string adminLogPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(),
                $"../admin_sweetsoft_tech_support/Notifications/{date.Year}/{date.Month:D2}/{date.Day:D2}"));

            var normalizedPath = Path.GetFullPath(adminLogPath);
            // Cấu hình NLog với đường dẫn log của Admin
            var config = new LoggingConfiguration();

            var fileTarget = new FileTarget("AdminLogFile")
            {
                FileName = Path.Combine(normalizedPath, $"{date.ToString("yyyy-MM-dd-HH")}.log"),
                Layout = "${date:format=dd/MM/yyyy HH\\:mm}, ${event-properties:item=Status}, ${event-properties:item=User}, ${message}",
                CreateDirs = true,
                KeepFileOpen = false,
            };

            config.AddTarget(fileTarget);

            var rule = new LoggingRule("AdminSweetsoftTechSupport", NLog.LogLevel.Info, fileTarget);
            config.LoggingRules.Add(rule);

            LogManager.Configuration = config;
            logger = LogManager.GetLogger("AdminSweetsoftTechSupport");
        }

        public void LogNotificationAction(string user, string message, int status = 0)
        {
            // Tạo LogEventInfo mới
            var logEvent = new LogEventInfo(NLog.LogLevel.Info, logger.Name, message);

            // Gán các properties vào logEvent
            logEvent.Properties["Status"] = status;
            logEvent.Properties["User"] = user;

            // Ghi log vào file
            logger.Log(logEvent);
        }
    }
}
