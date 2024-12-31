using admin_sweetsoft_tech_support.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace admin_sweetsoft_tech_support.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        public IActionResult AccessDenied()
        {
            TempData["ErrorMessage"] = "B?n không có quy?n truy c?p vào ch?c n?ng này.";
            return View();
        }

    }
}
