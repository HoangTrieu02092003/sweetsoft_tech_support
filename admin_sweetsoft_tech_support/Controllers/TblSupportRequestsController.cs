using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using admin_sweetsoft_tech_support.Models;
using admin_sweetsoft_tech_support.Attributes;
using Azure.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;

namespace admin_sweetsoft_tech_support.Controllers
{
    public class TblSupportRequestsController : Controller
    {
        private readonly RequestContext _context;
        private readonly LogService _logService;

        public TblSupportRequestsController(RequestContext context, LogService logService)
        {
            _context = context;
            _logService = logService;
        }

        //// GET: TblSupportRequests
        //public async Task<IActionResult> Index()
        //{
        //    var requestContext = _context.TblSupportRequests.Include(t => t.Customer).Include(t => t.Department);
        //    return View(await requestContext.ToListAsync());
        //}
        [HttpGet]
        public IActionResult Index(int? status, string search, string sortColumn, string sortOrder, int page = 1)
        {
            int pageSize = 6;
            var query = _context.TblSupportRequests
                .Include(r => r.Customer)
                .Include(r => r.Department)
                .AsQueryable();

            // Lọc dữ liệu
            if (status.HasValue)
                query = query.Where(r => r.Status == status.Value);

            // Lọc theo tìm kiếm
            if (!string.IsNullOrEmpty(search))
            {
                string lowerSearch = search.ToLower();
                query = query.Where(r =>
                    r.RequestTitle.ToLower().Contains(lowerSearch) || // Tìm theo tiêu đề
                    r.Department.DepartmentName.ToLower().Contains(lowerSearch)); // Tìm theo phòng ban
            }
            // Sắp xếp dữ liệu
            switch (sortColumn)
            {
                case "RequestTitle":
                    query = sortOrder == "asc" ? query.OrderBy(r => r.RequestTitle) : query.OrderByDescending(r => r.RequestTitle);
                    break;
                case "Status":
                    query = sortOrder == "asc" ? query.OrderBy(r => r.Status) : query.OrderByDescending(r => r.Status);
                    break;
                case "CreatedAt":
                    query = sortOrder == "asc" ? query.OrderBy(r => r.CreatedAt) : query.OrderByDescending(r => r.CreatedAt);
                    break;
                case "ResolvedAt":
                    query = sortOrder == "asc" ? query.OrderBy(r => r.ResolvedAt) : query.OrderByDescending(r => r.ResolvedAt);
                    break;
                case "CustomerFullName":
                    query = sortOrder == "asc" ? query.OrderBy(r => r.Customer.FullName) : query.OrderByDescending(r => r.Customer.FullName);
                    break;
                case "DepartmentName":
                    query = sortOrder == "asc" ? query.OrderBy(r => r.Department.DepartmentName) : query.OrderByDescending(r => r.Department.DepartmentName);
                    break;
                default:
                    query = query.OrderByDescending(r => r.CreatedAt); // Mặc định sắp xếp theo ngày tạo giảm dần
                    break;
            }

            // Phân trang
            int totalRequests = query.Count();
            var paginatedRequests = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Truyền dữ liệu sang View
            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = (int)Math.Ceiling(totalRequests / (double)pageSize);
            ViewData["SortColumn"] = sortColumn;
            ViewData["SortOrder"] = sortOrder;
            ViewData["Search"] = search;

            return View(paginatedRequests);
        }




        // GET: TblSupportRequests/Details/5
        public IActionResult Details(int id)
        {
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
                .ToList();

            return View(supportRequest);
        }

        // GET: TblSupportRequests/Create
        public IActionResult Create()
        {
            ViewData["CustomerId"] = new SelectList(_context.TblCustomers, "CustomerId", "CustomerId");
            ViewData["DepartmentId"] = new SelectList(_context.TblDepartments, "DepartmentId", "DepartmentId");
            ViewBag.CustomerId = new SelectList(_context.TblCustomers, "CustomerId", "FullName");
            ViewBag.DepartmentId = new SelectList(_context.TblDepartments, "DepartmentId", "DepartmentName");
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
                _context.Add(tblSupportRequest);
                await _context.SaveChangesAsync();
                var departmentManager = _context.TblUsers
                    .FirstOrDefault(u => u.DepartmentId == tblSupportRequest.DepartmentId && u.Role.RoleName == "Trưởng phòng");
                _logService.LogActivityAction("Tạo yêu cầu hỗ trợ", "Create", User.Identity.Name);
                if (departmentManager != null)
                {
                    _logService.LogNotificationAction(departmentManager.FullName, "Bạn có yêu cầu mới");
                }
                return RedirectToAction(nameof(Index), new { page = currentPage });
            }
            ViewData["CustomerId"] = new SelectList(_context.TblCustomers, "CustomerId", "CustomerId", tblSupportRequest.CustomerId);
            ViewData["DepartmentId"] = new SelectList(_context.TblDepartments, "DepartmentId", "DepartmentId", tblSupportRequest.DepartmentId);
            var RequestTitle = _context.TblSupportRequests.Find(tblSupportRequest.RequestId).RequestTitle;
            return View(tblSupportRequest);
        }


        public IActionResult Edit(int id)
        {
            var supportRequest = _context.TblSupportRequests.Find(id);
            if (supportRequest == null)
            {
                return NotFound();
            }

            ViewBag.CustomerId = new SelectList(_context.TblCustomers, "CustomerId", "FullName", supportRequest.CustomerId);
            ViewBag.DepartmentId = new SelectList(_context.TblDepartments, "DepartmentId", "DepartmentName", supportRequest.DepartmentId);

            return View(supportRequest);
        }

