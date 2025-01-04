using NLog.Config;
using NLog.Targets;
using NLog;

namespace admin_sweetsoft_tech_support.Attributes
{
    public class LogSetup
    {
        public static void CreateLogAuditDirectories()
        {
            string logDirectory = Path.Combine(Directory.GetCurrentDirectory(), "logs");
            string yearMonthDayDirectory = Path.Combine(logDirectory,
                DateTime.Now.ToString("yyyy"),
                DateTime.Now.ToString("MM"),
                DateTime.Now.ToString("dd"));

            // Kiểm tra và tạo thư mục nếu chưa có
            if (!Directory.Exists(yearMonthDayDirectory))
            {
                Directory.CreateDirectory(yearMonthDayDirectory);
            }

            // Tạo file log (nếu muốn tạo file log ngay từ đầu)
            string logFilePath = Path.Combine(yearMonthDayDirectory, $"{DateTime.Now.ToString("yyyy-MM-dd-HH")}.log");
            if (!File.Exists(logFilePath))
            {
                File.Create(logFilePath).Dispose(); // Tạo file nếu chưa có
            }

            var config = LogManager.Configuration ?? new LoggingConfiguration();
            var fileTarget = new FileTarget("file")
            {
                FileName = Path.Combine(yearMonthDayDirectory, "${date:format=yyyy-MM-dd-HH}.log"),
                Layout = "${date:format=dd/MM/yyyy HH\\:mm}, " +
                         "${event-properties:item=Action}, " +
                         "${event-properties:item=User}, " +
                         "${event-properties:item=Module}, " +
                         "${event-properties:item=OldValue}, " +
                         "${event-properties:item=NewValue}, " +
                         "${message}",
                CreateDirs = true,
                KeepFileOpen = false
            };

            config.AddTarget(fileTarget);

            var rule = new LoggingRule("AdminSweetsoftTechSupport", NLog.LogLevel.Info, fileTarget);
            config.LoggingRules.Add(rule);


            LogManager.Configuration = config;
            LogManager.ReconfigExistingLoggers();
        }
        public static void CreateLogNotificationDirectories()
        {
            string logDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Notifications");
            string yearMonthDayDirectory = Path.Combine(logDirectory,
                DateTime.Now.ToString("yyyy"),
                DateTime.Now.ToString("MM"),
                DateTime.Now.ToString("dd"));

            // Kiểm tra và tạo thư mục nếu chưa có
            if (!Directory.Exists(yearMonthDayDirectory))
            {
                Directory.CreateDirectory(yearMonthDayDirectory);
            }

            // Tạo file log (nếu muốn tạo file log ngay từ đầu)
            string logFilePath = Path.Combine(yearMonthDayDirectory, $"{DateTime.Now.ToString("yyyy-MM-dd-HH")}.log");
            if (!File.Exists(logFilePath))
            {
                File.Create(logFilePath).Dispose(); // Tạo file nếu chưa có
            }

            var config = LogManager.Configuration ?? new LoggingConfiguration();
            var fileTarget = new FileTarget("NotificationsFile")
            {
                FileName = Path.Combine(yearMonthDayDirectory, "${date:format=yyyy-MM-dd-HH}.log"),
                Layout = "${date:format=dd/MM/yyyy HH\\:mm}, " +
                         "${event-properties:item=Status}, " +
                         "${event-properties:item=Reciver}, " +
                         "${message}",
                CreateDirs = true,
                KeepFileOpen = false
            };

            config.AddTarget(fileTarget);

            var rule = new LoggingRule("Notifications", NLog.LogLevel.Info, fileTarget);
            config.LoggingRules.Add(rule);


            LogManager.Configuration = config;
            LogManager.ReconfigExistingLoggers();
        }
        public static void CreateLogActivityDirectories()
        {
            string logDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Activitys");
            string yearMonthDayDirectory = Path.Combine(logDirectory,
                DateTime.Now.ToString("yyyy"),
                DateTime.Now.ToString("MM"),
                DateTime.Now.ToString("dd"));

            // Kiểm tra và tạo thư mục nếu chưa có
            if (!Directory.Exists(yearMonthDayDirectory))
            {
                Directory.CreateDirectory(yearMonthDayDirectory);
            }

            // Tạo file log (nếu muốn tạo file log ngay từ đầu)
            string logFilePath = Path.Combine(yearMonthDayDirectory, $"{DateTime.Now.ToString("yyyy-MM-dd-HH")}.log");
            if (!File.Exists(logFilePath))
            {
                File.Create(logFilePath).Dispose(); // Tạo file nếu chưa có
            }

            var config = LogManager.Configuration ?? new LoggingConfiguration();
            var fileTarget = new FileTarget("ActivityFile")
            {
                FileName = Path.Combine(yearMonthDayDirectory, "${date:format=yyyy-MM-dd-HH}.log"),
                Layout = "${date:format=dd/MM/yyyy HH\\:mm}, " +
                         "${event-properties:item=Title}, " +
                         "${event-properties:item=Action}, " +
                         "${event-properties:item=User}, ",
                CreateDirs = true,
                KeepFileOpen = false
            };

            config.AddTarget(fileTarget);

            var rule = new LoggingRule("Activity", NLog.LogLevel.Info, fileTarget);
            config.LoggingRules.Add(rule);


            LogManager.Configuration = config;
            LogManager.ReconfigExistingLoggers();
        }
    }
}
