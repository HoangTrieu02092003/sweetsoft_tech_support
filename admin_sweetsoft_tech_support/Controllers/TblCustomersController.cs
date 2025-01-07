using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using admin_sweetsoft_tech_support.Models;
using OfficeOpenXml;
using admin_sweetsoft_tech_support.Attributes;
using System.Security.Claims;

namespace admin_sweetsoft_tech_support.Controllers
{
    public class TblCustomersController : Controller
    {
        private readonly RequestContext _context;

        public TblCustomersController(RequestContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? page, string Status, string SearchTerm, string sortColumn, string sortOrder)
        {
            var currentUserIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserIdString) || !int.TryParse(currentUserIdString, out int currentUserId))
            {
                TempData["ReturnUrl"] = Request.Path.ToString();
                return RedirectToAction("Login", "Admin");
            }
            // Mặc định trang hiện tại là 1 nếu chưa có
            int currentPage = page ?? 1;

            // Truy vấn cơ sở dữ liệu và lọc theo trạng thái
            var customersQuery = _context.TblCustomers
                .Where(c => c.IsDelete == false)
                .AsQueryable();

            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortOrder))
                customersQuery = TableSorter.Sort(customersQuery, sortColumn, sortOrder);

            if (!string.IsNullOrEmpty(Status))
            {
                // Lọc theo trạng thái
                if (Status == "1") // Kích hoạt
                {
                    customersQuery = customersQuery.Where(c => c.Status == 1);
                }
                else if (Status == "2") // Khóa
                {
                    customersQuery = customersQuery.Where(c => c.Status == 2);
                }
                else if (Status == "0") // Ngừng kích hoạt
                {
                    customersQuery = customersQuery.Where(c => c.Status == 0);
                }
            }

            // Lọc theo từ khóa tìm kiếm
            if (!string.IsNullOrEmpty(SearchTerm))
            {
                var lower = SearchTerm.ToLower();
                customersQuery = customersQuery.Where(c => c.FullName.ToLower().Contains(SearchTerm) || c.Email.ToLower().Contains(SearchTerm));
            }

            // Phân trang
            int pageSize = 6; // Số lượng khách hàng trên mỗi trang
            var totalItems = await customersQuery.CountAsync(); // Sử dụng CountAsync thay vì Count
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            var customers = await customersQuery
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(); // Sử dụng ToListAsync thay vì ToList

            // Thêm các ViewData cho phân trang, lọc trạng thái và từ khóa tìm kiếm
            ViewData["SortColumn"] = sortColumn;
            ViewData["SortOrder"] = sortOrder;
            ViewData["CurrentPage"] = currentPage;
            ViewData["TotalPages"] = totalPages;
            ViewData["Status"] = Status;
            ViewData["SearchTerm"] = SearchTerm;

            return View(customers);
        }


        [HttpPost]
        public async Task<IActionResult> ToggleActivation(int customerId)
        {
            var customer = await _context.TblCustomers.FindAsync(customerId);

            Console.WriteLine(customer.Status);
            //đổi trạng thái
            if (customer.Status == 0 || customer.Status == 2)
            {
                customer.Status = 1; // Chuyển về trạng thái đã kích hoạt
            }
            else if (customer.Status == 1)
            {
                customer.Status = 2; // Chuyển về trạng thái bị khóa
            }
            _context.Update(customer);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        [PermissionAuthorize("Quản lý khách hàng")]
        // GET: TblCustomers/Create
        public IActionResult Create()
        {
            ViewData["CreatedUser"] = new SelectList(_context.TblUsers, "UserId", "FullName");
            ViewData["UpdatedUser"] = new SelectList(_context.TblUsers, "UserId", "FullName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CustomerId,FullName,Email,Phone,TaxCode,Company,Username,Password,Status,ResetToken,ResetTokenExpiry,Token,TokenExpiry,CreatedUser,CreatedAt,UpdatedUser,UpdatedAt")] TblCustomer tblCustomer)
        {
            // Kiểm tra sự trùng lặp của Username
            bool isUsernameExist = await _context.TblCustomers.AnyAsync(c => c.Username == tblCustomer.Username && c.IsDelete == false);
            if (isUsernameExist)
            {
                TempData["ErrorMessage"] = "Username đã tồn tại. Vui lòng chọn một tên khác.";
            }

            // Kiểm tra sự trùng lặp của Email
            bool isEmailExist = await _context.TblCustomers.AnyAsync(c => c.Email == tblCustomer.Email && c.IsDelete == false);
            if (isEmailExist)
            {
                TempData["ErrorMessage"] = "Email đã tồn tại. Vui lòng sử dụng một email khác.";
            }

            // Nếu có lỗi trong ModelState, trả lại form để người dùng sửa
            if (!ModelState.IsValid)
            {
                tblCustomer.CreatedAt = DateTime.Now; // Mặc định là ngày hiện tại
                tblCustomer.UpdatedAt = DateTime.Now; // Mặc định là ngày hiện tại
                ViewData["CreatedUser"] = new SelectList(_context.TblUsers, "UserId", "FullName");
                ViewData["UpdatedUser"] = new SelectList(_context.TblUsers, "UserId", "FullName");
                return View(tblCustomer);
            }

            
            tblCustomer.Status = 0;
            tblCustomer.CreatedAt = DateTime.Now;
            tblCustomer.UpdatedAt = DateTime.Now;
            _context.Add(tblCustomer);
            await _context.SaveChangesAsync();

            // Thêm thông báo thành công vào TempData
            TempData["SuccessMessage"] = "Khách hàng đã được thêm thành công!";

            return RedirectToAction(nameof(Index)); // Chuyển hướng đến trang Index
        }


        // GET: TblCustomers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Fetch the customer and include support requests with department info
            var tblCustomer = await _context.TblCustomers
                                             .Include(c => c.TblSupportRequests)
                                                 .ThenInclude(sr => sr.Department) // Include the Department
                                             .Include(c => c.CreatedByNavigation)
                                             .Include(c => c.UpdatedByNavigation)
                                             .FirstOrDefaultAsync(m => m.CustomerId == id);

            if (tblCustomer == null)
            {
                return NotFound();
            }

            // Pass creator and updater information to ViewBag
            ViewBag.CreatedUser = tblCustomer.CreatedByNavigation?.FullName ?? "N/A";
            ViewBag.UpdatedUser = tblCustomer.UpdatedByNavigation?.FullName ?? "N/A";

            // Pass related data for other options
            ViewData["CreatedUser"] = new SelectList(_context.TblUsers, "UserId", "FullName", tblCustomer.CreatedBy);
            ViewData["UpdatedUser"] = new SelectList(_context.TblUsers, "UserId", "FullName", tblCustomer.UpdatedBy);

            return View(tblCustomer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CustomerId,FullName,Email,Phone,TaxCode,Company,Username,Password,Status,ResetToken,ResetTokenExpiry,Token,TokenExpiry,CreatedUser,CreatedAt,UpdatedUser,UpdatedAt")] TblCustomer tblCustomer)
        {
            if (id != tblCustomer.CustomerId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    bool isUsernameExist = await _context.TblCustomers.AnyAsync(c => c.Username == tblCustomer.Username && c.CustomerId != id);
                    if (isUsernameExist)
                    {
                        TempData["ErrorMessage"] = "Username đã tồn tại. Vui lòng chọn một tên khác.";
                    }

                    // Kiểm tra sự trùng lặp của Email
                    bool isEmailExist = await _context.TblCustomers.AnyAsync(c => c.Email == tblCustomer.Email && c.CustomerId != id);
                    if (isEmailExist)
                    {
                        TempData["ErrorMessage"] = "Email đã tồn tại. Vui lòng sử dụng một email khác.";
                    }

                    var existingCustomer = await _context.TblCustomers.FindAsync(id);
                    if (existingCustomer != null)
                    {
                        // Cập nhật các thông tin khách hàng
                        existingCustomer.FullName = tblCustomer.FullName;
                        existingCustomer.Email = tblCustomer.Email;
                        existingCustomer.Phone = tblCustomer.Phone;
                        existingCustomer.TaxCode = tblCustomer.TaxCode;
                        existingCustomer.Company = tblCustomer.Company;
                        existingCustomer.Username = tblCustomer.Username;
                        existingCustomer.Password = tblCustomer.Password;
                        existingCustomer.Status = tblCustomer.Status;

                        // Thiết lập ngày cập nhật là ngày hiện tại
                        existingCustomer.UpdatedAt = DateTime.Now;

                        _context.Update(existingCustomer);
                        await _context.SaveChangesAsync();

                        TempData["SuccessMessage"] = "Thông tin khách hàng đã được cập nhật thành công!";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Khách hàng không tồn tại!";
                    }
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

            ViewData["CreatedUser"] = new SelectList(_context.TblUsers, "UserId", "FullName", tblCustomer.CreatedBy);
            ViewData["UpdatedUser"] = new SelectList(_context.TblUsers, "UserId", "FullName", tblCustomer.UpdatedBy);
            return View(tblCustomer);
        }

        [PermissionAuthorize("Quản lý khách hàng")]
        // POST: TblCustomers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tblCustomer = await _context.TblCustomers.FindAsync(id);
            if (tblCustomer != null)
            {
                tblCustomer.IsDelete = true; // Đánh dấu là đã xóa
                tblCustomer.UpdatedAt = DateTime.Now; // Cập nhật ngày chỉnh sửa

                _context.Update(tblCustomer);
                await _context.SaveChangesAsync();

                // Thêm thông báo thành công vào TempData
                TempData["SuccessMessage"] = "Khách hàng đã được xóa thành công!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool TblCustomerExists(int id)
        {
            return _context.TblCustomers.Any(e => e.CustomerId == id);
        }

        // Action để xuất danh sách khách hàng ra file Excel
        public async Task<IActionResult> ExportToExcel()
        {
            var customers = await _context.TblCustomers
                .Include(t => t.CreatedByNavigation)
                .Include(t => t.UpdatedByNavigation)
                .Where(c => c.IsDelete == false)
                .ToListAsync();

            // Sử dụng EPPlus để tạo file Excel
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Customers");

                // Tạo tiêu đề cột
                worksheet.Cells[1, 1].Value = "Customer ID";
                worksheet.Cells[1, 2].Value = "Full Name";
                worksheet.Cells[1, 3].Value = "Email";
                worksheet.Cells[1, 4].Value = "Phone";
                worksheet.Cells[1, 5].Value = "Status";
                worksheet.Cells[1, 6].Value = "Created At";
                worksheet.Cells[1, 7].Value = "Updated At";

                // Tô đậm tiêu đề
                worksheet.Row(1).Style.Font.Bold = true;

                // Thêm dữ liệu khách hàng
                for (int i = 0; i < customers.Count; i++)
                {
                    var customer = customers[i];
                    worksheet.Cells[i + 2, 1].Value = customer.CustomerId;
                    worksheet.Cells[i + 2, 2].Value = customer.FullName;
                    worksheet.Cells[i + 2, 3].Value = customer.Email;
                    worksheet.Cells[i + 2, 4].Value = customer.Phone;
                    worksheet.Cells[i + 2, 5].Value = customer.Status == 1 ? "Active" : "Inactive";
                    worksheet.Cells[i + 2, 6].Value = customer.CreatedAt?.ToString("dd/MM/yyyy HH:mm"); 
                    worksheet.Cells[i + 2, 7].Value = customer.UpdatedAt?.ToString("dd/MM/yyyy HH:mm"); 
                }

                // Tự động căn chỉnh kích thước cột
                worksheet.Cells.AutoFitColumns();

                // Trả về file Excel dưới dạng FileStreamResult
                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                var fileName = $"Customers_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                return File(stream, contentType, fileName);
            }

        }
    }
}
