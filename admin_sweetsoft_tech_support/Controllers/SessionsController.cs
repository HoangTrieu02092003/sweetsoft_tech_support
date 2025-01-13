using admin_sweetsoft_tech_support.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace admin_sweetsoft_tech_support.Controllers
{
    public class SessionsController : Controller
    {
        private readonly RequestContext _context;

        public SessionsController(RequestContext context)
        {
            _context = context;
        }

        // Danh sách Sessions
        public async Task<IActionResult> Index(string name, string status, int page = 1)
        {
            var currentUserIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserIdString) || !int.TryParse(currentUserIdString, out int currentUserId))
            {
                TempData["ReturnUrl"] = Request.Path.ToString();
                return RedirectToAction("Login", "Admin");
            }
            var userRole = _context.TblUsers
                 .Where(u => u.UserId == currentUserId)
                 .FirstOrDefault();

            // Kiểm tra nếu người dùng không phải là admin
            if (userRole.IsAdmin == false)
            {
                TempData["ErrorMessage"] = "Tài khoản hiện tại không có quyền vào trang này.";
                return RedirectToAction("Index1", "Report");
            }
            var sessions = _context.TblSessions
                .Include(s => s.User)
                .AsQueryable();

            // Lọc theo User tên
            if (!string.IsNullOrEmpty(name))
            {
                sessions = sessions.Where(l => l.User.FullName.Contains(name));
            }

            // Lọc theo Trạng thái
            if (status == "active")
            {
                sessions = sessions.Where(s => s.ExpiresAt > DateTime.Now);
            }
            else if (status == "expired")
            {
                sessions = sessions.Where(s => s.ExpiresAt <= DateTime.Now);
            }

            var pageSize = 5; // số lượng session mỗi trang
            var skip = (page - 1) * pageSize;

            var requestContext = sessions
                .OrderByDescending(t => t.CreatedAt)
                .Skip(skip) // bỏ qua dữ liệu đã xem ở các trang trước
                .Take(pageSize);

            var totalSessions = await sessions.CountAsync();

            // Tính tổng số trang
            var totalPages = (int)Math.Ceiling(totalSessions / (double)pageSize);

            // Chuyển dữ liệu sang View
            ViewData["name"] = name;
            ViewData["status"] = status;
            ViewData["TotalPages"] = totalPages;
            ViewData["CurrentPage"] = page;

            return View(await requestContext.ToListAsync());
        }

        [HttpPost]
        public IActionResult EndSession(int id)
        {
            var session = _context.TblSessions.Find(id);
            if (session != null)
            {
                session.ExpiresAt = DateTime.Now;
                _context.SaveChanges();
                TempData["Message"] = "Phiên làm việc đã được kết thúc.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
