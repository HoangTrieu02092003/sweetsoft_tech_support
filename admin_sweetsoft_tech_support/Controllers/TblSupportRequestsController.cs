using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using admin_sweetsoft_tech_support.Models;
using admin_sweetsoft_tech_support.Attributes;
using System.Security.Claims;
using Azure.Core;

namespace admin_sweetsoft_tech_support.Controllers
{
    public class TblSupportRequestsController : Controller
    {
        private readonly RequestContext _context;
        private readonly LogService _logService;
        private readonly EmailHelper _emailHelper;

        public TblSupportRequestsController(RequestContext context, LogService logService, EmailHelper emailHelper)
        {
            _context = context;
            _logService = logService;
            _emailHelper = emailHelper;
        }

        [HttpGet]
        public IActionResult Index(int? status, string search, string sortColumn, string sortOrder, int page = 1)
        {
            var currentUserIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserIdString) || !int.TryParse(currentUserIdString, out int currentUserId))
            {
                TempData["ReturnUrl"] = Request.Path.ToString();
                return RedirectToAction("Login", "Admin");
            }
            var currentUser = _context.TblUsers
                .Include(u => u.Role)
                .FirstOrDefault(u => u.UserId == int.Parse(currentUserIdString));

            int pageSize = 6;
            var query = _context.TblSupportRequests
                .Include(r => r.Customer)
                .Include(r => r.Department)
                .Include(r => r.TblRequestFeedbacks)
                .Where(r => r.IsDelete == false)
                .Select(r => new
                {
                    SupportRequest = r,
                    r.RequestId,
                    r.Status,
                    r.RequestTitle,
                    r.CreatedAt,
                    r.ResolvedAt,
                    CustomerFullName = r.Customer.FullName,
                    r.Department.DepartmentName,
                    HasUnreadFeedback = r.TblRequestFeedbacks.Any(f => f.RequestId == r.RequestId && f.IsReadByUser == false)
                })
                .AsQueryable();

            if (currentUser != null)
            {
                if (currentUser.IsAdmin == true)
                {
                    query = query;
                }
                else if (currentUser.Role != null && currentUser.RoleId == 2 || currentUser.RoleId == 3)
                {
                    query = query.Where(r => r.SupportRequest.DepartmentId == currentUser.DepartmentId);
                }
                else
                {
                    query = query.Where(r => r.SupportRequest.HandleBy == currentUser.UserId);
                }
            }

            // Lọc dữ liệu
            if (status.HasValue)
                query = query.Where(r => r.SupportRequest.Status == status.Value);

