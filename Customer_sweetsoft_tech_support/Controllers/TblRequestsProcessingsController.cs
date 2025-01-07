using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Customer_sweetsoft_tech_support.Models;
using System.Security.Claims;

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
                TempData["ReturnUrl"] = Url.RouteUrl("followingRequest");
                return RedirectToAction("Login", "Custommer");
            }
            var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var requestContext = await _context.TblRequestsProcessings
                .Where(t => t.Request.CustomerId == int.Parse(id))
                .Include(t => t.Department)
                .Include(t => t.Request)
                .Select(t => new
                {
                    RequestProcessing = t,
                    HasUnreadFeedback = _context.TblRequestFeedbacks
                        .Any(f => f.RequestId == t.Request.RequestId && f.IsRead == false)
                })
                .ToListAsync();

            return View(requestContext);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SendMessage(int requestId, string message, int toUserId)
        {

            if (string.IsNullOrWhiteSpace(message))
            {
                return RedirectToAction("Index", new { requestId });
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var newFeedback = new TblRequestFeedback
            {
                RequestId = requestId,
                FromUserId = null,
                FromCustomerId = userId,
                ToUserId = null,
                ToCustomerId = null,
                Feedback = message,
                FeedbackType = 2,
                CreatedAt = DateTime.Now,
                IsRead = false
            };
            _context.TblRequestFeedbacks.Add(newFeedback);
            _context.SaveChanges();
            return RedirectToAction("Index", new { requestId });
        }
        //
        public IActionResult GetFeedbacks(int requestId)
        {
            var unreadFeedbacks = _context.TblRequestFeedbacks
                                .Where(f => f.RequestId == requestId && f.IsRead == false)
                                .ToList();

            foreach (var feedback in unreadFeedbacks)
            {
                feedback.IsRead = true;
            }

            _context.SaveChanges();

            var feedbacks = _context.TblRequestFeedbacks
                .Where(f => f.RequestId == requestId)
                .OrderBy(f => f.CreatedAt)
                .ToList();

            return PartialView("_FeedbacksPartial", feedbacks);
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int requestId)
        {
            var feedbacks = await _context.TblRequestFeedbacks
                .Where(f => f.RequestId == requestId && f.IsRead == false)
                .ToListAsync();

            if (feedbacks.Any())
            {
                feedbacks.ForEach(f => f.IsRead = true);
                await _context.SaveChangesAsync();
            }

            return Ok(new { success = true });
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
