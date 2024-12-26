using admin_sweetsoft_tech_support.Models;
using Microsoft.EntityFrameworkCore;

namespace admin_sweetsoft_tech_support.Attributes
{
    public class LogService
    {
        private readonly RequestContext _context; 
        public LogService(RequestContext context) 
        { 
            _context = context; 
        }
        public async Task LogAction(int? userId, string action, string description) 
        { 
            var log = new TblLog 
            { 
                UserId = userId, 
                Action = action, 
                Description = description, 
                CreatedAt = DateTime.Now 
            }; 
            _context.TblLogs.Add(log); 
            await _context.SaveChangesAsync(); 
        }
    }
}
