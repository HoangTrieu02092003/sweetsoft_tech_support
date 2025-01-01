using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using admin_sweetsoft_tech_support.Models;
using System.Security.Claims;
using admin_sweetsoft_tech_support.Attributes;
using System.Data;
namespace admin_sweetsoft_tech_support.Controllers
{
    
    public class TblUsersController : Controller
    {
        private readonly RequestContext _context;
        private readonly LogService _logService;

        public TblUsersController(RequestContext context, LogService logService)
        {
            _context = context;
            _logService = logService;
        }
        // GET: TblUsers
        public async Task<IActionResult> Index(string status, string search, int page = 1)
        {

            var users = _context.TblUsers
                .Include(u => u.Role)
                .Include(u => u.Department)
                .AsQueryable();

            if (status == "1")
            {
                users = users.Where(u => u.Status == 1);
            }
            else if (status == "0")
            {
                users = users.Where(u => u.Status == 0);
            }
            //tìm kiếm 
            if (!string.IsNullOrEmpty(search))
            {
                // Lọc logs theo tiêu chí tìm kiếm
                var lowerSearch = search.ToLower();
                users = users.Where(u =>
                    u.FullName.ToLower().Contains(lowerSearch) ||
                    u.Email.ToLower().Contains(lowerSearch) ||
                    u.Phone.ToLower().Contains(lowerSearch) ||
                    (u.Department != null && u.Department.DepartmentName.ToLower().Contains(lowerSearch))||
                    (u.Role != null && u.Role.RoleName.ToLower().Contains(lowerSearch))
                    );
            }
            var pageSize = 6; // số lượng người dùng mỗi trang
            var skip = (page - 1) * pageSize;
            var currentUserIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserIdString) || !int.TryParse(currentUserIdString, out int currentUserId))
            {
                TempData["ReturnUrl"] = Request.Path.ToString();
                return RedirectToAction("Login", "Admin");
            }
            var requestContext = users
                .Where(u => u.UserId != currentUserId)
                .Include(t => t.CreatedUserNavigation)
                .Include(t => t.Department)
                .Include(t => t.Role)
                .Include(t => t.UpdatedUserNavigation)
                .Skip(skip) // bỏ qua dữ liệu đã xem ở các trang trước
                .Take(pageSize);

            var totalUsers = await users.CountAsync();

            // Tính tổng số trang
            var totalPages = (int)Math.Ceiling(totalUsers / (double)pageSize);

            // Chuyển dữ liệu sang View
            ViewData["TotalPages"] = totalPages;
            ViewData["CurrentPage"] = page;
            ViewData["status"] = status ?? "";  // Giữ giá trị của status nếu có, nếu không thì để trống
            ViewData["search"] = search ?? "";
            return View(await requestContext.ToListAsync());
        }

        [PermissionAuthorize("Quản lý nhân viên")]
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
                var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                tblUser.Password = BCrypt.Net.BCrypt.HashPassword("Password123"); // Mã hóa mật khẩu mặc định
                tblUser.Status = 1;
                tblUser.IsAdmin = false; // Mặc định là false
                tblUser.ResetToken = null; // Mặc định là null
                tblUser.ResetTokenExpiry = null; // Mặc định là null
                tblUser.CreatedUser = currentUserId;
                tblUser.CreatedAt = DateTime.Now; // Mặc định là ngày hiện tại
                tblUser.UpdatedUser = currentUserId;
                tblUser.UpdatedAt = DateTime.Now; // Mặc định là ngày hiện tại
                _context.Add(tblUser);
                await _context.SaveChangesAsync();
                _logService.LogAuditAction("Thêm",User.Identity.Name, $"Thêm thành công nhân viên {tblUser.FullName}","Nhân viên", " ", Newtonsoft.Json.JsonConvert.SerializeObject(tblUser));
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

                // Lưu giá trị cũ và thay đổi
                var oldValue = new Dictionary<string, object>();
                var changes = new Dictionary<string, object>();

                // Lấy danh sách các thuộc tính cần quan tâm (lọc bỏ các navigation properties không cần thiết)
                var properties = typeof(TblUser).GetProperties()
                    .Where(p => !p.PropertyType.Name.Contains("ICollection")) // Loại bỏ navigation collections
                    .ToList();

                foreach (var property in properties)
                {
                    var oldPropValue = property.GetValue(existingUser);
                    var newPropValue = property.GetValue(tblUser);

                    // Nếu giá trị thay đổi, lưu vào log
                    if (newPropValue != null && !Equals(oldPropValue, newPropValue))
                    {
                        oldValue[property.Name] = oldPropValue;
                        changes[property.Name] = newPropValue;

                        // Cập nhật giá trị mới vào existingUser
                        property.SetValue(existingUser, newPropValue);
                    }
                }

                existingUser.UpdatedAt = DateTime.Today;

                try
                {
                    _context.Update(existingUser);
                    await _context.SaveChangesAsync();

                    // Ghi log chỉ khi có thay đổi
                    if (changes.Count > 0)
                    {
                        _logService.LogAuditAction(
                            "Sửa",
                            User.Identity.Name,
                            $"Sửa nhân viên {tblUser.FullName} thành công",
                            "Nhân viên",
                            Newtonsoft.Json.JsonConvert.SerializeObject(oldValue),
                            Newtonsoft.Json.JsonConvert.SerializeObject(changes)
                        );
                    }
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
                return RedirectToAction("Login", "Admin"); 
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