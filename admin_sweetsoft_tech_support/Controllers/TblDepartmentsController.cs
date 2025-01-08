using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using admin_sweetsoft_tech_support.Models;
using System.Security.Claims;
using admin_sweetsoft_tech_support.Attributes;
using OfficeOpenXml;

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
            var currentUserIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserIdString) || !int.TryParse(currentUserIdString, out int currentUserId))
            {
                TempData["ReturnUrl"] = Request.Path.ToString();
                return RedirectToAction("Login", "Admin");
            }
            int pageSize = 6; // Số lượng phòng ban trên mỗi trang
            int skip = (page - 1) * pageSize;

            // Lấy danh sách phòng ban theo phân trang
            var departments = await _context.TblDepartments
                .Where(d => d.IsDelete == false)
                .OrderBy(d => d.DepartmentName) // Sắp xếp theo tên phòng ban
                .Skip(skip) // Bỏ qua các mục trước đó
                .Take(pageSize) // Lấy số mục cho trang hiện tại
                .ToListAsync();

            // Tính tổng số phòng ban
            int totalDepartments = await _context.TblDepartments.Where(u => u.IsDelete == false).CountAsync();

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

        [PermissionAuthorize("Tạo phòng ban")]
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

        [PermissionAuthorize("Sửa phòng ban")]
        // GET: TblDepartments/Edit/5
        public async Task<IActionResult> Edit(int? id, int page = 1)
        {
            int pageSize = 6;  // Số lượng nhân viên mỗi trang
            int skip = (page - 1) * pageSize;  // Tính số lượng nhân viên cần bỏ qua

            if (id == null)
            {
                return NotFound();
            }

            // Lấy thông tin phòng ban và danh sách nhân viên liên kết, phân trang danh sách nhân viên
            var tblDepartment = await _context.TblDepartments
                .Include(d => d.TblUsers) // Lấy danh sách nhân viên
                    .ThenInclude(u => u.Role) // Bao gồm thông tin Role của từng nhân viên
                .FirstOrDefaultAsync(d => d.DepartmentId == id);

            if (tblDepartment == null)
            {
                return NotFound();
            }

            // Lấy danh sách nhân viên đã phân trang
            var totalUsers = tblDepartment.TblUsers.Count();
            var usersPaged = tblDepartment.TblUsers.Skip(skip).Take(pageSize).ToList();

            // Truyền dữ liệu phòng ban vào ViewData
            ViewData["Department"] = tblDepartment;
            ViewData["UsersPaged"] = usersPaged;
            ViewData["TotalUsers"] = totalUsers;
            ViewData["CurrentPage"] = page;

            // Trả về View cùng với các dữ liệu cần thiết
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

        [PermissionAuthorize("Xóa phòng ban")]
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
                tblDepartment.IsDelete = true; // Đánh dấu là đã xóa

                _context.Update(tblDepartment);
                await _context.SaveChangesAsync();

                // Lưu thông báo thành công vào TempData
                TempData["SuccessMessage"] = "Phòng ban đã được xóa thành công!";
            }

            return RedirectToAction(nameof(Index)); // Trở lại trang danh sách phòng ban
        }

        public IActionResult ExportToExcel(int id)
        {
            Console.WriteLine(id);

            // Lấy danh sách người dùng với thông tin về Role và Department
            var users = _context.TblUsers
                .Where(u => u.DepartmentId == id && u.IsDelete == false)
                .Include(u => u.Role)  // Lấy thông tin Role từ bảng TblRoles
                .Include(u => u.Department)  // Lấy thông tin Department từ bảng TblDepartments
                .ToList();

            // Lấy tên phòng ban
            var departmentName = _context.TblDepartments
                .Where(u => u.DepartmentId == id)
                .Select(u => u.DepartmentName)
                .FirstOrDefault();

            // Tạo file Excel
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Danh sách nhân viên");

                // Thiết lập tiêu đề cho bảng
                worksheet.Cells[1, 1].Value = $"Danh sách nhân viên - Phòng ban: {departmentName}";
                worksheet.Cells[1, 1, 1, 6].Merge = true;
                worksheet.Cells[1, 1].Style.Font.Size = 16;
                worksheet.Cells[1, 1].Style.Font.Bold = true;
                worksheet.Cells[1, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                worksheet.Cells[1, 1].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

                // Thiết lập tiêu đề cho các cột
                worksheet.Cells[2, 1].Value = "Tên nhân viên";
                worksheet.Cells[2, 2].Value = "Email";
                worksheet.Cells[2, 3].Value = "Số điện thoại";
                worksheet.Cells[2, 4].Value = "Trạng thái";
                worksheet.Cells[2, 5].Value = "Nhóm quyền";
                worksheet.Cells[2, 6].Value = "Bộ phận";

                // Điền dữ liệu
                int row = 3;
                foreach (var user in users)
                {
                    worksheet.Cells[row, 1].Value = user.FullName;
                    worksheet.Cells[row, 2].Value = user.Email;
                    worksheet.Cells[row, 3].Value = user.Phone;
                    worksheet.Cells[row, 4].Value = user.Status == 1 ? "Hoạt động" : "Tạm dừng";
                    worksheet.Cells[row, 5].Value = user.Role?.RoleName;  // Hiển thị tên role (nếu có)
                    worksheet.Cells[row, 6].Value = user.Department?.DepartmentName;  // Hiển thị tên phòng ban (nếu có)
                    row++;
                }

                // Tạo viền cho bảng
                var range = worksheet.Cells[2, 1, row - 1, 6]; // Tạo phạm vi từ dòng tiêu đề đến dòng cuối

                // Cài đặt viền cho toàn bộ phạm vi
                range.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                range.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                range.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                range.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

                // Điều chỉnh chiều rộng cột cho phù hợp với nội dung
                worksheet.Cells.AutoFitColumns();

                // Tạo và trả về file Excel
                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                var fileName = $"Nhân Viên {departmentName}.xlsx";
                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                return File(stream, contentType, fileName);
            }
        }

        private bool TblDepartmentExists(int id)
        {
            return _context.TblDepartments.Any(e => e.DepartmentId == id);
        }
    }
}
