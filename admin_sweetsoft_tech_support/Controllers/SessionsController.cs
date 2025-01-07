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

            var pageSize = 6; // số lượng người dùng mỗi trang
            var skip = (page - 1) * pageSize;

            var requestContext = sessions
                .OrderByDescending(t => t.CreatedAt)
                .Skip(skip) // bỏ qua dữ liệu đã xem ở các trang trước
                .Take(pageSize);

            var totalUsers = await sessions.CountAsync();


            // Tính tổng số trang
            var totalPages = (int)Math.Ceiling(totalUsers / (double)pageSize);

            // Chuyển dữ liệu sang View
            ViewData["name"] = name;
            ViewData["status"] = status;
            ViewData["TotalPages"] = totalPages;
            ViewData["CurrentPage"] = page;
            return View(await requestContext.ToListAsync());
        }

        // Kết thúc Session
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
