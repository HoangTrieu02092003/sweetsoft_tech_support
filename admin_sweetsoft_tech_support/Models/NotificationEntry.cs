namespace admin_sweetsoft_tech_support.Models
{
    public class NotificationEntry
    {
        public string Id { get; set; }
        public string User { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string Status { get; set; }
        public DateTime Timestamp { get; set; }
        public string isDelete { get; set; }
    }
}

