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
    public class TblUserPermissionsController : Controller
    {
        private readonly RequestContext _context;

        public TblUserPermissionsController(RequestContext context)
        {
            _context = context;
        }

        // GET: TblUserPermissions
        public async Task<IActionResult> Index()
        {
            var requestContext = _context.TblUserPermissions.Include(t => t.Permission).Include(t => t.User);
            return View(await requestContext.ToListAsync());
        }

        // GET: TblUserPermissions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tblUserPermission = await _context.TblUserPermissions
                .Include(t => t.Permission)
                .Include(t => t.User)
                .FirstOrDefaultAsync(m => m.UserPermissionId == id);
            if (tblUserPermission == null)
            {
                return NotFound();
            }

            return View(tblUserPermission);
        }

        // GET: TblUserPermissions/Create
        public IActionResult Create()
        {
            ViewData["PermissionId"] = new SelectList(_context.TblPermissions, "PermissionId", "PermissionId");
            ViewData["UserId"] = new SelectList(_context.TblUsers, "UserId", "UserId");
            return View();
        }

        // POST: TblUserPermissions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UserPermissionId,UserId,PermissionId")] TblUserPermission tblUserPermission)
        {
            if (ModelState.IsValid)
            {
                _context.Add(tblUserPermission);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["PermissionId"] = new SelectList(_context.TblPermissions, "PermissionId", "PermissionId", tblUserPermission.PermissionId);
            ViewData["UserId"] = new SelectList(_context.TblUsers, "UserId", "UserId", tblUserPermission.UserId);
            return View(tblUserPermission);
        }

        // GET: TblUserPermissions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tblUserPermission = await _context.TblUserPermissions.FindAsync(id);
            if (tblUserPermission == null)
            {
                return NotFound();
            }
            ViewData["PermissionId"] = new SelectList(_context.TblPermissions, "PermissionId", "PermissionId", tblUserPermission.PermissionId);
            ViewData["UserId"] = new SelectList(_context.TblUsers, "UserId", "UserId", tblUserPermission.UserId);
            return View(tblUserPermission);
        }

        // POST: TblUserPermissions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("UserPermissionId,UserId,PermissionId")] TblUserPermission tblUserPermission)
        {
            if (id != tblUserPermission.UserPermissionId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tblUserPermission);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TblUserPermissionExists(tblUserPermission.UserPermissionId))
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
            ViewData["PermissionId"] = new SelectList(_context.TblPermissions, "PermissionId", "PermissionId", tblUserPermission.PermissionId);
            ViewData["UserId"] = new SelectList(_context.TblUsers, "UserId", "UserId", tblUserPermission.UserId);
            return View(tblUserPermission);
        }

        // GET: TblUserPermissions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tblUserPermission = await _context.TblUserPermissions
                .Include(t => t.Permission)
                .Include(t => t.User)
                .FirstOrDefaultAsync(m => m.UserPermissionId == id);
            if (tblUserPermission == null)
            {
                return NotFound();
            }

            return View(tblUserPermission);
        }

        // POST: TblUserPermissions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tblUserPermission = await _context.TblUserPermissions.FindAsync(id);
            if (tblUserPermission != null)
            {
                _context.TblUserPermissions.Remove(tblUserPermission);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TblUserPermissionExists(int id)
        {
            return _context.TblUserPermissions.Any(e => e.UserPermissionId == id);
        }
    }
}
