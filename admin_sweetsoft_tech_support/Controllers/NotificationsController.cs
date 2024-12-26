using admin_sweetsoft_tech_support.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
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
        public async Task<IActionResult> Index(string search, int? status)
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

            ViewBag.Notifications = notifications;
            return View(notifications.ToList());
        }

        public IActionResult MyNotifications()
        {
            // Lấy thông tin userId từ người dùng đang đăng nhập
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Admin"); // Chuyển hướng đến trang đăng nhập nếu hết session
            }

            // Truy vấn thông báo của người dùng
            var notifications = _context.TblNotifications
                .Where(n => n.UserId == int.Parse(userId))
                .OrderByDescending(n => n.CreatedAt)
                .ToList();
            ViewBag.Notifications = notifications;
            return View(notifications);
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
