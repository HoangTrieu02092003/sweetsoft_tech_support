using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Customer_sweetsoft_tech_support.Models;
using System.Security.Claims;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;

namespace Customer_sweetsoft_tech_support.Controllers
{
    public class TblRequestsProcessingsController : Controller
    {
        private readonly RequestContext _context;

        public TblRequestsProcessingsController(RequestContext context)
        {
            _context = context;
        }

        // GET: TblRequestsProcessings
        public async Task<IActionResult> Index()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Custommer");
            }
            var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var requestContext = _context.TblRequestsProcessings
                .Where(t => t.Request.CustomerId == int.Parse(id))
                .Include(t => t.Department)
                .Include(t => t.Request);

            return View(await requestContext.ToListAsync());
        }

        // GET: TblRequestsProcessings/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tblRequestsProcessing = await _context.TblRequestsProcessings
                .Include(t => t.Department)
                .Include(t => t.Request)
                .FirstOrDefaultAsync(m => m.ProcessId == id);
            if (tblRequestsProcessing == null)
            {
                return NotFound();
            }

            return View(tblRequestsProcessing);
        }
    }
}
