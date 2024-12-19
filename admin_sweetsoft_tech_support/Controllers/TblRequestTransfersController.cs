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
    public class TblRequestTransfersController : Controller
    {
        private readonly RequestContext _context;

        public TblRequestTransfersController(RequestContext context)
        {
            _context = context;
        }

        // GET: TblRequestTransfers
        public async Task<IActionResult> Index()
        {
            var requestContext = _context.TblRequestTransfers.Include(t => t.FromDepartment).Include(t => t.Request).Include(t => t.ToDepartment).Include(t => t.TransferredByNavigation);
            return View(await requestContext.ToListAsync());
        }

        // GET: TblRequestTransfers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tblRequestTransfer = await _context.TblRequestTransfers
                .Include(t => t.FromDepartment)
                .Include(t => t.Request)
                .Include(t => t.ToDepartment)
                .Include(t => t.TransferredByNavigation)
                .FirstOrDefaultAsync(m => m.TransferId == id);
            if (tblRequestTransfer == null)
            {
                return NotFound();
            }

            return View(tblRequestTransfer);
        }

        // GET: TblRequestTransfers/Create
        public IActionResult Create()
        {
            ViewData["FromDepartmentId"] = new SelectList(_context.TblDepartments, "DepartmentId", "DepartmentId");
            ViewData["RequestId"] = new SelectList(_context.TblSupportRequests, "RequestId", "RequestId");
            ViewData["ToDepartmentId"] = new SelectList(_context.TblDepartments, "DepartmentId", "DepartmentId");
            ViewData["TransferredBy"] = new SelectList(_context.TblUsers, "UserId", "UserId");
            ViewBag.RequestId = new SelectList(_context.TblSupportRequests, "RequestId", "RequestDetails");
            ViewBag.FromDepartmentId = new SelectList(_context.TblDepartments, "DepartmentId", "DepartmentName");
            ViewBag.ToDepartmentId = new SelectList(_context.TblDepartments, "DepartmentId", "DepartmentName");
            ViewBag.TransferredBy = new SelectList(_context.TblUsers, "UserId", "FullName");
            return View();
        }

        // POST: TblRequestTransfers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TransferId,RequestId,FromDepartmentId,ToDepartmentId,Priority,TransferredBy,TransferredAt,Note")] TblRequestTransfer tblRequestTransfer)
        {
            if (ModelState.IsValid)
            {
                _context.Add(tblRequestTransfer);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["FromDepartmentId"] = new SelectList(_context.TblDepartments, "DepartmentId", "DepartmentId", tblRequestTransfer.FromDepartmentId);
            ViewData["RequestId"] = new SelectList(_context.TblSupportRequests, "RequestId", "RequestId", tblRequestTransfer.RequestId);
            ViewData["ToDepartmentId"] = new SelectList(_context.TblDepartments, "DepartmentId", "DepartmentId", tblRequestTransfer.ToDepartmentId);
            ViewData["TransferredBy"] = new SelectList(_context.TblUsers, "UserId", "UserId", tblRequestTransfer.TransferredBy);
            return View(tblRequestTransfer);
        }

        // GET: TblRequestTransfers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tblRequestTransfer = await _context.TblRequestTransfers.FindAsync(id);
            if (tblRequestTransfer == null)
            {
                return NotFound();
            }
            ViewData["FromDepartmentId"] = new SelectList(_context.TblDepartments, "DepartmentId", "DepartmentId", tblRequestTransfer.FromDepartmentId);
            ViewData["RequestId"] = new SelectList(_context.TblSupportRequests, "RequestId", "RequestId", tblRequestTransfer.RequestId);
            ViewData["ToDepartmentId"] = new SelectList(_context.TblDepartments, "DepartmentId", "DepartmentId", tblRequestTransfer.ToDepartmentId);
            ViewData["TransferredBy"] = new SelectList(_context.TblUsers, "UserId", "UserId", tblRequestTransfer.TransferredBy);
            ViewBag.RequestId = new SelectList(_context.TblSupportRequests, "RequestId", "RequestDetails");
            ViewBag.FromDepartmentId = new SelectList(_context.TblDepartments, "DepartmentId", "DepartmentName");
            ViewBag.ToDepartmentId = new SelectList(_context.TblDepartments, "DepartmentId", "DepartmentName");
            ViewBag.TransferredBy = new SelectList(_context.TblUsers, "UserId", "FullName");
            return View(tblRequestTransfer);
        }

        // POST: TblRequestTransfers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TransferId,RequestId,FromDepartmentId,ToDepartmentId,Priority,TransferredBy,TransferredAt,Note")] TblRequestTransfer tblRequestTransfer)
        {
            if (id != tblRequestTransfer.TransferId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tblRequestTransfer);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TblRequestTransferExists(tblRequestTransfer.TransferId))
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
            ViewData["FromDepartmentId"] = new SelectList(_context.TblDepartments, "DepartmentId", "DepartmentId", tblRequestTransfer.FromDepartmentId);
            ViewData["RequestId"] = new SelectList(_context.TblSupportRequests, "RequestId", "RequestId", tblRequestTransfer.RequestId);
            ViewData["ToDepartmentId"] = new SelectList(_context.TblDepartments, "DepartmentId", "DepartmentId", tblRequestTransfer.ToDepartmentId);
            ViewData["TransferredBy"] = new SelectList(_context.TblUsers, "UserId", "UserId", tblRequestTransfer.TransferredBy);
            return View(tblRequestTransfer);
        }

        // GET: TblRequestTransfers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tblRequestTransfer = await _context.TblRequestTransfers
                .Include(t => t.FromDepartment)
                .Include(t => t.Request)
                .Include(t => t.ToDepartment)
                .Include(t => t.TransferredByNavigation)
                .FirstOrDefaultAsync(m => m.TransferId == id);
            if (tblRequestTransfer == null)
            {
                return NotFound();
            }

            return View(tblRequestTransfer);
        }

        // POST: TblRequestTransfers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tblRequestTransfer = await _context.TblRequestTransfers.FindAsync(id);
            if (tblRequestTransfer != null)
            {
                _context.TblRequestTransfers.Remove(tblRequestTransfer);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TblRequestTransferExists(int id)
        {
            return _context.TblRequestTransfers.Any(e => e.TransferId == id);
        }
    }
}
