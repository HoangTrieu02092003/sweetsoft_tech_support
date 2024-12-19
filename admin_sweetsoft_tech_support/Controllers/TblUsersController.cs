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
    public class TblUsersController : Controller
    {
        private readonly RequestContext _context;

        public TblUsersController(RequestContext context)
        {
            _context = context;
        }

        // GET: TblUsers
        public async Task<IActionResult> Index()
        {
            var requestContext = _context.TblUsers.Include(t => t.CreatedUserNavigation).Include(t => t.Department).Include(t => t.Role).Include(t => t.UpdatedUserNavigation);
            return View(await requestContext.ToListAsync());
        }

        // GET: TblUsers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tblUser = await _context.TblUsers
                .Include(t => t.CreatedUserNavigation)
                .Include(t => t.Department)
                .Include(t => t.Role)
                .Include(t => t.UpdatedUserNavigation)
                .FirstOrDefaultAsync(m => m.UserId == id);
            if (tblUser == null)
            {
                return NotFound();
            }

            return View(tblUser);
        }

        // GET: TblUsers/Create
        public IActionResult Create()
        {
            ViewData["CreatedUser"] = new SelectList(_context.TblUsers, "UserId", "UserId");
            ViewData["DepartmentId"] = new SelectList(_context.TblDepartments, "DepartmentId", "DepartmentId");
            ViewData["RoleId"] = new SelectList(_context.TblRoles, "RoleId", "RoleId");
            ViewData["UpdatedUser"] = new SelectList(_context.TblUsers, "UserId", "UserId");
            return View();
        }

        // POST: TblUsers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UserId,FullName,Email,Phone,Username,Password,RoleId,DepartmentId,Status,IsAdmin,ResetToken,ResetTokenExpiry,CreatedUser,CreatedAt,UpdatedUser,UpdatedAt")] TblUser tblUser)
        {
            if (ModelState.IsValid)
            {
                _context.Add(tblUser);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CreatedUser"] = new SelectList(_context.TblUsers, "UserId", "UserId", tblUser.CreatedUser);
            ViewData["DepartmentId"] = new SelectList(_context.TblDepartments, "DepartmentId", "DepartmentId", tblUser.DepartmentId);
            ViewData["RoleId"] = new SelectList(_context.TblRoles, "RoleId", "RoleId", tblUser.RoleId);
            ViewData["UpdatedUser"] = new SelectList(_context.TblUsers, "UserId", "UserId", tblUser.UpdatedUser);
            return View(tblUser);
        }

        // GET: TblUsers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tblUser = await _context.TblUsers.FindAsync(id);
            if (tblUser == null)
            {
                return NotFound();
            }
            ViewData["CreatedUser"] = new SelectList(_context.TblUsers, "UserId", "UserId", tblUser.CreatedUser);
            ViewData["DepartmentId"] = new SelectList(_context.TblDepartments, "DepartmentId", "DepartmentId", tblUser.DepartmentId);
            ViewData["RoleId"] = new SelectList(_context.TblRoles, "RoleId", "RoleId", tblUser.RoleId);
            ViewData["UpdatedUser"] = new SelectList(_context.TblUsers, "UserId", "UserId", tblUser.UpdatedUser);
            return View(tblUser);
        }

        // POST: TblUsers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("UserId,FullName,Email,Phone,Username,Password,RoleId,DepartmentId,Status,IsAdmin,ResetToken,ResetTokenExpiry,CreatedUser,CreatedAt,UpdatedUser,UpdatedAt")] TblUser tblUser)
        {
            if (id != tblUser.UserId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tblUser);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TblUserExists(tblUser.UserId))
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
            ViewData["CreatedUser"] = new SelectList(_context.TblUsers, "UserId", "UserId", tblUser.CreatedUser);
            ViewData["DepartmentId"] = new SelectList(_context.TblDepartments, "DepartmentId", "DepartmentId", tblUser.DepartmentId);
            ViewData["RoleId"] = new SelectList(_context.TblRoles, "RoleId", "RoleId", tblUser.RoleId);
            ViewData["UpdatedUser"] = new SelectList(_context.TblUsers, "UserId", "UserId", tblUser.UpdatedUser);
            return View(tblUser);
        }

        // GET: TblUsers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tblUser = await _context.TblUsers
                .Include(t => t.CreatedUserNavigation)
                .Include(t => t.Department)
                .Include(t => t.Role)
                .Include(t => t.UpdatedUserNavigation)
                .FirstOrDefaultAsync(m => m.UserId == id);
            if (tblUser == null)
            {
                return NotFound();
            }

            return View(tblUser);
        }

        // POST: TblUsers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tblUser = await _context.TblUsers.FindAsync(id);
            if (tblUser != null)
            {
                _context.TblUsers.Remove(tblUser);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TblUserExists(int id)
        {
            return _context.TblUsers.Any(e => e.UserId == id);
        }
    }
}
