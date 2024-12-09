using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using admin_sweetsoft_tech_support.Models;
using Microsoft.EntityFrameworkCore;

namespace admin_sweetsoft_tech_support.Controllers
{
    public class AdminController : Controller
    {
        private readonly RequestContext _context;
        private readonly ILogger<AdminController> _logger;

        public AdminController(RequestContext context, ILogger<AdminController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Trang Login (GET)
        public IActionResult Login()
        {
            return View();
        }

        // Xử lý Login (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password)
        {
            var user = await _context.TblUsers
                .FirstOrDefaultAsync(u => u.Username == username && u.IsAdmin == true); // Kiểm tra admin

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Username không tồn tại hoặc không phải quản trị viên.");
                return View();
            }

            // Kiểm tra mật khẩu
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(user.Password);
            if (BCrypt.Net.BCrypt.Verify(password, hashedPassword)) // Sử dụng BCrypt để so sánh mật khẩu
            {
                // Tạo các Claims và Identity cho người dùng đã đăng nhập
                var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, "Admin") // Thêm quyền admin cho người dùng
            };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                // Đăng nhập và lưu thông tin vào Cookie
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

                return RedirectToAction("Index", "Home"); // Sau khi đăng nhập, chuyển tới trang chính của quản trị viên
            }

            ModelState.AddModelError(string.Empty, "Mật khẩu sai.");
            return View();
        }

        // Đăng xuất (Logout)
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
}