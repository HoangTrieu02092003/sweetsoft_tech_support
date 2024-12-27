namespace admin_sweetsoft_tech_support.Models
{
    public class LogItem
    {
        public int LogId { get; set; }
        public string UserId { get; set; }
        public string Action { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
