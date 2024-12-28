using admin_sweetsoft_tech_support.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Dynamic;

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
        public async Task<IActionResult> Index(string search, int? userId, int page = 1, int filePage = 1)
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
            var logsFromDb = await logs.Skip((page - 1) * 10).Take(10).ToListAsync();
            var logsFromFile = await GetLogsFromFile();
            var paginatedFileLogs = logsFromFile
              .Skip((filePage - 1) * 10)
              .Take(10)
              .ToList();
            var logsActivity = logsFromDb.Select(log =>
            {
                dynamic logItem = new ExpandoObject();
                logItem.ActivityId = log.ActivityId;
                logItem.RequestId = log.RequestId;
                logItem.UserId = log.UserId;
                logItem.Action = log.Action;
                logItem.CreatedAt = log.CreatedAt;
                logItem.User = log.User;
                logItem.Request = log.Request;
                return logItem;
            }).ToList();

            var totalLog = await logs.CountAsync();

            // Tính tổng số trang
            var totalPages = (int)Math.Ceiling((double)totalLog / 10);
            var FileTotalPages = (int)Math.Ceiling((double)logsFromFile.Count / 10);

            ViewData["DbPagination"] = new Pagination { CurrentPage = page, TotalPages = totalPages };
            ViewData["FilePagination"] = new Pagination { CurrentPage = filePage, TotalPages = FileTotalPages };

            ViewData["Search"] = search;
            ViewData["userId"] = userId;
            ViewData["Users"] = await _context.TblUsers.ToListAsync();

            return View(new Tuple<List<dynamic>, List<dynamic>>(logsActivity, paginatedFileLogs));
        }

        private async Task<List<dynamic>> GetLogsFromFile()
        {
            var logs = new List<dynamic>();
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Logs", "activity.log");

            if (System.IO.File.Exists(filePath))
            {
                var logLines = System.IO.File.ReadAllLines(filePath);

                foreach (var line in logLines)
                {
                    // Tách các phần từ log
                    var logParts = line.Split(new string[] { ": " }, StringSplitOptions.None);

                    if (logParts.Length == 2)
                    {
                        var dateTime = logParts[0];
                        var logDetails = logParts[1].Split(", ");

                        var userId = logDetails.FirstOrDefault(detail => detail.StartsWith("UserId"))?.Split('=')[1].Trim();
                        var requestId = logDetails.FirstOrDefault(detail => detail.StartsWith("RequestId"))?.Split('=')[1].Trim();
                        var action = logDetails.FirstOrDefault(detail => detail.StartsWith("Action"))?.Split('=')[1].Trim();
                        var createByUser = await _context.TblUsers.FindAsync(int.Parse(userId));
                        var user = createByUser?.FullName;
                        var requestNavigater = await _context.TblSupportRequests.FindAsync(int.Parse(requestId));
                        var request = requestNavigater?.RequestId;
                        logs.Add(new
                        {
                            UserId = userId,
                            Message = requestId,
                            Status = action,
                            CreatedAt = DateTime.Parse(dateTime),
                            User = new { FullName = user },
                            Request = new { RequestId = request },
                        });
                    }
                }
            }

            return logs;
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
