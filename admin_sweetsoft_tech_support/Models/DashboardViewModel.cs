namespace admin_sweetsoft_tech_support.Models
{
    public class DashboardViewModel
    {
        public int ActiveSessions { get; set; }
        public int RecentLogCount { get; set; }
        public List<LogEntry> RecentLogs { get; set; }
        public List<ActionStatistic> AuditLogStatistics { get; set; }
    }

    public class LogEntry
    {
        public DateTime? CreatedAt { get; set; }
    }

        public class ActionStatistic
    {
        public string ActionType { get; set; }
        public int Count { get; set; }
    }
}
