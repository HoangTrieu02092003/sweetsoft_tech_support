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
        [HttpGet]
        public async Task<IActionResult> Index(string status, string search, string sortColumn, string sortOrder, int page = 1)
        {
            var currentUserIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserIdString) || !int.TryParse(currentUserIdString, out int currentUserId))
            {
                TempData["ReturnUrl"] = Request.Path.ToString();
                return RedirectToAction("Login", "Admin");
            }

            var users = _context.TblUsers
                .Where(u => u.IsDelete == false)
                .Include(u => u.Role)
                .Include(u => u.Department)
                .AsQueryable();
            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortOrder))
                users = TableSorter.Sort(users, sortColumn, sortOrder).AsQueryable();
            if (!string.IsNullOrEmpty(status))
            {
                if (status == "1")
                {
                    users = users.Where(u => u.Status == 1);
                }
                else if (status == "0")
                {
                    users = users.Where(u => u.Status == 0);
                }
            }
            //tìm kiếm 
            if (!string.IsNullOrEmpty(search))
            {
                // Lọc logs theo tiêu chí tìm kiếm
                var lowerSearch = search.ToLower();
                users = users.Where(u =>
                    u.FullName != null && u.FullName.ToLower().Contains(lowerSearch) ||
                    u.Email != null && u.Email.ToLower().Contains(lowerSearch) ||
                    u.Phone != null && u.Phone.ToLower().Contains(lowerSearch) ||
                    u.Department != null && u.Department != null && u.Department.DepartmentName.ToLower().Contains(lowerSearch)||
                    u.Role != null && u.Role.RoleName.ToLower().Contains(lowerSearch)
                    );
            }
            var pageSize = 6; // số lượng người dùng mỗi trang
            var skip = (page - 1) * pageSize;
            
            var requestContext = await users
                .Where(u => u.UserId != currentUserId && u.IsDelete == false)
                .Include(t => t.CreatedUserNavigation)
                .Include(t => t.Department)
                .Include(t => t.Role)
                .Include(t => t.UpdatedUserNavigation)
                .Skip(skip) // bỏ qua dữ liệu đã xem ở các trang trước
                .Take(pageSize).ToListAsync();

            var totalUsers = await users.CountAsync();

            // Tính tổng số trang
            var totalPages = (int)Math.Ceiling(totalUsers / (double)pageSize);

            // Chuyển dữ liệu sang View
            ViewData["SortColumn"] = sortColumn;
            ViewData["SortOrder"] = sortOrder;
            ViewData["TotalPages"] = totalPages;
            ViewData["CurrentPage"] = page;
            ViewData["Status"] = status;
            ViewData["Search"] = search;
            return View( requestContext);
        }

        [PermissionAuthorize("Quản lý nhân viên")]
        // GET: TblUsers/Create
        public IActionResult Create()
        {
            ViewData["CreatedUser"] = new SelectList(_context.TblUsers, "UserId", "UserId");
            ViewBag.DepartmentId = new SelectList(
            _context.TblDepartments.Where(d => d.IsDelete == false),
            "DepartmentId",
            "DepartmentName");
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
                var existingEmail = await _context.TblUsers
                    .FirstOrDefaultAsync(c => c.Email == tblUser.Email);

                var existingUsername = await _context.TblUsers
                    .FirstOrDefaultAsync(c => c.Username == tblUser.Username);

                if (existingEmail != null && existingEmail.IsDelete == true)
                {
                    _context.TblUsers.Remove(existingEmail); // Xóa bản ghi cũ để tránh trùng lặp
                    await _context.SaveChangesAsync();
                }

                if (existingUsername != null && existingUsername.IsDelete == true)
                {
                    _context.TblUsers.Remove(existingUsername); // Xóa bản ghi cũ để tránh trùng lặp
                    await _context.SaveChangesAsync();
                }
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


                _logService.LogAuditAction("Thêm",User.Identity.Name, $"Thêm thành công nhân viên {tblUser.FullName}","Nhân viên", " ", Newtonsoft.Json.JsonConvert.SerializeObject(tblUser.ToLogData()));
                TempData["Success"] = "Thêm nhân viên thành công";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                var existingEmail = await _context.TblUsers
                    .FirstOrDefaultAsync(c => c.Email == tblUser.Email);

                var existingUsername = await _context.TblUsers
                    .FirstOrDefaultAsync(c => c.Username == tblUser.Username);
                if(existingEmail != null && existingEmail.IsDelete == false)
                {
                    TempData["Error"] = "Email đã tồn tại trong hệ thống. Vui lòng sử dụng email khác!";
                    return View(tblUser);
                }

                if (existingUsername != null && existingUsername.IsDelete == false)
                {
                    TempData["Error"] = "Username đã tồn tại trong hệ thống. Vui lòng sử dụng tên khác!";
                    return View(tblUser);
                }
            }
            ViewData["CreatedUser"] = new SelectList(_context.TblUsers, "UserId", "UserId", tblUser.CreatedUser);
            ViewData["DepartmentId"] = new SelectList(
                _context.TblDepartments.Where(d => d.IsDelete == false),
                "DepartmentId",
                "DepartmentName",tblUser.DepartmentId );
            ViewData["RoleId"] = new SelectList(_context.TblRoles, "RoleId", "RoleName", tblUser.RoleId);
            ViewData["UpdatedUser"] = new SelectList(_context.TblUsers, "UserId", "UserId", tblUser.UpdatedUser);
            TempData["Error"] = "Thêm nhân viên thất bại";
            return View(tblUser);
        }

        [PermissionAuthorize("Quản lý nhân viên")]
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
            ViewData["DepartmentId"] = new SelectList(
                _context.TblDepartments.Where(d => d.IsDelete == false),
                "DepartmentId",
                "DepartmentName", 
                tblUser.DepartmentId
            );
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
                // Kiểm tra email có trùng không (ngoại trừ chính đối tượng đang được sửa)
                var duplicateEmailUser = await _context.TblUsers
                    .FirstOrDefaultAsync(u => u.Email == tblUser.Email && u.UserId != id);
                var duplicateUsername = await _context.TblUsers
                    .FirstOrDefaultAsync(u => u.Username == tblUser.Username && u.UserId != id);

                if (duplicateEmailUser != null)
                {
                    if (duplicateEmailUser.IsDelete == false)
                    {
                        // Nếu email đã tồn tại và không bị xóa, không cho phép đổi
                        TempData["Error"] = "Email đã tồn tại trong hệ thống. Vui lòng sử dụng email khác!";
                        return View(tblUser);
                    }
                    else
                    {
                        await _context.SaveChangesAsync();
                    }
                }
                if (duplicateUsername != null)
                {
                    if (duplicateUsername.IsDelete == false)
                    {
                        // Nếu email đã tồn tại và không bị xóa, không cho phép đổi
                        TempData["Error"] = "Username đã tồn tại trong hệ thống. Vui lòng sử dụng tên khác!";
                        return View(tblUser);
                    }
                    else
                    {
                        await _context.SaveChangesAsync();
                    }
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
                TempData["Success"] = "Sửa nhân viên thành công";
                return RedirectToAction(nameof(Index));
            }

            ViewData["CreatedUser"] = new SelectList(_context.TblUsers, "UserId", "UserId", tblUser.CreatedUser);
            ViewData["DepartmentId"] = new SelectList(_context.TblDepartments, "DepartmentId", "DepartmentId", tblUser.DepartmentId);
            ViewData["RoleId"] = new SelectList(_context.TblRoles, "RoleId", "RoleId", tblUser.RoleId);
            ViewData["UpdatedUser"] = new SelectList(_context.TblUsers, "UserId", "UserId", tblUser.UpdatedUser);
            ViewData["Error"] = "Sửa nhân viên thất bại";
            return View(tblUser);
        }

        [PermissionAuthorize("Quản lý quyền truy cập")]
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

            var requestPermissions = allPermissions
        .Where(p => p.PermissionName.ToLower().Contains("yêu cầu", StringComparison.OrdinalIgnoreCase))
        .ToList();

            var managementPermissions = allPermissions
                .Where(p => p.PermissionName.ToLower().Contains("quản lý", StringComparison.OrdinalIgnoreCase))
                .ToList();

            var otherPermissions = allPermissions
                .Where(p => !requestPermissions.Contains(p) && !managementPermissions.Contains(p))
                .ToList();
            ViewBag.username = user.FullName;
            ViewBag.RequestPermissions = requestPermissions;
            ViewBag.ManagementPermissions = managementPermissions;
            ViewBag.OtherPermissions = otherPermissions;
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
                    // Tìm tên quyền dựa trên permissionId
                    var permissionName = _context.TblPermissions
                        .Where(p => p.PermissionId == permissionId)
                        .Select(p => p.PermissionName.ToLower())
                        .FirstOrDefault();

                    if (!string.IsNullOrEmpty(permissionName))
                    {
                        var userPermission = new TblUserPermission
                        {
                            UserId = id,
                            PermissionId = permissionId
                        };
                        _context.TblUserPermissions.Add(userPermission);

                        // Ghi log với tên quyền
                        _logService.LogNotificationAction(id.ToString(), $"Bạn đã được cấp quyền {permissionName}");
                    }
                }
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Quyền của người dùng đã được cập nhật thành công.";
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
            TempData["Success"] = "Cập nhật thành công";
            return View(user); // Trả về view với thông tin người dùng
        }

        [PermissionAuthorize("Quản lý nhân viên")]
        // POST: TblUsers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tblUser = await _context.TblUsers.FindAsync(id);
            if (tblUser != null)
            {
                tblUser.IsDelete = true; // Đánh dấu là đã xóa

                _context.Update(tblUser);
                await _context.SaveChangesAsync();
            }
            TempData["Success"] = "Xóa nhân viên thành công";
            return RedirectToAction(nameof(Index));
        }

        private bool TblUserExists(int id)
        {
            return _context.TblUsers.Any(e => e.UserId == id);
        }
    }
}