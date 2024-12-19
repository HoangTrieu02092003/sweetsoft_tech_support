using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using admin_sweetsoft_tech_support.Models;

namespace admin_sweetsoft_tech_support.Controllers
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
            var requestContext = _context.TblRequestsProcessings.Include(t => t.Department).Include(t => t.Request);
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

        // GET: TblRequestsProcessings/Create
        public IActionResult Create()
        {
            ViewData["DepartmentId"] = new SelectList(_context.TblDepartments, "DepartmentId", "DepartmentId");
            ViewData["RequestId"] = new SelectList(_context.TblSupportRequests, "RequestId", "RequestId");
            ViewBag.RequestId = new SelectList(_context.TblSupportRequests, "RequestId", "RequestDetails");
            ViewBag.DepartmentId = new SelectList(_context.TblDepartments, "DepartmentId", "DepartmentName");
            return View();
        }

        // POST: TblRequestsProcessings/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProcessId,RequestId,DepartmentId,IsCompleted,ProcessedAt,Note")] TblRequestsProcessing tblRequestsProcessing)
        {
            if (ModelState.IsValid)
            {
                _context.Add(tblRequestsProcessing);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["DepartmentId"] = new SelectList(_context.TblDepartments, "DepartmentId", "DepartmentId", tblRequestsProcessing.DepartmentId);
            ViewData["RequestId"] = new SelectList(_context.TblSupportRequests, "RequestId", "RequestId", tblRequestsProcessing.RequestId);
            return View(tblRequestsProcessing);
        }

        // GET: TblRequestsProcessings/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tblRequestsProcessing = await _context.TblRequestsProcessings.FindAsync(id);
            if (tblRequestsProcessing == null)
            {
                return NotFound();
            }
            ViewData["DepartmentId"] = new SelectList(_context.TblDepartments, "DepartmentId", "DepartmentId", tblRequestsProcessing.DepartmentId);
            ViewData["RequestId"] = new SelectList(_context.TblSupportRequests, "RequestId", "RequestId", tblRequestsProcessing.RequestId);
            ViewBag.RequestId = new SelectList(_context.TblSupportRequests, "RequestId", "RequestDetails");
            ViewBag.DepartmentId = new SelectList(_context.TblDepartments, "DepartmentId", "DepartmentName");
            return View(tblRequestsProcessing);
        }

        // POST: TblRequestsProcessings/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProcessId,RequestId,DepartmentId,IsCompleted,ProcessedAt,Note")] TblRequestsProcessing tblRequestsProcessing)
        {
            if (id != tblRequestsProcessing.ProcessId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tblRequestsProcessing);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TblRequestsProcessingExists(tblRequestsProcessing.ProcessId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["DepartmentId"] = new SelectList(_context.TblDepartments, "DepartmentId", "DepartmentId", tblRequestsProcessing.DepartmentId);
            ViewData["RequestId"] = new SelectList(_context.TblSupportRequests, "RequestId", "RequestId", tblRequestsProcessing.RequestId);           
            return View(tblRequestsProcessing);
        }

        // GET: TblRequestsProcessings/Delete/5
        public async Task<IActionResult> Delete(int? id)
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

        // POST: TblRequestsProcessings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tblRequestsProcessing = await _context.TblRequestsProcessings.FindAsync(id);
            if (tblRequestsProcessing != null)
            {
                _context.TblRequestsProcessings.Remove(tblRequestsProcessing);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TblRequestsProcessingExists(int id)
        {
            return _context.TblRequestsProcessings.Any(e => e.ProcessId == id);
        }
    }
}
