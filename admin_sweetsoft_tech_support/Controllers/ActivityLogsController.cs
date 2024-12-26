using admin_sweetsoft_tech_support.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace admin_sweetsoft_tech_support.Controllers
{
    public class ActivityLogsController : Controller
    {
        private readonly RequestContext _context;

        public ActivityLogsController(RequestContext context)
        {
            _context = context;
        }

        // GET: ActivityLogs
        public async Task<IActionResult> Index(string search, int? userId)
        {
            var logs = _context.TblActivityLogs
                .Include(l => l.User)
                .Include(l => l.Request)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                logs = logs.Where(l => l.Action.Contains(search));
            }

            if (userId.HasValue)
            {
                logs = logs.Where(l => l.UserId == userId.Value);
            }

            ViewData["Search"] = search;
            ViewData["userId"] = userId;
            ViewData["Users"] = await _context.TblUsers.ToListAsync();

            return View(logs.ToList());
        }

        // POST: ActivityLogs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var activityLog = await _context.TblActivityLogs.FindAsync(id);

            if (activityLog != null)
            {
                _context.TblActivityLogs.Remove(activityLog);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
