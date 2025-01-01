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
        public async Task<IActionResult> Index(int page = 1)
        {
            int pageSize = 6; // Số lượng phòng ban trên mỗi trang
            int skip = (page - 1) * pageSize;

            // Lấy danh sách phòng ban theo phân trang
            var departments = await _context.TblDepartments
                .OrderBy(d => d.DepartmentName) // Sắp xếp theo tên phòng ban
                .Skip(skip) // Bỏ qua các mục trước đó
                .Take(pageSize) // Lấy số mục cho trang hiện tại
                .ToListAsync();

            // Tính tổng số phòng ban
            int totalDepartments = await _context.TblDepartments.CountAsync();

            // Tính tổng số trang
            int totalPages = (int)Math.Ceiling(totalDepartments / (double)pageSize);

            // Tính tổng số thành viên của từng phòng ban
            var memberCounts = departments.ToDictionary(
                d => d.DepartmentId,
                d => _context.TblUsers.Count(u => u.DepartmentId == d.DepartmentId)
            );

            // Gửi dữ liệu sang View
            ViewBag.MemberCounts = memberCounts;
            ViewData["TotalPages"] = totalPages;
            ViewData["CurrentPage"] = page;

            return View(departments);
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
                try
                {
                    _context.Add(tblDepartment);
                    await _context.SaveChangesAsync();
                    // Thêm thông báo thành công
                    TempData["SuccessMessage"] = "Phòng ban đã được tạo thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception)
                {
                    // Thêm thông báo lỗi
                    TempData["ErrorMessage"] = "Có lỗi xảy ra khi tạo phòng ban. Vui lòng thử lại!";
                }
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

            // Lấy thông tin phòng ban và danh sách nhân viên liên kết
            var tblDepartment = await _context.TblDepartments
                .Include(d => d.TblUsers) // Include để lấy danh sách nhân viên thuộc phòng ban
                .FirstOrDefaultAsync(d => d.DepartmentId == id);

            if (tblDepartment == null)
            {
                return NotFound();
            }

            // Truyền dữ liệu phòng ban vào ViewData
            ViewData["Department"] = tblDepartment;

            // Truyền danh sách nhân viên vào ViewData
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
                    // Thêm thông báo thành công
                    TempData["SuccessMessage"] = "Phòng ban đã được chỉnh sửa thành công!";
                    return RedirectToAction(nameof(Index));
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
                catch (Exception)
                {
                    // Thêm thông báo lỗi
                    TempData["ErrorMessage"] = "Có lỗi xảy ra khi chỉnh sửa phòng ban. Vui lòng thử lại!";
                }
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
                    // Lưu thông báo lỗi vào TempData
                    TempData["ErrorMessage"] = "Không thể xóa vì phòng ban còn người dùng hoặc yêu cầu liên quan. Vui lòng xử lý trước khi xóa!";
                    return RedirectToAction(nameof(Index)); // Trở lại trang danh sách phòng ban
                }

                // Xóa phòng ban nếu không có người dùng liên quan
                _context.TblDepartments.Remove(tblDepartment);
                await _context.SaveChangesAsync();

                // Lưu thông báo thành công vào TempData
                TempData["SuccessMessage"] = "Phòng ban đã được xóa thành công!";
            }

            return RedirectToAction(nameof(Index)); // Trở lại trang danh sách phòng ban
        }

        private bool TblDepartmentExists(int id)
        {
            return _context.TblDepartments.Any(e => e.DepartmentId == id);
        }
    }
}
