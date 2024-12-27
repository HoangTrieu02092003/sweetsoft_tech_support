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
        private readonly NotificationService _notificationService;

        public TblSupportRequestsController(RequestContext context, NotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        //// GET: TblSupportRequests
        //public async Task<IActionResult> Index()
        //{
        //    var requestContext = _context.TblSupportRequests.Include(t => t.Customer).Include(t => t.Department);
        //    return View(await requestContext.ToListAsync());
        //}
        [HttpGet]
        [Route("TblSupportRequests/Index")]
        public IActionResult Index(int? status, int page = 1)
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

            int totalRequests = requests.Count();
            var paginatedRequests = requests.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = (int)Math.Ceiling(totalRequests / (double)pageSize);
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RequestId,CustomerId,DepartmentId,RequestTitle,Product,RequestDetails,Status,CreatedAt,ResolvedAt")] TblSupportRequest tblSupportRequest)
        {
            if (!_context.TblCustomers.Any(c => c.CustomerId == tblSupportRequest.CustomerId))
            {
                ModelState.AddModelError("CustomerId", "Invalid CustomerId.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(tblSupportRequest);
                await _context.SaveChangesAsync();
                var departmentManager = _context.TblUsers
                    .FirstOrDefault(u => u.DepartmentId == tblSupportRequest.DepartmentId && u.Role.RoleName == "Trưởng phòng");
                await _notificationService.LogActionToFile(departmentManager.UserId,$"Bạn có yêu cầu mới");
                return RedirectToAction(nameof(Index));
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, [Bind("RequestId,RequestDetails,Status,CustomerId,DepartmentId,CreatedAt,ResolvedAt")] TblSupportRequest supportRequest)
        {
            if (id != supportRequest.RequestId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                _context.Update(supportRequest);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
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
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tblSupportRequest = await _context.TblSupportRequests.FindAsync(id);
            if (tblSupportRequest != null)
            {
                _context.TblSupportRequests.Remove(tblSupportRequest);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
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

            ViewData["RequestId"] = new SelectList(_context.TblSupportRequests, "RequestId", "RequestDetails", tblSupportRequest.RequestId);
            ViewData["FromDepartmentId"] = new SelectList(_context.TblDepartments, "DepartmentId", "DepartmentName", tblSupportRequest.DepartmentId);
            ViewData["ToDepartmentId"] = new SelectList(_context.TblDepartments, "DepartmentId", "DepartmentName");
            ViewData["TransferredBy"] = new SelectList(_context.TblUsers, "UserId", "FullName");

            return View(requestTransfer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Transfer(int id, [Bind("TransferId,RequestId,FromDepartmentId,Priority,TransferredBy,TransferredAt,Note")] TblRequestTransfer requestTransfer, List<int> ToDepartmentId)
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
                            supportRequest.DepartmentId = toDepartmentId;
                            _context.Update(supportRequest);
                        }
                        // Create a new support request for subsequent departments
                        else if (ToDepartmentId.Count > 1)
                        {
                            var newSupportRequest = new TblSupportRequest
                            {
                                CustomerId = supportRequest.CustomerId,
                                DepartmentId = toDepartmentId,
                                RequestDetails = supportRequest.RequestDetails,
                                Status = 0, // Assuming 0 is the default status
                                CreatedAt = DateTime.Now,
                                ResolvedAt = null
                            };

                            _context.Add(newSupportRequest);
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
                return RedirectToAction(nameof(Index));
            }

            ViewData["RequestId"] = new SelectList(_context.TblSupportRequests, "RequestId", "RequestDetails", requestTransfer.RequestId);
            ViewData["FromDepartmentId"] = new SelectList(_context.TblDepartments, "DepartmentId", "DepartmentName", requestTransfer.FromDepartmentId);
            ViewData["ToDepartmentId"] = new SelectList(_context.TblDepartments, "DepartmentId", "DepartmentName", requestTransfer.ToDepartmentId);
            ViewData["TransferredBy"] = new SelectList(_context.TblUsers, "UserId", "FullName", requestTransfer.TransferredBy);

            return View(requestTransfer);
        }   

    }
}