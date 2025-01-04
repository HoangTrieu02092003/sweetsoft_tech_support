using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using admin_sweetsoft_tech_support.Models;
using admin_sweetsoft_tech_support.Attributes;

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
        [Route("TblSupportRequests/Index")]
        public IActionResult Index(int? status, int page = 1, string sortOrder = "")
        {
            int pageSize = 6;
            var requests = _context.TblSupportRequests
                .Include(r => r.Customer)
                .Include(r => r.Department)
                .AsQueryable();

            if (status.HasValue)
            {
                requests = requests.Where(r => r.Status == status.Value);
            }

            // Sorting logic
            switch (sortOrder)
            {
                case "title_asc":
                    requests = requests.OrderBy(r => r.RequestTitle);
                    break;
                case "title_desc":
                    requests = requests.OrderByDescending(r => r.RequestTitle);
                    break;
                case "date_asc":
                    requests = requests.OrderBy(r => r.CreatedAt);
                    break;
                case "date_desc":
                    requests = requests.OrderByDescending(r => r.CreatedAt);
                    break;
                case "status_asc":
                    requests = requests.OrderBy(r => r.Status);
                    break;
                case "status_desc":
                    requests = requests.OrderByDescending(r => r.Status);
                    break;
                default:
                    requests = requests.OrderBy(r => r.RequestId);
                    break;
            }

            int totalRequests = requests.Count();
            var paginatedRequests = requests.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = (int)Math.Ceiling(totalRequests / (double)pageSize);
            ViewData["SortOrder"] = sortOrder;
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

                var newRequestProcessing = new TblRequestsProcessing
                {
                    RequestId = tblSupportRequest.RequestId,
                    DepartmentId = tblSupportRequest.DepartmentId,
                    IsCompleted = 0, // Đánh dấu là chưa xử lý
                    ProcessedAt = DateTime.Now,
                    Note = "Yêu cầu được tạo mới"
                };

                // Lưu vào database
                _context.TblRequestsProcessings.Add(newRequestProcessing);
                await _context.SaveChangesAsync();

                var departmentManager = _context.TblUsers
                    .FirstOrDefault(u => u.DepartmentId == tblSupportRequest.DepartmentId && u.Role.RoleName == "Trưởng phòng");

                if (departmentManager != null)
                {
                    _logService.LogNotificationAction(departmentManager.UserId.ToString(), "Bạn có yêu cầu mới");
                }
                return RedirectToAction(nameof(Index), new { page = currentPage });
            }
            ViewData["CustomerId"] = new SelectList(_context.TblCustomers, "CustomerId", "CustomerId", tblSupportRequest.CustomerId);
            ViewData["DepartmentId"] = new SelectList(_context.TblDepartments, "DepartmentId", "DepartmentId", tblSupportRequest.DepartmentId);
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
        //public async Task<IActionResult> Transfer(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var tblSupportRequest = await _context.TblSupportRequests
        //        .Include(t => t.Customer)
        //        .Include(t => t.Department)
        //        .FirstOrDefaultAsync(m => m.RequestId == id);
        //    if (tblSupportRequest == null)
        //    {
        //        return NotFound();
        //    }

        //    var requestTransfer = new TblRequestTransfer
        //    {
        //        RequestId = tblSupportRequest.RequestId,
        //        FromDepartmentId = tblSupportRequest.DepartmentId,
        //        TransferredAt = DateTime.Now // Set default values as needed
        //    };

        //    ViewData["RequestId"] = new SelectList(_context.TblSupportRequests, "RequestId", "RequestDetails", tblSupportRequest.RequestId);
        //    ViewData["FromDepartmentId"] = new SelectList(_context.TblDepartments, "DepartmentId", "DepartmentName", tblSupportRequest.DepartmentId);
        //    ViewData["ToDepartmentId"] = new SelectList(_context.TblDepartments, "DepartmentId", "DepartmentName");
        //    ViewData["TransferredBy"] = new SelectList(_context.TblUsers, "UserId", "FullName");

        //    return View(requestTransfer);
        //}


        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Transfer(int id, [Bind("TransferId,RequestId,FromDepartmentId,ToDepartmentId,Priority,TransferredBy,TransferredAt,Note")] TblRequestTransfer requestTransfer)
        //{
        //    if (id != requestTransfer.RequestId)
        //    {
        //        return NotFound();
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            // Add the new transfer record
        //            _context.Add(requestTransfer);

        //            // Update the department of the support request
        //            var supportRequest = await _context.TblSupportRequests.FindAsync(requestTransfer.RequestId);
        //            if (supportRequest != null)
        //            {
        //                supportRequest.DepartmentId = requestTransfer.ToDepartmentId;
        //                _context.Update(supportRequest);
        //            }

        //            await _context.SaveChangesAsync();
        //        }
        //        catch (DbUpdateConcurrencyException)
        //        {
        //            if (!TblRequestTransferExists(requestTransfer.TransferId))
        //            {
        //                return NotFound();
        //            }
        //            else
        //            {
        //                throw;
        //            }
        //        }
        //        return RedirectToAction(nameof(Index));
        //    }

        //    ViewData["RequestId"] = new SelectList(_context.TblSupportRequests, "RequestId", "RequestDetails", requestTransfer.RequestId);
        //    ViewData["FromDepartmentId"] = new SelectList(_context.TblDepartments, "DepartmentId", "DepartmentName", requestTransfer.FromDepartmentId);
        //    ViewData["ToDepartmentId"] = new SelectList(_context.TblDepartments, "DepartmentId", "DepartmentName", requestTransfer.ToDepartmentId);
        //    ViewData["TransferredBy"] = new SelectList(_context.TblUsers, "UserId", "FullName", requestTransfer.TransferredBy);

        //    return View(requestTransfer);
        //}

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
            ViewData["TransferredBy"] = new SelectList(_context.TblUsers, "UserId", "FullName");

            return View(requestTransfer);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Transfer(int id, [Bind("TransferId,RequestId,FromDepartmentId,Priority,TransferredBy,TransferredAt,Note,RequestTitle,Product")] TblRequestTransfer requestTransfer, List<int> ToDepartmentId, int currentPage = 1)
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

            return View(requestTransfer);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, int status, DateTime? resolvedAt)
        {
            var supportRequest = await _context.TblSupportRequests.FindAsync(id);
            if (supportRequest == null)
            {
                return NotFound();
            }

            supportRequest.Status = (short)status;
            supportRequest.ResolvedAt = status == 1 ? resolvedAt : null;

            try
            {
                _context.Update(supportRequest);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Chuyển trạng thái thành công";
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
        
    }
}