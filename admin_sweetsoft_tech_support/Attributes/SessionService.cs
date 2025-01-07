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
            //Xóa tất cả các phiên cũ trước khi tạo phiên mới
            var existingSessions = await _context.TblSessions.Where(s => s.UserId == userId).ToListAsync();
            if (existingSessions.Any())
            {
                _context.TblSessions.RemoveRange(existingSessions);
            }

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
            if (userId == null)
                return;

            var sessions = await _context.TblSessions.Where(s => s.UserId == userId).ToListAsync();
            if (sessions.Any())
            {
                _context.TblSessions.RemoveRange(sessions); // Xóa tất cả các phiên liên quan
                await _context.SaveChangesAsync();
            }
        }

    }
}
