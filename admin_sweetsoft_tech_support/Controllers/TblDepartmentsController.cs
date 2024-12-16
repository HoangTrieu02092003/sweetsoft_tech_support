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
    public class TblDepartmentsController : Controller
    {
        private readonly RequestContext _context;

        public TblDepartmentsController(RequestContext context)
        {
            _context = context;
        }

        // GET: TblDepartments
        public async Task<IActionResult> Index()
        {
            return View(await _context.TblDepartments.ToListAsync());
        }

        // GET: TblDepartments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tblDepartment = await _context.TblDepartments
                .FirstOrDefaultAsync(m => m.DepartmentId == id);
            if (tblDepartment == null)
            {
                return NotFound();
            }

            return View(tblDepartment);
        }

        // GET: TblDepartments/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: TblDepartments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("DepartmentId,DepartmentName,Status")] TblDepartment tblDepartment)
        {
            if (ModelState.IsValid)
            {
                _context.Add(tblDepartment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(tblDepartment);
        }

        // GET: TblDepartments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tblDepartment = await _context.TblDepartments.FindAsync(id);
            if (tblDepartment == null)
            {
                return NotFound();
            }
            return View(tblDepartment);
        }

        // POST: TblDepartments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("DepartmentId,DepartmentName,Status")] TblDepartment tblDepartment)
        {
            
            if (id != tblDepartment.DepartmentId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tblDepartment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TblDepartmentExists(tblDepartment.DepartmentId))
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
            return View(tblDepartment);
        }

        // GET: TblDepartments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tblDepartment = await _context.TblDepartments
                .FirstOrDefaultAsync(m => m.DepartmentId == id);
            if (tblDepartment == null)
            {
                return NotFound();
            }

            return View(tblDepartment);
        }

        // POST: TblDepartments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tblDepartment = await _context.TblDepartments.FindAsync(id);

            if (tblDepartment != null)
            {
                // Kiểm tra nếu phòng ban còn người dùng liên quan
                bool hasUsers = await _context.TblUsers.AnyAsync(u => u.DepartmentId == id);
                if (hasUsers)
                {
                    // Nếu có người dùng liên quan, trả về thông báo lỗi mà không xóa phòng ban
                    return Json(new { success = false, message = "Không thể xóa vì phòng ban còn người dùng hoặc yêu cầu liên quan. Vui lòng xử lý trước khi xóa!" });
                }

                // Xóa phòng ban nếu không có người dùng liên quan
                _context.TblDepartments.Remove(tblDepartment);
                await _context.SaveChangesAsync();
            }

            return Json(new { success = true });
        }

        private bool TblDepartmentExists(int id)
        {
            return _context.TblDepartments.Any(e => e.DepartmentId == id);
        }
    }
}
