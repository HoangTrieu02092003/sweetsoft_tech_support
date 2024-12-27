using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using admin_sweetsoft_tech_support.Models;
using admin_sweetsoft_tech_support.Attributes;
using System.Security.Claims;

namespace admin_sweetsoft_tech_support.Controllers
{
    public class TblCustomersController : Controller
    {
        private readonly RequestContext _context;
        private readonly AuditLogService _auditLogService;

        public TblCustomersController(RequestContext context, AuditLogService auditLogService)
        {
            _context = context;
            _auditLogService = auditLogService;
        }

        // GET: TblCustomers
        public async Task<IActionResult> Index(int page = 1)
        {
            int pageSize = 6;

            // Include related users for CreatedUser and UpdatedUser
            var query = _context.TblCustomers.Include(t => t.CreatedByNavigation).Include(t => t.UpdatedByNavigation);

            // Get the total count of customers
            var totalCount = await query.CountAsync();

            // Fetch the paged list of customers
            var customers = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            // Calculate total pages for pagination
            ViewData["TotalPages"] = (int)Math.Ceiling(totalCount / (double)pageSize);
            ViewData["CurrentPage"] = page;

            // Dropdown lists for CreatedUser and UpdatedUser (if needed)
            ViewData["CreatedUser"] = new SelectList(_context.TblUsers, "UserId", "FullName");
            ViewData["UpdatedUser"] = new SelectList(_context.TblUsers, "UserId", "FullName");

            return View(customers);
        }


        [HttpPost]
        public async Task<IActionResult> ToggleActivation(int customerId)
        {
            var customer = await _context.TblCustomers.FindAsync(customerId);

            if (customer != null)
            {
                // Đổi trạng thái: Nếu hiện tại là 1 (kích hoạt), chuyển thành 0 (chưa kích hoạt) và ngược lại
                customer.Status = (short)(customer.Status == 1 ? 0 : 1);

                // Cập nhật khách hàng
                _context.Update(customer);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Trạng thái đã được thay đổi.";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: TblCustomers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tblCustomer = await _context.TblCustomers
                .Include(t => t.CreatedByNavigation)
                .Include(t => t.UpdatedByNavigation)
                .FirstOrDefaultAsync(m => m.CustomerId == id);
            if (tblCustomer == null)
            {
                return NotFound();
            }

            return View(tblCustomer);
        }

        // GET: TblCustomers/Create
        public IActionResult Create()
        {
            ViewData["CreatedUser"] = new SelectList(_context.TblUsers, "UserId", "FullName");
            ViewData["UpdatedUser"] = new SelectList(_context.TblUsers, "UserId", "FullName");
            return View();
        }

        // POST: TblCustomers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CustomerId,FullName,Email,Phone,TaxCode,Company,Product,Username,Password,Status,ResetToken,ResetTokenExpiry,Token,TokenExpiry,CreatedUser,CreatedAt,UpdatedUser,UpdatedAt")] TblCustomer tblCustomer)
        {
            // Kiểm tra sự trùng lặp của Username
            bool isUsernameExist = await _context.TblCustomers.AnyAsync(c => c.Username == tblCustomer.Username);
            if (isUsernameExist)
            {
                ModelState.AddModelError("Username", "Username đã tồn tại. Vui lòng chọn một tên khác.");
            }

            // Kiểm tra sự trùng lặp của Email
            bool isEmailExist = await _context.TblCustomers.AnyAsync(c => c.Email == tblCustomer.Email);
            if (isEmailExist)
            {
                ModelState.AddModelError("Email", "Email đã tồn tại. Vui lòng sử dụng một email khác.");
            }

            // Nếu có lỗi trong ModelState, trả lại form để người dùng sửa
            if (!ModelState.IsValid)
            {
                ViewData["CreatedUser"] = new SelectList(_context.TblUsers, "UserId", "FullName");
                ViewData["UpdatedUser"] = new SelectList(_context.TblUsers, "UserId", "FullName");
                return View(tblCustomer);
            }

            // Nếu không có lỗi, thêm khách hàng vào cơ sở dữ liệu
            tblCustomer.Status = 0;
            _context.Add(tblCustomer);
            await _context.SaveChangesAsync();
            _auditLogService.LogActionToFile("Khách hàng", tblCustomer.CustomerId, "Thêm", int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value), "", Newtonsoft.Json.JsonConvert.SerializeObject(tblCustomer));
            return RedirectToAction(nameof(Index));
        }

        // GET: TblCustomers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Truy vấn thông tin khách hàng và các yêu cầu hỗ trợ liên quan
            var tblCustomer = await _context.TblCustomers
                                            .Include(c => c.TblSupportRequests) // Bao gồm dữ liệu yêu cầu hỗ trợ
                                            .Include(c => c.CreatedByNavigation)
                                            .Include(c => c.UpdatedByNavigation)
                                            .FirstOrDefaultAsync(m => m.CustomerId == id);

            if (tblCustomer == null)
            {
                return NotFound();
            }

            // Truyền dữ liệu Customer và yêu cầu hỗ trợ vào View
            ViewData["CreatedUser"] = new SelectList(_context.TblUsers, "UserId", "FullName", tblCustomer.CreatedBy);
            ViewData["UpdatedUser"] = new SelectList(_context.TblUsers, "UserId", "FullName", tblCustomer.UpdatedBy);

            return View(tblCustomer); // Trả lại View với dữ liệu khách hàng và các yêu cầu hỗ trợ
        }

        // POST: TblCustomers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CustomerId,FullName,Email,Phone,TaxCode,Company,Product,Username,Password,Status,ResetToken,ResetTokenExpiry,Token,TokenExpiry,CreatedUser,CreatedAt,UpdatedUser,UpdatedAt")] TblCustomer tblCustomer, List<TblSupportRequest> updatedSupportRequests)
        {
            if (id != tblCustomer.CustomerId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Cập nhật thông tin khách hàng
                    _context.Update(tblCustomer);

                    // Cập nhật các yêu cầu hỗ trợ
                    foreach (var supportRequest in updatedSupportRequests)
                    {
                        var existingRequest = await _context.TblSupportRequests.FindAsync(supportRequest.RequestId);
                        if (existingRequest != null)
                        {
                            existingRequest.RequestDetails = supportRequest.RequestDetails; // Ví dụ, cập nhật chi tiết yêu cầu
                            existingRequest.Status = supportRequest.Status; // Cập nhật trạng thái yêu cầu
                            
                        }
                    }

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TblCustomerExists(tblCustomer.CustomerId))
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
            ViewData["CreatedUser"] = new SelectList(_context.TblUsers, "UserId", "UserId", tblCustomer.CreatedBy);
            ViewData["UpdatedUser"] = new SelectList(_context.TblUsers, "UserId", "UserId", tblCustomer.UpdatedBy);
            return View(tblCustomer);
        }



        // POST: TblCustomers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tblCustomer = await _context.TblCustomers.FindAsync(id);
            if (tblCustomer != null)
            {
                _context.TblCustomers.Remove(tblCustomer);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TblCustomerExists(int id)
        {
            return _context.TblCustomers.Any(e => e.CustomerId == id);
        }
    }
}
