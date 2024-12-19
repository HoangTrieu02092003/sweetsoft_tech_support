﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using admin_sweetsoft_tech_support.Models;

namespace admin_sweetsoft_tech_support.Controllers
{
    public class TblSupportRequestsController : Controller
    {
        private readonly RequestContext _context;

        public TblSupportRequestsController(RequestContext context)
        {
            _context = context;
        }

        // GET: TblSupportRequests
        public async Task<IActionResult> Index()
        {
            var requestContext = _context.TblSupportRequests.Include(t => t.Customer).Include(t => t.Department);
            return View(await requestContext.ToListAsync());
        }

        // GET: TblSupportRequests/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tblSupportRequest = await _context.TblSupportRequests
                .Include(t => t.Customer)
                .Include(t => t.Department)
                .FirstOrDefaultAsync(m => m.RequestId == id);
            if (tblSupportRequest == null)
            {
                return NotFound();
            }
            ViewBag.CustomerName = tblSupportRequest.Customer?.FullName??"Unknown"; // Assuming the Customer entity has a Name property
            ViewBag.DepartmentName = tblSupportRequest.Department?.DepartmentName ?? "Unknown";

            return View(tblSupportRequest);
        }

        // GET: TblSupportRequests/Create
        public IActionResult Create()
        {
            ViewData["CustomerId"] = new SelectList(_context.TblCustomers, "CustomerId", "CustomerId");
            ViewData["DepartmentId"] = new SelectList(_context.TblDepartments, "DepartmentId", "DepartmentId");
            ViewBag.CustomerId = new SelectList(_context.TblCustomers, "CustomerId", "FullName");
            ViewBag.DepartmentId = new SelectList(_context.TblDepartments, "DepartmentId", "DepartmentName");
            return View();
        }

        // POST: TblSupportRequests/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RequestId,CustomerId,DepartmentId,RequestDetails,Status,CreatedAt,ResolvedAt")] TblSupportRequest tblSupportRequest)
        {
            if (ModelState.IsValid)
            {
                _context.Add(tblSupportRequest);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CustomerId"] = new SelectList(_context.TblCustomers, "CustomerId", "CustomerId", tblSupportRequest.CustomerId);
            ViewData["DepartmentId"] = new SelectList(_context.TblDepartments, "DepartmentId", "DepartmentId", tblSupportRequest.DepartmentId);
            return View(tblSupportRequest);
        }


        public IActionResult Edit(int id)
        {
            var supportRequest = _context.TblSupportRequests.Find(id);
            if (supportRequest == null)
            {
                return NotFound();
            }

            ViewBag.CustomerId = new SelectList(_context.TblCustomers, "CustomerId", "FullName", supportRequest.CustomerId);
            ViewBag.DepartmentId = new SelectList(_context.TblDepartments, "DepartmentId", "DepartmentName", supportRequest.DepartmentId);

            return View(supportRequest);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, [Bind("RequestId,RequestDetails,Status,CustomerId,DepartmentId,CreatedAt,ResolvedAt")] TblSupportRequest supportRequest)
        {
            if (id != supportRequest.RequestId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                _context.Update(supportRequest);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.CustomerId = new SelectList(_context.TblCustomers, "CustomerId", "FullName", supportRequest.CustomerId);
            ViewBag.DepartmentId = new SelectList(_context.TblDepartments, "DepartmentId", "DepartmentName", supportRequest.DepartmentId);

            return View(supportRequest);
        }
        // GET: TblSupportRequests/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tblSupportRequest = await _context.TblSupportRequests
                .Include(t => t.Customer)
                .Include(t => t.Department)
                .FirstOrDefaultAsync(m => m.RequestId == id);
            if (tblSupportRequest == null)
            {
                return NotFound();
            }

            return View(tblSupportRequest);
        }

        // POST: TblSupportRequests/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tblSupportRequest = await _context.TblSupportRequests.FindAsync(id);
            if (tblSupportRequest != null)
            {
                _context.TblSupportRequests.Remove(tblSupportRequest);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        
        private bool TblSupportRequestExists(int id)
        {
            return _context.TblSupportRequests.Any(e => e.RequestId == id);
        }
        public async Task<IActionResult> Transfer(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tblSupportRequest = await _context.TblSupportRequests
                .Include(t => t.Customer)
                .Include(t => t.Department)
                .FirstOrDefaultAsync(m => m.RequestId == id);
            if (tblSupportRequest == null)
            {
                return NotFound();
            }

            var requestTransfer = new TblRequestTransfer
            {
                RequestId = tblSupportRequest.RequestId,
                FromDepartmentId = tblSupportRequest.DepartmentId,
                TransferredAt = DateTime.Now // Set default values as needed
            };

            ViewData["RequestId"] = new SelectList(_context.TblSupportRequests, "RequestId", "RequestDetails", tblSupportRequest.RequestId);
            ViewData["FromDepartmentId"] = new SelectList(_context.TblDepartments, "DepartmentId", "DepartmentName", tblSupportRequest.DepartmentId);
            ViewData["ToDepartmentId"] = new SelectList(_context.TblDepartments, "DepartmentId", "DepartmentName");
            ViewData["TransferredBy"] = new SelectList(_context.TblUsers, "UserId", "FullName");

            return View(requestTransfer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Transfer(int id, [Bind("TransferId,RequestId,FromDepartmentId,ToDepartmentId,Priority,TransferredBy,TransferredAt,Note")] TblRequestTransfer requestTransfer)
        {
            if (id != requestTransfer.RequestId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(requestTransfer);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TblRequestTransferExists(requestTransfer.TransferId))
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

            ViewData["RequestId"] = new SelectList(_context.TblSupportRequests, "RequestId", "RequestDetails", requestTransfer.RequestId);
            ViewData["FromDepartmentId"] = new SelectList(_context.TblDepartments, "DepartmentId", "DepartmentName", requestTransfer.FromDepartmentId);
            ViewData["ToDepartmentId"] = new SelectList(_context.TblDepartments, "DepartmentId", "DepartmentName", requestTransfer.ToDepartmentId);
            ViewData["TransferredBy"] = new SelectList(_context.TblUsers, "UserId", "FullName", requestTransfer.TransferredBy);

            return View(requestTransfer);
        }

        private bool TblRequestTransferExists(int id)
        {
            return _context.TblRequestTransfers.Any(e => e.TransferId == id);
        }
        
    }
}
