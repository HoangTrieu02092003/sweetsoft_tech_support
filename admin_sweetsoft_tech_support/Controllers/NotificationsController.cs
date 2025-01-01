using admin_sweetsoft_tech_support.Attributes;
using admin_sweetsoft_tech_support.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Dynamic;
using System.Globalization;
using System.Security.AccessControl;
using System.Security.Claims;

namespace admin_sweetsoft_tech_support.Controllers
{
    public class NotificationsController : Controller
    {
        private readonly RequestContext _context;

        public NotificationsController(RequestContext context)
        {
            _context = context;
        }

        // GET: Notifications
        public async Task<IActionResult> Index(string search, int? status, int page = 1, int filePage = 1)
        {
            ViewData["Search"] = search;
            ViewData["Status"] = status;

            // Truy vấn thông báo
            var notifications = _context.TblNotifications
                .Include(n => n.User)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                notifications = notifications.Where(n => n.Message.Contains(search));
            }

            if (status.HasValue)
            {
                notifications = notifications.Where(u => u.Status == status.Value);
            }

            var logsFromDb = await notifications.Skip((page - 1) * 10).Take(10).ToListAsync();
            var logsFromFile = await GetLogsFromFile();
            var paginatedFileLogs = logsFromFile
               .Skip((filePage - 1) * 10)
               .Take(10)
               .ToList();
            var logs = logsFromDb.Select(log =>
            {
                dynamic logItem = new ExpandoObject();
                logItem.NotificationId = log.NotificationId;
                logItem.UserId = log.UserId;
                logItem.Message = log.Message;
                logItem.Status = log.Status;
                logItem.CreatedAt = log.CreatedAt;
                logItem.User = log.User;
                return logItem;
            }).ToList();

            var totalNotification = await notifications.CountAsync();

            // Tính tổng số trang
            var totalPages = (int)Math.Ceiling((double)totalNotification / 10);
            var FileTotalPages = (int)Math.Ceiling((double)logsFromFile.Count / 10);

            ViewData["DbPagination"] = new Pagination { CurrentPage = page, TotalPages = totalPages };
            ViewData["FilePagination"] = new Pagination { CurrentPage = filePage, TotalPages = FileTotalPages };
            return View(new Tuple<List<dynamic>, List<dynamic>>(logs, paginatedFileLogs));
        }

        public IActionResult MyNotifications(string sortColumn, string sortOrder)
        {
            // Lấy thông tin userId từ người dùng đang đăng nhập
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Admin"); // Chuyển hướng đến trang đăng nhập nếu hết session
            }

            // Truy vấn thông báo của người dùng từ cơ sở dữ liệu
            var notifications = _context.TblNotifications
                .Where(n => n.UserId == int.Parse(userId)).ToList();

            // Lấy thông tin log từ file
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Logs", "notification.log");

            if (System.IO.File.Exists(filePath))
            {
                var logLines = System.IO.File.ReadAllLines(filePath);
                var logNotifications = new List<TblNotification>();
                foreach (var line in logLines)
                {
                    // Tách các phần từ log
                    var logParts = line.Split(new string[] { ": " }, StringSplitOptions.None);

                    if (logParts.Length == 2)
                    {
                        var dateTime = logParts[0];
                        var logDetails = logParts[1].Split(", ");

                        var userIdLog = logDetails.FirstOrDefault(detail => detail.StartsWith("UserId"))?.Split('=')[1].Trim();
                        var message = logDetails.FirstOrDefault(detail => detail.StartsWith("Message"))?.Split('=')[1].Trim();
                        var status = logDetails.FirstOrDefault(detail => detail.StartsWith("Status"))?.Split('=')[1].Trim();

                        if (userIdLog == userId)
                        {
                            DateTime parsedDateTime;
                            if (DateTime.TryParseExact(dateTime, "MM/dd/yyyy h:mm:ss tt", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDateTime))
                            {
                                logNotifications.Add(new TblNotification 
                                {
                                    UserId = int.Parse(userIdLog),
                                    Message = message,
                                    Status = short.Parse(status),
                                    CreatedAt = parsedDateTime,
                                });
                            }
                        }
                    }
                }

                notifications.AddRange(logNotifications);
            }
            if (!string.IsNullOrEmpty(sortColumn))
            {
                notifications = TableSorter.Sort<TblNotification>(notifications.AsQueryable(), sortColumn, sortOrder);
            }
            ViewData["SortColumn"] = sortColumn;
            ViewData["SortOrder"] = sortOrder;
            ViewBag.Notifications = notifications;
            return View(notifications);
        }

        private async Task<List<dynamic>> GetLogsFromFile()
        {
            var logs = new List<dynamic>();
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Logs", "notification.log");

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
                        var message = logDetails.FirstOrDefault(detail => detail.StartsWith("Message"))?.Split('=')[1].Trim();
                        var status = logDetails.FirstOrDefault(detail => detail.StartsWith("status"))?.Split('=')[1].Trim();
                        var createByUser = await _context.TblUsers.FindAsync(int.Parse(userId));
                        var user = createByUser?.FullName;
                        DateTime parsedDateTime;
                        if (DateTime.TryParseExact(dateTime, "MM/dd/yyyy h:mm:ss tt", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDateTime))
                        {
                            logs.Add(new
                            {
                                UserId = userId,
                                Message = message,
                                Status = status,
                                CreatedAt = parsedDateTime,
                                User = new { FullName = user },
                            });
                        }
                    }
                }
            }

            return logs;
        }

        public int GetUnreadNotificationsCount()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return 0; // Nếu chưa đăng nhập, trả về 0
            }

            // Đếm số lượng thông báo chưa đọc cho người dùng hiện tại
            var count = _context.TblNotifications
                .Where(n => n.UserId == int.Parse(userId) && n.Status == 0)
                .Count();

            return count;
        }

        // POST: Notifications/MarkAsRead
        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var notification = await _context.TblNotifications.FindAsync(id);
            if (notification == null) return NotFound();

            notification.Status = 1; // Đánh dấu là "Đã đọc"
            _context.Update(notification);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // POST: Notifications/Delete/5
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var notification = await _context.TblNotifications.FindAsync(id);
            if (notification == null) return NotFound();

            _context.TblNotifications.Remove(notification);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
