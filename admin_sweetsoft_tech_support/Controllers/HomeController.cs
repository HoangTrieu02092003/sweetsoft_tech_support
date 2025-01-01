using admin_sweetsoft_tech_support.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace admin_sweetsoft_tech_support.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult AccessDenied()
        {
            TempData["ErrorMessage"] = "Bạn không có quyền truy cập vào chức năng này.";
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        public IActionResult AccessDenied()
        {
            TempData["ErrorMessage"] = "B?n kh�ng c� quy?n truy c?p v�o ch?c n?ng n�y.";
            return View();
        }

    }
}
