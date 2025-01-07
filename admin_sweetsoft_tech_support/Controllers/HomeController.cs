using admin_sweetsoft_tech_support.Attributes;
using admin_sweetsoft_tech_support.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace admin_sweetsoft_tech_support.Controllers
{
    public class HomeController : Controller
    {
        private readonly RequestContext _context;
        private readonly LogService _logService;

        public HomeController(RequestContext context, LogService logService)
        {
            _context = context;
            _logService = logService;
        }
        public IActionResult AccessDenied(string permission)
        {
            TempData["ErrorMessage"] = "Bạn không có quyền truy cập vào chức năng này.";
            ViewData["permission"] = permission;
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestPermission(string requestedPermission)
        {
            // Kiểm tra thông tin người dùng
            var admin = await _context.TblUsers.FirstOrDefaultAsync(u => u.IsAdmin == true);

            if (admin == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy quản lý bộ phận.";
                return RedirectToAction(nameof(AccessDenied));
            }

            try
            {
                // Gửi thông báo đến quản lý bộ phận
                _logService.LogNotificationAction(
                    admin.UserId.ToString(),$"Yêu cầu cấp quyền",
                    $"Yêu cầu cấp quyền từ {User.Identity.Name} với quyền: {requestedPermission}"
                );

                TempData["SuccessMessage"] = "Yêu cầu cấp quyền đã được gửi thành công.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi gửi yêu cầu cấp quyền: {ex.Message}";
                return RedirectToAction(nameof(Error));
            }

            return RedirectToAction(nameof(AccessDenied));
        }
    }
}
