namespace admin_sweetsoft_tech_support.Models
{
    public class AuditLogEntry
    {
        public DateTime Timestamp { get; set; }
        public string id { get; set; }
        public string Action { get; set; }
        public string User { get; set; }
        public string Module { get; set; }
        public string Message { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
    }
}
