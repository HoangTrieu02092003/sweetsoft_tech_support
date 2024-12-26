using admin_sweetsoft_tech_support.Models;

namespace admin_sweetsoft_tech_support.Attributes
{
    public class ActivityLogService
    {
        private readonly RequestContext _context;

        public ActivityLogService(RequestContext context)
        {
            _context = context;
        }

        public void Log(int userId, int requestId, string action)
        {
            var log = new TblActivityLog
            {
                UserId = userId,
                RequestId = requestId,
                Action = action,
                CreatedAt = DateTime.Now
            };

            _context.TblActivityLogs.Add(log);
            _context.SaveChanges();
        }
    }
}