using admin_sweetsoft_tech_support.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace admin_sweetsoft_tech_support.Controllers
{
    public class DashboardController : Controller
    {
        private readonly RequestContext _context;

        public DashboardController(RequestContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            var activeSessions = await _context.TblSessions.CountAsync(s => s.ExpiresAt > DateTime.Now);


            var recentLogs = await _context.TblLogs
                .Where(l => l.CreatedAt > DateTime.Now.AddDays(-7))
                .Select(l => new LogEntry 
                { 
                    CreatedAt = l.CreatedAt,
                })
                .ToListAsync();

            var recentLogCount = recentLogs.Count();
            var auditLogStatistics = await _context.TblAuditLogs
                .GroupBy(a => a.ActionType)
                .Select(g => new ActionStatistic
                {
                    ActionType = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            var viewModel = new DashboardViewModel
            {
                ActiveSessions = activeSessions,
                RecentLogCount = recentLogCount,
                RecentLogs = recentLogs,
                AuditLogStatistics = auditLogStatistics
            };

            return View(viewModel);
        }
    }
}