        // POST: TblSupportRequests/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("RequestId,RequestTitle,Product,RequestDetails,Status,CustomerId,DepartmentId,CreatedAt,ResolvedAt")] TblSupportRequest supportRequest, int currentPage = 1)
        {
            if (id != supportRequest.RequestId)
            {
                return NotFound();
            }

            if (string.IsNullOrEmpty(supportRequest.RequestTitle))
            {
                ModelState.AddModelError("RequestTitle", "RequestTitle is required.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(supportRequest);
                    await _context.SaveChangesAsync();
                    _logService.LogActivityAction("Cập nhật yêu cầu hỗ trợ", "Cập nhật", User.Identity.Name);
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
                return RedirectToAction(nameof(Index), new { page = currentPage });
            }

            ViewBag.CustomerId = new SelectList(_context.TblCustomers, "CustomerId", "FullName", supportRequest.CustomerId);
            ViewBag.DepartmentId = new SelectList(_context.TblDepartments, "DepartmentId", "DepartmentName", supportRequest.DepartmentId);

            return View(supportRequest);
        }



        // GET: TblSupportRequests/Delete/5
        public async Task<IActionResult> Delete(int? id)
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

            return View(tblSupportRequest);
        }

        // POST: TblSupportRequests/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, int currentPage = 1)
        {
            var tblSupportRequest = await _context.TblSupportRequests.FindAsync(id);
            if (tblSupportRequest != null)
            {
                _context.TblSupportRequests.Remove(tblSupportRequest);
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

            ViewData["RequestId"] = new SelectList(_context.TblSupportRequests, "RequestId", "RequestTitle", tblSupportRequest.RequestId);
            ViewData["FromDepartmentId"] = new SelectList(_context.TblDepartments, "DepartmentId", "DepartmentName", tblSupportRequest.DepartmentId);
            ViewData["ToDepartmentId"] = new SelectList(_context.TblDepartments, "DepartmentId", "DepartmentName");
            ViewBag.TransferredBy = new SelectList(_context.TblUsers, "UserId", "FullName", User.Identity.Name);
            ViewBag.RequestTitle = tblSupportRequest.RequestTitle;
            ViewBag.FormDepartment = tblSupportRequest.Department.DepartmentName;
            return View(requestTransfer);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Transfer(int id, [Bind("TransferId,RequestId,FromDepartmentId,Priority,TransferredBy,TransferredAt,Note,RequestTitle,Product")] TblRequestTransfer requestTransfer, List<int> ToDepartmentId, string userName, int currentPage = 1)
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
                        var transfer = new TblRequestTransfer
                        {
                            RequestId = requestTransfer.RequestId,
                            FromDepartmentId = requestTransfer.FromDepartmentId,
                            ToDepartmentId = toDepartmentId,
                            Priority = requestTransfer.Priority,
                            TransferredBy = requestTransfer.TransferredBy,
                            TransferredAt = requestTransfer.TransferredAt,
                            Note = requestTransfer.Note
                        };

                        _context.Add(transfer);

                        // Update the existing support request for the first department
                        if (toDepartmentId == ToDepartmentId.First())
                        {
                            if (supportRequest != null)
                            {
                                supportRequest.DepartmentId = toDepartmentId;
                                supportRequest.RequestTitle = supportRequest.RequestTitle;
                                supportRequest.Product = supportRequest.Product;
                                _context.Update(supportRequest);
                            }
                        }
                        // Create a new support request for subsequent departments
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
                                    Status = 0, // Assuming 0 is the default status
                                    CreatedAt = DateTime.Now,
                                    ResolvedAt = null
                                };

                                _context.Add(newSupportRequest);
                            }
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
            ViewData["ToDepartmentId"] = new SelectList(_context.TblDepartments, "DepartmentId", "DepartmentName", requestTransfer.ToDepartmentId);
            ViewData["TransferredBy"] = new SelectList(_context.TblUsers, "UserId", "FullName", requestTransfer.TransferredBy);
            ViewBag.TransferredBy = new SelectList(_context.TblUsers, "UserId", "FullName", User.Identity.Name);
            ViewBag.RequestTitle = supportRequest.RequestTitle;
            return View(requestTransfer);
        }

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
            if (supportRequest == null)
            {
                return NotFound();
            }

            supportRequest.Status = (short)status;
            supportRequest.ResolvedAt = status == 1 ? resolvedAt ?? DateTime.Now : null;

            if (status == 1)
            {
                processing.IsCompleted = 1;
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
                string resetLink = ""; // Lấy hoặc tạo link đặt lại mật khẩu (hoặc thông tin chi tiết cần thiết khác)
                if (status == 1)
                {
                    await SendEmailAsync(email, "Thông báo trạng thái yêu cầu hỗ trợ", $"Yêu cầu của bạn đã được giải quyết. Chi tiết: {note}");
                }
                if (status == 2)
                {
                    await SendEmailAsync(email, "Thông báo trạng thái yêu cầu hỗ trợ", $"Yêu cầu của bạn không xử lý được. Chi tiết: {note}");
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
        // Hàm gửi email
        private async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            // Cấu hình SMTP client (ví dụ: Gmail SMTP)
            using var client = new System.Net.Mail.SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new System.Net.NetworkCredential("nhantrung890@gmail.com", "mika juyt thab rbit"),
                EnableSsl = true,
            };

            var mailMessage = new System.Net.Mail.MailMessage
            {
                From = new System.Net.Mail.MailAddress("nhantrung890@gmail.com", "Support Team"),
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
            };

            mailMessage.To.Add(toEmail);

            await client.SendMailAsync(mailMessage);
        }
    }
}