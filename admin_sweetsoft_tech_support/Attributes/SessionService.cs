using admin_sweetsoft_tech_support.Models;
using Microsoft.EntityFrameworkCore;

namespace admin_sweetsoft_tech_support.Attributes
{
    public class SessionService
    {
        private readonly RequestContext _context;

        public SessionService(RequestContext context)
        {
            _context = context;
        }

        //tạo mới
        public async Task CreateSessionAsync(int userId, string sessionToken, int expiresInHours = 2)
        {
            var session = new TblSession
            {
                UserId = userId,
                SessionToken = sessionToken,
                CreatedAt = DateTime.Now,
                ExpiresAt = DateTime.Now.AddHours(expiresInHours)
            };

            _context.TblSessions.Add(session);
            await _context.SaveChangesAsync();
        }

        //xóa
        public async Task DeleteSessionAsync(int? userId)
        {
            var session = await _context.TblSessions.FirstOrDefaultAsync(s => s.UserId == userId);
            if (session != null)
            {
                _context.TblSessions.Remove(session);
                await _context.SaveChangesAsync();
            }
        }
    }
}
