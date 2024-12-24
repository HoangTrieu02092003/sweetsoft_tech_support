using Microsoft.AspNetCore.Mvc;

namespace Customer_sweetsoft_tech_support.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
