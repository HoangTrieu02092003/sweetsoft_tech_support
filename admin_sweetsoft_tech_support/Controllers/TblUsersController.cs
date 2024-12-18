using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using admin_sweetsoft_tech_support.Models;
using System.Security.Claims;
using admin_sweetsoft_tech_support.Attributes;

namespace admin_sweetsoft_tech_support.Controllers
{
    
    public class TblUsersController : Controller
    {
        private readonly RequestContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TblUsersController(RequestContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        // GET: TblUsers
        public async Task<IActionResult> Index(int page = 1)
        {
            var pageSize = 6; // số lượng người dùng mỗi trang
            var skip = (page - 1) * pageSize;
            var currentUserIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserIdString) || !int.TryParse(currentUserIdString, out int currentUserId))
            {
                return RedirectToAction("Login", "Admin");
            }
            var requestContext = _context.TblUsers
                .Where(u => u.UserId != currentUserId)
                .Include(t => t.CreatedUserNavigation)
                .Include(t => t.Department)
                .Include(t => t.Role)
                .Include(t => t.UpdatedUserNavigation)
                .Skip(skip) // bỏ qua dữ liệu đã xem ở các trang trước
                .Take(pageSize);

            var totalUsers = await _context.TblUsers.CountAsync();

            // Tính tổng số trang
            var totalPages = (int)Math.Ceiling(totalUsers / (double)pageSize);

            // Chuyển dữ liệu sang View
            ViewData["TotalPages"] = totalPages;
            ViewData["CurrentPage"] = page;
            return View(await requestContext.ToListAsync());
        }