            // Lọc theo tìm kiếm
            if (!string.IsNullOrEmpty(search))
            {
                var lower = search.ToLower();
                query = query.Where(r =>
                r.SupportRequest.RequestTitle.ToLower().Contains(search) ||
                r.SupportRequest.Department.DepartmentName.ToLower().Contains(search)
                );
            }

            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortOrder))
                query = TableSorter.Sort(query, sortColumn, sortOrder);
            else
            { 
                //xếp theo thời gian tạo yêu cầu, nếu có feedback mới thì đẩy feedback đó lên
              query = query.OrderByDescending(r => r.HasUnreadFeedback) 
                    .ThenByDescending(r => r.SupportRequest.CreatedAt); 
            }

                int totalRequests = query.Count();
            var paginatedRequests = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            //
            // Lấy danh sách nhân viên từ cơ sở dữ liệu
            var employees = _context.TblUsers // Giả sử Users là bảng chứa nhân viên của bạn
                .Where(u => u.Status == 1 && u.IsDelete == false) // Lọc nhân viên đang hoạt động (nếu cần)
                .Select(u => new { u.UserId, u.FullName })
                .ToList();

            // Truyền danh sách nhân viên vào ViewBag
            ViewBag.Employees = new SelectList(employees, "UserId", "FullName");
            //
            // Truyền dữ liệu sang View
            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = (int)Math.Ceiling(totalRequests / (double)pageSize);
            // dữ liệu quyền
            ViewData["IsAdmin"] = currentUser?.IsAdmin ?? false;
            ViewData["IsDepartmentManager"] = currentUser?.RoleId == 2 || currentUser?.RoleId == 3;
            ViewData["IsRegularUser"] = !(currentUser?.IsAdmin ?? false) && currentUser?.RoleId != 2 && currentUser?.RoleId != 3;
            // dữ liệu tìm kiếm
            ViewData["Status"] = status;
            ViewData["Search"] = search;
            ViewData["SortColumn"] = sortColumn;
            ViewData["SortOrder"] = sortOrder;
            ViewData["Search"] = search;

            return View(paginatedRequests);
        }


        [PermissionAuthorize("Sửa yêu cầu hỗ trợ")]
        // GET: TblSupportRequests/Details/5
        public IActionResult Details(int id)
        {
            var currentUserIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var supportRequest = _context.TblSupportRequests
                .Include(r => r.Customer)
                .Include(r => r.Department)
                .FirstOrDefault(r => r.RequestId == id);

            if (supportRequest == null)
            {
                return NotFound();
            }

            ViewBag.CustomerName = supportRequest.Customer?.FullName;
            ViewBag.DepartmentName = supportRequest.Department?.DepartmentName;
            ViewBag.RequestTransfers = _context.TblRequestTransfers
                .Where(t => t.RequestId == id)
                .Include(t => t.FromDepartment)
                .Include(t => t.ToDepartment)
                .Include(t => t.TransferredByNavigation)
                .Include(t => t.TransferredHandleNavigation)
                .ToList();
            var departmentManager = _context.TblUsers
                                    .FirstOrDefault(u => u.DepartmentId == supportRequest.DepartmentId && u.RoleId == 2);
            var currentUser = _context.TblUsers
                            .Include(u => u.Role)
                            .FirstOrDefault(u => u.UserId == int.Parse(currentUserIdString));
            if (departmentManager != null)
            {
                if (supportRequest.HandleByNavigation != null)
                {
                    ViewBag.HandleBy = supportRequest.HandleByNavigation.FullName;
                }
                else
                {
                    supportRequest.HandleBy = departmentManager.UserId;
                    ViewBag.HandleBy = departmentManager.FullName; // Lấy tên đầy đủ từ bảng TblUsers
                }
            }
            else
            {
                supportRequest.HandleBy = _context.TblUsers.FirstOrDefault(u => u.RoleId == 1)?.UserId; // Gán quản trị viên
                ViewBag.HandleBy = _context.TblUsers.FirstOrDefault(u => u.RoleId == 1)?.FullName; // Lấy tên đầy đủ của quản trị viên
            }
            ViewData["IsAdmin"] = currentUser?.IsAdmin ?? false;
            ViewData["IsDepartmentManager"] = currentUser?.RoleId == 2 || currentUser?.RoleId == 3;
            return View(supportRequest);
        }

        [PermissionAuthorize("Tạo yêu cầu hỗ trợ")]
        // GET: TblSupportRequests/Create
        public IActionResult Create()
        {
            ViewData["CustomerId"] = new SelectList(_context.TblCustomers, "CustomerId", "CustomerId");
            ViewData["DepartmentId"] = new SelectList(_context.TblDepartments, "DepartmentId", "DepartmentId");
            ViewBag.CustomerId = new SelectList(_context.TblCustomers, "CustomerId", "FullName");
            ViewBag.DepartmentId = new SelectList(
            _context.TblDepartments.Where(d => d.IsDelete == false),
            "DepartmentId",
            "DepartmentName");
            return View();
        }

        // POST: TblSupportRequests/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        // POST: TblSupportRequests/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RequestId,CustomerId,DepartmentId,RequestTitle,Product,RequestDetails,Status,CreatedAt,ResolvedAt")] TblSupportRequest tblSupportRequest, int currentPage = 1)
        {
            if (!_context.TblCustomers.Any(c => c.CustomerId == tblSupportRequest.CustomerId))
            {
                ModelState.AddModelError("CustomerId", "Invalid CustomerId.");
            }

            if (string.IsNullOrEmpty(tblSupportRequest.RequestTitle))
            {
                ModelState.AddModelError("RequestTitle", "RequestTitle is required.");
            }

            if (ModelState.IsValid)
            {
                var departmentManager = _context.TblUsers
                    .FirstOrDefault(u => u.DepartmentId == tblSupportRequest.DepartmentId && u.RoleId == 2);

                if (departmentManager != null)
                {
                    tblSupportRequest.HandleBy = departmentManager.UserId;
                }

                _context.Add(tblSupportRequest);
                await _context.SaveChangesAsync();

                int newRequestId = tblSupportRequest.RequestId;

                var requestProcessing = new TblRequestsProcessing
                {
                    RequestId = newRequestId,
                    DepartmentId = tblSupportRequest.DepartmentId,
                    IsCompleted = 0,
                    ProcessedAt = null,
                    Note = "Chưa xử lý...."
                };
                _context.TblRequestsProcessings.Add(requestProcessing);
                await _context.SaveChangesAsync();

                _logService.LogActivityAction("Thêm yêu cầu hỗ trợ thành công", "Thêm", User.Identity.Name);
                if (departmentManager != null)
                {
                    _logService.LogNotificationAction(departmentManager.UserId.ToString(),"Có yêu cầu mới", "Bạn có yêu cầu mới từ khách hàng");
                }
                return RedirectToAction(nameof(Index), new { page = currentPage });
            }

            ViewData["CustomerId"] = new SelectList(_context.TblCustomers, "CustomerId", "CustomerId", tblSupportRequest.CustomerId);
            ViewData["DepartmentId"] = new SelectList(
                _context.TblDepartments.Where(d => d.IsDelete == false), "DepartmentId", "DepartmentName", tblSupportRequest.DepartmentId);
            return View(tblSupportRequest);
        }

        [PermissionAuthorize("Sửa yêu cầu hỗ trợ")]
        public IActionResult Edit(int id)
        {
            // Tìm yêu cầu hỗ trợ từ CSDL
            var supportRequest = _context.TblSupportRequests.Find(id);
            if (supportRequest == null)
            {
                return NotFound();
            }

            // Truyền danh sách khách hàng và phòng ban vào ViewBag
            ViewBag.CustomerId = new SelectList(_context.TblCustomers, "CustomerId", "FullName", supportRequest.CustomerId);
            ViewBag.DepartmentId = new SelectList(_context.TblDepartments, "DepartmentId", "DepartmentName", supportRequest.DepartmentId);
       
            return View(supportRequest);
        }

        // POST: TblSupportRequests/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TblSupportRequest updatedRequest, int currentPage = 1)
        {
            if (id != updatedRequest.RequestId)
            {
                return NotFound();
            }

            if (string.IsNullOrEmpty(updatedRequest.RequestTitle))
            {
                ModelState.AddModelError("RequestTitle", "RequestTitle is required.");
            }

            if (ModelState.IsValid)
            {
                var existingRequest = _context.TblSupportRequests.AsNoTracking().FirstOrDefault(r => r.RequestId == id);

                // Lưu giá trị cũ và thay đổi
                var oldValue = new Dictionary<string, object>();
                var changes = new Dictionary<string, object>();

                // Lấy danh sách các thuộc tính cần quan tâm (lọc bỏ các navigation properties không cần thiết)
                var properties = typeof(TblSupportRequest).GetProperties()
                    .Where(p => !p.PropertyType.Name.Contains("ICollection")) // Loại bỏ navigation collections
                    .ToList();

                foreach (var property in properties)
                {
                    var oldPropValue = property.GetValue(existingRequest);
                    var newPropValue = property.GetValue(updatedRequest);

                    // Nếu giá trị thay đổi, lưu vào log
                    if (newPropValue != null && !Equals(oldPropValue, newPropValue))
                    {
                        oldValue[property.Name] = oldPropValue;
                        changes[property.Name] = newPropValue;

                        // Cập nhật giá trị mới vào existingUser
                        property.SetValue(existingRequest, newPropValue);
                    }
                }
                try
                {
                    // Lấy dữ liệu gốc từ CSDL
                    if (existingRequest == null)
                    {
                        return NotFound();
                    }

                    // Chỉ cập nhật các trường được thay đổi từ view
                    existingRequest.RequestTitle = updatedRequest.RequestTitle;
                    existingRequest.Product = updatedRequest.Product;
                    existingRequest.RequestDetails = updatedRequest.RequestDetails;
                    existingRequest.Status = updatedRequest.Status;
                    existingRequest.CustomerId = updatedRequest.CustomerId;
                    existingRequest.DepartmentId = updatedRequest.DepartmentId;

                    // Cập nhật vào CSDL
                    _context.Update(existingRequest);
                    await _context.SaveChangesAsync();

                    // Ghi nhật ký hành động
                    _logService.LogActivityAction("Cập nhật yêu cầu hỗ trợ", "Cập nhật", User.Identity.Name);

                    // Ghi log chỉ khi có thay đổi
                    if (changes.Count > 0)
                    {
                        _logService.LogActivityAction(
                            "Sửa",
                            $"Sửa yêu cầu {updatedRequest.RequestTitle} thành công",
                            User.Identity.Name,
                            Newtonsoft.Json.JsonConvert.SerializeObject(oldValue),
                            Newtonsoft.Json.JsonConvert.SerializeObject(changes)
                        );
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TblSupportRequestExists(updatedRequest.RequestId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            // Nếu ModelState không hợp lệ, truyền lại dữ liệu cho view
            ViewBag.CustomerId = new SelectList(_context.TblCustomers, "CustomerId", "FullName", updatedRequest.CustomerId);
            ViewBag.DepartmentId = new SelectList(
               _context.TblDepartments.Where(d => d.IsDelete == false), "DepartmentId", "DepartmentName", updatedRequest.DepartmentId);

            return View(updatedRequest);
        }

        [PermissionAuthorize("Xóa yêu cầu hỗ trợ")]
        // POST: TblSupportRequests/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, int currentPage = 1)
        {
            var tblSupportRequest = await _context.TblSupportRequests
                .FindAsync(id);
            if (tblSupportRequest != null)
            {
                tblSupportRequest.IsDelete = true; // Đánh dấu là đã xóa

                _context.Update(tblSupportRequest);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa yêu cầu hỗ trợ thành công.";
                _logService.LogActivityAction("Xóa yêu cầu hỗ trợ", "Xoá", User.Identity.Name);
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy yêu cầu hỗ trợ.";
            }           
            return RedirectToAction(nameof(Index), new { page = currentPage });
        }

        private bool TblSupportRequestExists(int id)
        {
            return _context.TblSupportRequests.Any(e => e.RequestId == id);
        }
        
        private bool TblRequestTransferExists(int id)
        {
            return _context.TblRequestTransfers.Any(e => e.TransferId == id);
        }

        [PermissionAuthorize("Chuyển giao yêu cầu cho phòng ban")]
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
            var departments = await _context.TblDepartments
            .Where(d => d.DepartmentId != tblSupportRequest.DepartmentId) // Loại bỏ phòng ban hiện tại
            .ToListAsync();
            var departmentManager = await _context.TblUsers
            .FirstOrDefaultAsync(u => u.DepartmentId == tblSupportRequest.DepartmentId && u.Role.RoleId == 1);


            ViewData["RequestId"] = new SelectList(_context.TblSupportRequests, "RequestId", "RequestTitle", tblSupportRequest.RequestId);
            ViewData["FromDepartmentId"] = new SelectList(_context.TblDepartments, "DepartmentId", "DepartmentName", tblSupportRequest.DepartmentId);
            ViewData["ToDepartmentId"] = new SelectList(
                    _context.TblDepartments.Where(d => d.IsDelete == false), "DepartmentId", "DepartmentName", tblSupportRequest.DepartmentId);
            ViewBag.RequestTitle = tblSupportRequest.RequestTitle;
            ViewBag.FormDepartment = tblSupportRequest.Department.DepartmentName;
            ViewBag.TransferredBy = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            ViewBag.TransferredHandle = departmentManager?.UserId ?? 0; // Set to 0 if no manager found
            return View(requestTransfer);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Transfer(int id,
            [Bind("TransferId,RequestId,FromDepartmentId,Priority,TransferredBy,TransferredAt,Note,RequestTitle,Product,TransferredHandle")]
            TblRequestTransfer requestTransfer, List<int> ToDepartmentId, string userName, int currentPage = 1)
        {
            if (id != requestTransfer.RequestId)
            {
                return NotFound();
            }

            // Validate if the CustomerId exists in TblCustomers
            var supportRequest = await _context.TblSupportRequests
                .Include(r => r.Customer)
                .FirstOrDefaultAsync(r => r.RequestId == requestTransfer.RequestId);

            if (supportRequest == null || supportRequest.Customer == null)
            {
                ModelState.AddModelError("RequestId", "Invalid CustomerId.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    foreach (var toDepartmentId in ToDepartmentId)
                    {
                        // Tìm trưởng phòng của phòng ban dựa trên ID phòng ban và role = 2 (trưởng phòng)
                        var departmentHead = await _context.TblUsers
                            .Where(u => u.DepartmentId == toDepartmentId && u.RoleId == 2) // Role = 2 là trưởng phòng
                            .Select(u => u.UserId) // Lấy UserId (ID của trưởng phòng)
                            .FirstOrDefaultAsync();

                        if (departmentHead == 0)
                        {
                            // Nếu không tìm thấy trưởng phòng, có thể xử lý lỗi hoặc thông báo
                            ModelState.AddModelError("", "Không tìm thấy trưởng phòng cho phòng ban này.");
                            return View(requestTransfer); // Hoặc trả về thông báo lỗi phù hợp
                        }

                        var transfer = new TblRequestTransfer
                        {
                            RequestId = requestTransfer.RequestId,
                            FromDepartmentId = requestTransfer.FromDepartmentId,
                            ToDepartmentId = toDepartmentId,
                            Priority = requestTransfer.Priority,
                            TransferredBy = requestTransfer.TransferredBy,
                            TransferredAt = requestTransfer.TransferredAt,
                            Note = requestTransfer.Note,
                            TransferredHandle = departmentHead // Gán trưởng phòng vào TransferredHandle
                        };
                        
                        _context.Add(transfer);
                        // Cập nhật yêu cầu hỗ trợ trong TblSupportRequest
                        if (toDepartmentId == ToDepartmentId.First())
                        {
                            if (supportRequest != null)
                            {
                                // Cập nhật HandleBy trong TblSupportRequest với ID trưởng phòng
                                supportRequest.DepartmentId = toDepartmentId;
                                supportRequest.RequestTitle = supportRequest.RequestTitle;
                                supportRequest.Product = supportRequest.Product;
                                supportRequest.HandleBy = departmentHead; // Gán trưởng phòng vào HandleBy
                                _context.Update(supportRequest);
                            }
                        }
                        // Tạo mới yêu cầu hỗ trợ cho các phòng ban sau
                        else if (ToDepartmentId.Count > 1)
                        {
                            if (supportRequest != null)
                            {
                                var newSupportRequest = new TblSupportRequest
                                {
                                    CustomerId = supportRequest.CustomerId,
                                    DepartmentId = toDepartmentId,
                                    RequestDetails = supportRequest.RequestDetails,
                                    Product = supportRequest.Product,
                                    RequestTitle = supportRequest.RequestTitle,
                                    Status = 0, // Trạng thái mặc định
                                    CreatedAt = DateTime.Now,
                                    ResolvedAt = null,
                                    HandleBy = departmentHead // Gán trưởng phòng vào HandleBy
                                };
                               
                                _context.Add(newSupportRequest);
                            }
                        }
                        var text = "";
                        if (transfer.Priority == 1)
                        {
                            text = "mức độ thấp";
                        }
                        else if (transfer.Priority == 2)
                        {
                            text = "mức độ trung bình";
                        }
                        else if (transfer.Priority == 3)
                        {
                            text = "mức độ cao (gấp)";
                        }
                        var toEmail = _context.TblUsers
                            .Where(x => x.UserId == departmentHead)
                            .Select(x => x.Email)
                            .FirstOrDefault();
                        if (!string.IsNullOrEmpty(toEmail))
                        {
                            await _emailHelper.SendEmailAsync(toEmail, "Yêu cầu mới", $"yêu cầu mới cần được giải quyết {text}");
                            _logService.LogNotificationAction(departmentHead.ToString(), "Yêu cầu được chuyển giao", "Phòng ban mới nhận được một yêu cầu mới");
                        }
                        else
                        {
                            _logService.LogNotificationAction(departmentHead.ToString(), "Lỗi gửi email", "Không có địa chỉ email cho trưởng phòng.");
                        }
                        _logService.LogActivityAction("Chuyển yêu cầu hỗ trợ", "Chuyển giao", User.Identity.Name);
                    }

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
                return RedirectToAction(nameof(Index), new { page = currentPage });
            }

            ViewData["RequestId"] = new SelectList(_context.TblSupportRequests, "RequestId", "RequestTitle", requestTransfer.RequestId);
            ViewData["FromDepartmentId"] = new SelectList(_context.TblDepartments, "DepartmentId", "DepartmentName", requestTransfer.FromDepartmentId);
            ViewData["ToDepartmentId"] = new SelectList(
                    _context.TblDepartments.Where(d => d.IsDelete == false), "DepartmentId", "DepartmentName", requestTransfer.Request.DepartmentId);
            ViewData["TransferredBy"] = new SelectList(_context.TblUsers, "UserId", "FullName", requestTransfer.TransferredBy);
            ViewBag.TransferredBy = new SelectList(_context.TblUsers.Select(u => new { u.UserId, u.FullName }), "UserId", "FullName");
            ViewBag.RequestTitle = supportRequest?.RequestTitle;
            return View(requestTransfer);
        }

        //giải quyết yêu cầu
        [PermissionAuthorize("Giải quyết yêu cầu")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, int status, DateTime? resolvedAt, string note)
        {
            var supportRequest = await _context.TblSupportRequests.FindAsync(id);
            var processing = await _context.TblRequestsProcessings.FirstOrDefaultAsync(p => p.RequestId == id);
            var customer = await _context.TblCustomers.FindAsync(supportRequest.CustomerId);
            if (processing == null)
            {
                processing = new TblRequestsProcessing
                {
                    RequestId = id,
                    DepartmentId = supportRequest.DepartmentId, // Gán DepartmentId từ TblSupportRequest
                    IsCompleted = 1,
                    ProcessedAt = resolvedAt ?? DateTime.Now,
                    Note = note
                };
                _context.TblRequestsProcessings.Add(processing);
            }
            else
            {
                processing.IsCompleted = 1;
                processing.ProcessedAt = resolvedAt ?? DateTime.Now;
                processing.Note = note;
                processing.DepartmentId = supportRequest.DepartmentId; // Cập nhật lại DepartmentId nếu cần
                _context.TblRequestsProcessings.Update(processing);
            }
            await _context.SaveChangesAsync();

            var feedbacks = _context.TblRequestFeedbacks.Where(f => f.RequestId == id); 
            _context.TblRequestFeedbacks.RemoveRange(feedbacks);
            if (supportRequest == null)
            {
                return NotFound();
            }

            supportRequest.Status = (short)status;
            supportRequest.ResolvedAt = (status == 1 || status == 2) ? resolvedAt ?? DateTime.Now : null;

            if (status == 1 || status == 2)
            {
                processing.IsCompleted = (short?)status;
                processing.ProcessedAt = resolvedAt ?? DateTime.Now;
                processing.Note = note;
            }
            try
            {
                _context.Update(supportRequest);
                _context.TblRequestsProcessings.Update(processing);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Chuyển trạng thái thành công";
                _logService.LogActivityAction("Cập nhật trạng thái yêu cầu hỗ trợ", "Update", User.Identity.Name);
                // Gửi email thông báo
                string email = customer.Email; // Lấy địa chỉ email của khách hàng từ TblSupportRequest
                if (status == 1)
                {
                    await _emailHelper.SendEmailAsync(email, "Thông báo trạng thái yêu cầu hỗ trợ", $"Yêu cầu của bạn đã được giải quyết. Chi tiết: {note}");
                }
                if (status == 2)
                {
                    await _emailHelper.SendEmailAsync(email, "Thông báo trạng thái yêu cầu hỗ trợ", $"Yêu cầu của bạn không xử lý được. Chi tiết: {note}");
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TblSupportRequestExists(supportRequest.RequestId))
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
        
        [PermissionAuthorize("Chuyển giao yêu cầu cho nhân viên")]        
        // GET: SupportRequest/TransferToEmployee/{id}
        [HttpGet]
        public async Task<IActionResult> TransferToEmployee(int id)
        {
            // Lấy yêu cầu hỗ trợ dựa trên ID
            var supportRequest = await _context.TblSupportRequests
                .Include(r => r.Department)
                .FirstOrDefaultAsync(r => r.RequestId == id);

            if (supportRequest == null)
            {
                return NotFound(); // Nếu yêu cầu không tồn tại, trả về lỗi NotFound
            }

            // Lấy danh sách nhân viên trong phòng ban
            var employees = await _context.TblUsers
                .Where(u => u.DepartmentId == supportRequest.DepartmentId && u.RoleId == 4) // Role 4 là nhân viên
                .ToListAsync();

            ViewBag.SupportRequest = supportRequest;
            ViewBag.Employees = employees; // Pass the list of employees directly

            return View();
        }

        // POST: SupportRequest/TransferToEmployee/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TransferToEmployee(int id, int employeeId,
    [Bind("TransferId,RequestId,FromDepartmentId,Priority,TransferredBy,TransferredAt,Note,RequestTitle,Product,TransferredHandle")]
    TblRequestTransfer requestTransfer)
        {
            // Kiểm tra tính hợp lệ của requestTransfer
            if (id != requestTransfer.RequestId)
            {
                return BadRequest("Request ID mismatch.");
            }

            // Lấy yêu cầu hỗ trợ
            var supportRequest = await _context.TblSupportRequests
                .Include(r => r.Department)
                .FirstOrDefaultAsync(r => r.RequestId == id);

            if (supportRequest == null)
            {
                return NotFound();
            }

            // Cập nhật yêu cầu hỗ trợ với nhân viên mới
            supportRequest.HandleBy = employeeId;
            // Lưu thay đổi vào cơ sở dữ liệu
            _context.Update(supportRequest);
            await _context.SaveChangesAsync();
            // Tạo đối tượng chuyển giao mới
            var transfer = new TblRequestTransfer
            {
                RequestId = requestTransfer.RequestId,
                FromDepartmentId = requestTransfer.FromDepartmentId,
                ToDepartmentId = requestTransfer.FromDepartmentId, // Có thể cần cập nhật lại nếu có thông tin phòng ban đích
                Priority = requestTransfer.Priority,
                TransferredBy = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value), // Người chuyển giao là người đang thực hiện
                TransferredAt = DateTime.Now,
                Note = requestTransfer.Note,
                TransferredHandle = employeeId // Gán nhân viên mới vào TransferredHandle
            };

            // Thêm đối tượng chuyển giao vào cơ sở dữ liệu
            _context.Add(transfer);
            await _context.SaveChangesAsync();
            //gửi mail
            var emmployHandle = _context.TblUsers
                .FirstOrDefault(u => u.UserId == employeeId);
            _emailHelper.SendEmailAsync(emmployHandle.Email,"Yêu cầu mới",$"giải quyết yêu cầu {supportRequest.RequestTitle} cho khách hàng.");
            // Log hành động
            _logService.LogActivityAction("Chuyển yêu cầu hỗ trợ", "Chuyển giao cho nhân viên", User.Identity.Name);

            return RedirectToAction(nameof(Index));
        }

        //Feedback
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SendMessage(int requestId, string message, int toCustomerId, int toUserId)
        {
            
            if (string.IsNullOrWhiteSpace(message))
            {
                return RedirectToAction("Index", new { requestId });
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            TblRequestFeedback newFeedback = null;
                newFeedback = new TblRequestFeedback
                {
                    RequestId = requestId,
                    FromUserId = userId,
                    FromCustomerId = null,
                    ToUserId = null,
                    ToCustomerId = toCustomerId,
                    Feedback = message,
                    FeedbackType = 1,
                    CreatedAt = DateTime.Now,
                    IsReadByCustomer = false,
                    IsReadByUser = true
                };
            if (newFeedback != null)
            {
                _context.TblRequestFeedbacks.Add(newFeedback);
                _context.SaveChanges();
            }

            return RedirectToAction("Index", new { requestId });
        }
        //lấy các feedback
        public IActionResult GetFeedbacks(int requestId)
        {
            var feedbacks = _context.TblRequestFeedbacks
                .Where(f => f.RequestId == requestId)
                .OrderBy(f => f.CreatedAt)
                .ToList();

            return PartialView("_FeedbacksPartial", feedbacks);
        }
        //đánh dấu đã đọc
        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int requestId)
        {
            var feedbacks = await _context.TblRequestFeedbacks
                .Where(f => f.RequestId == requestId && f.IsReadByUser == false)
                .ToListAsync();

            if (feedbacks.Any())
            {
                feedbacks.ForEach(f => f.IsReadByUser = true);
                await _context.SaveChangesAsync();
            }

            return Ok(new { success = true });
        }
        //
        //xem đã đọc feedback chưa
        [HttpGet]
        public async Task<IActionResult> CheckForNewFeedback()
        {
            var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var hasUnreadFeedback = await _context.TblRequestFeedbacks
                .Where(f => f.IsReadByUser == false)
                .Select(f => f.RequestId).ToListAsync();
            return Json(hasUnreadFeedback);
        }
    }
}