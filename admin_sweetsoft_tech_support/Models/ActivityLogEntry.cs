namespace admin_sweetsoft_tech_support.Models
{
    public class ActivityLogEntry
    {
        public DateTime Timestamp { get; set; }
        public string Id { get; set; }
        public string Title { get; set; }
        public string Action { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public string User { get; set; }
    }
}
