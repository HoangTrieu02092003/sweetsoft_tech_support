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
    public class TblPermissionsController : Controller
    {
        private readonly RequestContext _context;

        public TblPermissionsController(RequestContext context)
        {
            _context = context;
        }

        // GET: TblPermissions
        public async Task<IActionResult> Index()
        {
            return View(await _context.TblPermissions.ToListAsync());
        }

        // GET: TblPermissions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tblPermission = await _context.TblPermissions
                .FirstOrDefaultAsync(m => m.PermissionId == id);
            if (tblPermission == null)
            {
                return NotFound();
            }

            return View(tblPermission);
        }

        // GET: TblPermissions/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: TblPermissions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PermissionId,PermissionName,Description")] TblPermission tblPermission)
        {
            if (ModelState.IsValid)
            {
                _context.Add(tblPermission);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(tblPermission);
        }

        // GET: TblPermissions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tblPermission = await _context.TblPermissions.FindAsync(id);
            if (tblPermission == null)
            {
                return NotFound();
            }
            return View(tblPermission);
        }

        // POST: TblPermissions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PermissionId,PermissionName,Description")] TblPermission tblPermission)
        {
            if (id != tblPermission.PermissionId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tblPermission);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TblPermissionExists(tblPermission.PermissionId))
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
            return View(tblPermission);
        }

        // GET: TblPermissions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tblPermission = await _context.TblPermissions
                .FirstOrDefaultAsync(m => m.PermissionId == id);
            if (tblPermission == null)
            {
                return NotFound();
            }

            return View(tblPermission);
        }

        // POST: TblPermissions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tblPermission = await _context.TblPermissions.FindAsync(id);
            if (tblPermission != null)
            {
                _context.TblPermissions.Remove(tblPermission);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TblPermissionExists(int id)
        {
            return _context.TblPermissions.Any(e => e.PermissionId == id);
        }
    }
}