        [PermissionAuthorize("Thêm người dùng")]
        // GET: TblUsers/Create
        public IActionResult Create()
        {
            ViewData["CreatedUser"] = new SelectList(_context.TblUsers, "UserId", "UserId");
            ViewData["DepartmentId"] = new SelectList(_context.TblDepartments, "DepartmentId", "DepartmentName");
            ViewData["RoleId"] = new SelectList(_context.TblRoles, "RoleId", "RoleName");
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
                tblUser.Password = BCrypt.Net.BCrypt.HashPassword("Password123"); // Mã hóa mật khẩu mặc định
                tblUser.Status = 1;
                tblUser.IsAdmin = false; // Mặc định là false
                tblUser.ResetToken = null; // Mặc định là null
                tblUser.ResetTokenExpiry = null; // Mặc định là null
                tblUser.CreatedUser = 1; // Mặc định là userId 1
                tblUser.CreatedAt = DateTime.Now; // Mặc định là ngày hiện tại
                tblUser.UpdatedUser = 1; // Mặc định là userId 1
                tblUser.UpdatedAt = DateTime.Now; // Mặc định là ngày hiện tại
                _context.Add(tblUser);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CreatedUser"] = new SelectList(_context.TblUsers, "UserId", "UserId", tblUser.CreatedUser);
            ViewData["DepartmentId"] = new SelectList(_context.TblDepartments, "DepartmentId", "DepartmentName", tblUser.DepartmentId);
            ViewData["RoleId"] = new SelectList(_context.TblRoles, "RoleId", "RoleName", tblUser.RoleId);
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
            var statusList = new List<SelectListItem> { 
                new SelectListItem { Value = "1", Text = "Hoạt động" }, 
                new SelectListItem { Value = "0", Text = "Ngừng hoạt động" } 
            }; 
            ViewBag.StatusList = new SelectList(statusList, "Value", "Text", tblUser.Status);

            ViewBag.createdUser = tblUser.CreatedUserNavigation?.FullName ?? "N/A";
            ViewBag.updatedUser = tblUser.UpdatedUserNavigation?.FullName ?? "N/A";
            ViewData["DepartmentId"] = new SelectList(_context.TblDepartments, "DepartmentId", "DepartmentName", tblUser.DepartmentId);
            ViewData["RoleId"] = new SelectList(_context.TblRoles, "RoleId", "RoleName", tblUser.RoleId);
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
                var existingUser = await _context.TblUsers.FindAsync(id);
                if (existingUser == null)
                {
                    return NotFound();
                }

                // Cập nhật chỉ những thuộc tính được chỉnh sửa, các thuộc tính không thay đổi sẽ giữ nguyên
                if (!string.IsNullOrEmpty(tblUser.FullName)) existingUser.FullName = tblUser.FullName;
                if (!string.IsNullOrEmpty(tblUser.Email)) existingUser.Email = tblUser.Email;
                if (!string.IsNullOrEmpty(tblUser.Phone)) existingUser.Phone = tblUser.Phone;
                if (!string.IsNullOrEmpty(tblUser.Username)) existingUser.Username = tblUser.Username;
                if (tblUser.RoleId != null) existingUser.RoleId = tblUser.RoleId;
                if (tblUser.DepartmentId != null) existingUser.DepartmentId = tblUser.DepartmentId;
                existingUser.Status = tblUser.Status;
                existingUser.UpdatedAt = DateTime.Today;
                try
                {
                    _context.Update(existingUser);
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

        // GET: Users/AssignPermission/5
        public async Task<IActionResult> AssignPermissions(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.TblUsers
                .Include(u => u.TblUserPermissions)
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null)
            {
                return NotFound();
            }

            var allPermissions = await _context.TblPermissions.ToListAsync();
            var existingPermissions = await _context.TblUserPermissions
                .Where(up => up.UserId == id)
                .ToListAsync();
            var assignedPermissionIds = existingPermissions.Select(up => up.PermissionId).ToList();

            var requestPermissions = allPermissions.Where(p => p.PermissionId == 1 || p.PermissionId == 2).ToList();
            var managementPermissions = allPermissions.Where(p => p.PermissionId != 1 && p.PermissionId != 2).ToList();

            ViewBag.username = user.FullName;
            ViewBag.RequestPermissions = requestPermissions;
            ViewBag.ManagementPermissions = managementPermissions;
            ViewBag.AssignedPermissions = assignedPermissionIds;

            return View();
        }

        // POST: Users/AssignPermissions/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignPermissions(int id, List<int> selectedPermissions)
        {
            // Lấy danh sách quyền cũ
            var existingPermissions = await _context.TblUserPermissions
                .Where(up => up.UserId == id)
                .ToListAsync();

            // Xóa quyền cũ
            _context.TblUserPermissions.RemoveRange(existingPermissions);

            // Thêm quyền mới nếu có
            if (selectedPermissions != null && selectedPermissions.Any())
            {
                foreach (var permissionId in selectedPermissions)
                {
                    var userPermission = new TblUserPermission
                    {
                        UserId = id,
                        PermissionId = permissionId
                    };
                    _context.TblUserPermissions.Add(userPermission);
                }
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Quyền của người dùng đã được cập nhật thành công.";
            return RedirectToAction(nameof(Index)); // Điều hướng về danh sách người dùng
        }

        public async Task<IActionResult> MyAccount()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value; // Lấy ID người dùng từ Claims
            if (userId == null)
            {
                return RedirectToAction("Login", "Account"); 
            }

            var user = await _context.TblUsers
                .Include(t => t.Department)
                .Include(t => t.Role)
                .FirstOrDefaultAsync(u => u.UserId.ToString() == userId);

            if (user == null)
            {
                return NotFound();
            }

            return View(user); // Trả về view với thông tin người dùng
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
            var currentUserId = TempData["UserId"] as int?;
            var isAdmin = TempData["IsAdmin"] as string == "true";
            if (currentUserId == null || !isAdmin)
            {
                // Nếu không phải admin, chuyển hướng về danh sách với thông báo lỗi
                TempData["ErrorMessage"] = "Bạn không có quyền xóa người dùng.";
                return RedirectToAction(nameof(Index));
            }

            var tblUser = await _context.TblUsers.FindAsync(id);
            if (tblUser != null)
            {
                _context.TblUsers.Remove(tblUser);
                await _context.SaveChangesAsync();
            }
            else
            {
                TempData["ErrorMessage"] = "Người dùng không tồn tại.";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool TblUserExists(int id)
        {
            return _context.TblUsers.Any(e => e.UserId == id);
        }
    }
}