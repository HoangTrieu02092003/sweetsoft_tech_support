using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using admin_sweetsoft_tech_support.Models;
using Newtonsoft.Json;
using report.Models;
using System.Security.Claims;

namespace admin_sweetsoft_tech_support.Controllers
{
    public class ReportController : Controller
    {
        private readonly RequestContext _context;

        public ReportController(RequestContext context)
        {
            _context = context;
        }

        // GET: Report
        public async Task<IActionResult> Index1(ReportFilterModel filter)
        {
            var currentUserIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserIdString) || !int.TryParse(currentUserIdString, out int currentUserId))
            {
                TempData["ReturnUrl"] = Request.Path.ToString();
                return RedirectToAction("Login", "Admin");
            }
            // Khởi tạo truy vấn cơ sở dữ liệu
            var query = _context.TblSupportRequests
                .Include(r => r.Customer) // Liên kết với bảng khách hàng
                .AsQueryable();

            // Áp dụng bộ lọc từ người dùng
            if (filter.CustomerId.HasValue)
                query = query.Where(r => r.CustomerId == filter.CustomerId);

            if (filter.DepartmentId.HasValue)
                query = query.Where(r => r.DepartmentId == filter.DepartmentId);

            if (filter.StartDate.HasValue)
                query = query.Where(r => r.CreatedAt >= filter.StartDate);

            if (filter.EndDate.HasValue)
                query = query.Where(r => r.CreatedAt <= filter.EndDate);

            // Tính tổng số lượng yêu cầu của mỗi khách hàng và thêm cột detail
            var reportData = await query
            .GroupBy(r => r.CustomerId) // Nhóm theo CustomerId
            .Select(g => new ReportSummaryViewModel
            {
                CustomerId = g.Key ?? 0, // Nếu CustomerId là null, thay thế bằng 0
                CustomerName = g.FirstOrDefault().Customer.FullName, // Lấy tên khách hàng từ bản ghi đầu tiên trong nhóm
                TotalRequests = g.Count(), // Đếm số lượng yêu cầu trong nhóm
                Detail = g.Select(r => new RequestDetail
                {
                    RequestId = r.RequestId,
                    RequestDetails = r.RequestDetails,
                    Status = r.Status == 1 ? "Pending" :
                             r.Status == 2 ? "Processing" :
                             r.Status == 3 ? "Completed" :
                             r.Status == 4 ? "Cannot be Resolved" :
                             "Unknown",

                    CreatedAt = r.CreatedAt
                }).ToList() // Lấy danh sách chi tiết các yêu cầu
            })
            .ToListAsync();

            return View(reportData);
        }

        public async Task<IActionResult> ShowRequestDetails(int customerId)
        {
            // Lấy tất cả các yêu cầu của khách hàng theo CustomerId
            var requests = await _context.TblSupportRequests
                .Include(r => r.Customer)
                .Where(r => r.CustomerId == customerId)
                .Select(r => new RequestDetail
                {
                    RequestId = r.RequestId,
                    RequestDetails = r.RequestDetails,
                    Status = r.Status == 1 ? "Pending" :
                             r.Status == 2 ? "Processing" :
                             r.Status == 3 ? "Completed" :
                             r.Status == 4 ? "Cannot be Resolved" :
                             "Unknown",
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync();

            // Trả về view với danh sách yêu cầu chi tiết
            return View(requests);
        }

        [HttpGet("api/requests/monthly")]
        public async Task<IActionResult> GetMonthlyRequestSummary(DateTime? startDate, DateTime? endDate)
        {
            // Lấy ngày mặc định nếu không có tham số startDate và endDate
            var now = DateTime.Now;
            var defaultStartDate = new DateTime(now.Year, 1, 1); // Ngày đầu tiên của năm hiện tại
            var defaultEndDate = new DateTime(now.Year, 12, 31); // Ngày cuối cùng của năm hiện tại

            startDate ??= defaultStartDate; // Gán giá trị mặc định nếu không cung cấp startDate
            endDate ??= defaultEndDate;     // Gán giá trị mặc định nếu không cung cấp endDate

            // Đảm bảo startDate <= endDate
            if (startDate > endDate)
            {
                return BadRequest("Start date cannot be later than end date.");
            }

            // Danh sách tất cả các tháng trong khoảng thời gian từ startDate đến endDate
            var months = new List<(int Year, int Month)>();
            var current = startDate.Value;

            while (current <= endDate.Value)
            {
                months.Add((current.Year, current.Month)); // Thêm năm và tháng vào danh sách
                current = current.AddMonths(1);           // Tiến tới tháng tiếp theo
            }

            // Lấy dữ liệu yêu cầu từ cơ sở dữ liệu
            var requests = await _context.TblSupportRequests
                .Where(r => r.CreatedAt >= startDate && r.CreatedAt <= endDate)
                .GroupBy(r => new { r.CreatedAt.Year, r.CreatedAt.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Count = g.Count()
                })
                .ToListAsync();

            // Tạo danh sách kết quả với số lượng yêu cầu cho tất cả các tháng
            var result = months.Select(month =>
            {
                var request = requests.FirstOrDefault(r => r.Year == month.Year && r.Month == month.Month);
                return new
                {
                    Year = month.Year,
                    Month = month.Month,
                    Count = request?.Count ?? 0 // Nếu không có yêu cầu, set count = 0
                };
            }).ToList();

            // Trả về kết quả dưới dạng JSON
            return Ok(new { monthlySummary = result });
        }


        [HttpGet("api/requests/status-summary")]
        public async Task<IActionResult> GetStatusRequest(DateTime? startDate, DateTime? endDate)
        {
            // Nếu không có startDate và endDate, mặc định lấy dữ liệu của năm hiện tại
            if (!startDate.HasValue && !endDate.HasValue)
            {
                var currentYear = DateTime.Now.Year;
                startDate = new DateTime(currentYear, 1, 1);  // Ngày bắt đầu của năm
                endDate = new DateTime(currentYear, 12, 31); // Ngày kết thúc của năm
            }

            // Kiểm tra ngày hợp lệ
            if (startDate.HasValue && endDate.HasValue && startDate.Value > endDate.Value)
            {
                return BadRequest("Start date cannot be later than end date.");
            }

            IQueryable<TblSupportRequest> query = _context.TblSupportRequests;

            // Lọc theo startDate và endDate nếu có
            if (startDate.HasValue)
            {
                query = query.Where(r => r.CreatedAt >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(r => r.CreatedAt <= endDate.Value);
            }

            // Nhóm theo trạng thái và đếm số lượng yêu cầu
            var requests = await query
                .GroupBy(r => r.Status)
                .Select(g => new
                {
                    Status = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            return Ok(new { requests });
        }


        [HttpGet("api/requests/department-summary")]
        public async Task<IActionResult> GetDepartmentRequest([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            // Nếu không có giá trị nào được cung cấp, mặc định là cả năm
            startDate ??= new DateTime(DateTime.Now.Year, 1, 1);
            endDate ??= new DateTime(DateTime.Now.Year, 12, 31);

            Console.WriteLine("GetDepartmentRequest method was called");
            Console.WriteLine($"Start Date: {startDate}, End Date: {endDate}");

            // Lấy số lượng yêu cầu theo từng phòng ban
            var departmentRequests = await _context.TblSupportRequests
                .Where(r => r.CreatedAt >= startDate && r.CreatedAt <= endDate)
                .GroupBy(r => r.DepartmentId)
                .Select(g => new
                {
                    DepartmentId = g.Key,
                    DepartmentName = g.Select(r => r.Department.DepartmentName).FirstOrDefault(),
                    Count = g.Count()
                })
                .ToListAsync();

            // Tính tổng số yêu cầu
            var totalRequests = departmentRequests.Sum(r => r.Count);

            // Tính phần trăm cho mỗi phòng ban
            var departmentPercentages = departmentRequests.Select(r => new
            {
                r.DepartmentId,
                r.DepartmentName,
                r.Count,
                Percentage = totalRequests > 0 ? Math.Round(((double)r.Count / totalRequests) * 100, 2) : 0
            }).ToList();

            return Ok(new { departmentPercentages });
        }

         [HttpGet("api/requests/export-excel")]
        public async Task<IActionResult> ExportSupportRequestsToExcel(DateTime? startDate, DateTime? endDate)
        {
            // Kiểm tra ngày bắt đầu và kết thúc
            if (!startDate.HasValue || !endDate.HasValue)
            {
                return BadRequest("Mời bạn nhập ngày bắt đầu và kết thúc!");
            }

            // Đảm bảo startDate <= endDate
            if (startDate.Value > endDate.Value)
            {
                return BadRequest("Ngày bắt đầu không thể trễ hơn ngày kết thúc!");
            }

            // Lấy dữ liệu từ bảng TblSupportRequests trong khoảng thời gian được chọn
            var supportRequests = await _context.TblSupportRequests
                .Include(r => r.Customer)
                .Include(r => r.Department)
                .Where(r => r.CreatedAt >= startDate.Value && r.CreatedAt <= endDate.Value)
                .Select(r => new
                {
                    r.RequestId,
                    CustomerName = r.Customer.FullName,
                    DepartmentName = r.Department.DepartmentName,
                    r.RequestDetails,
                    Status = r.Status == 1 ? "Chưa xử lý" :
                             r.Status == 2 ? "Đang xử lý" :
                             r.Status == 3 ? "Đã xử lý" :
                             r.Status == 4 ? "Không xử lý được" :
                             "Unknown",
                    r.CreatedAt
                })
                .ToListAsync();

            if (!supportRequests.Any())
            {
                return NotFound("Không có dữ liệu trong khoảng thời gian này.");
            }

            using var package = new OfficeOpenXml.ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("SupportRequests");

            // Thiết lập tiêu đề cột
            worksheet.Cells[1, 1].Value = "STT";
            worksheet.Cells[1, 2].Value = "Mã yêu cầu";
            worksheet.Cells[1, 3].Value = "Tên khách hàng";
            worksheet.Cells[1, 4].Value = "Tên bộ phận";
            worksheet.Cells[1, 5].Value = "Thông tin yêu cầu";
            worksheet.Cells[1, 6].Value = "Trạng thái";
            worksheet.Cells[1, 7].Value = "Ngày tạo";

            // Đổ dữ liệu vào Excel
            for (int i = 0; i < supportRequests.Count; i++)
            {
                var request = supportRequests[i];
                worksheet.Cells[i + 2, 1].Value = i + 1; // STT
                worksheet.Cells[i + 2, 2].Value = request.RequestId;
                worksheet.Cells[i + 2, 3].Value = request.CustomerName;
                worksheet.Cells[i + 2, 4].Value = request.DepartmentName;
                worksheet.Cells[i + 2, 5].Value = request.RequestDetails;
                worksheet.Cells[i + 2, 6].Value = request.Status;
                worksheet.Cells[i + 2, 7].Value = request.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss");
            }

            // Định dạng bảng
            worksheet.Cells[1, 1, 1, 7].Style.Font.Bold = true; // Tiêu đề in đậm
            worksheet.Cells[1, 1, supportRequests.Count + 1, 7].AutoFitColumns(); // Tự động chỉnh độ rộng cột

            // Trả về file Excel
            var excelData = package.GetAsByteArray();
            var fileName = $"SupportRequests_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
            return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }


        public async Task<IActionResult> Index()
        {
            var requestContext = _context.TblSupportRequests.Include(t => t.Customer).Include(t => t.Department);
            return View(await requestContext.ToListAsync());
        }

        // GET: Report/Details/5
        public async Task<IActionResult> Details(int? id)
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

        // GET: Report/Create
        public IActionResult Create()
        {
            ViewData["CustomerId"] = new SelectList(_context.TblCustomers, "CustomerId", "CustomerId");
            ViewData["DepartmentId"] = new SelectList(_context.TblDepartments, "DepartmentId", "DepartmentId");
            return View();
        }

        // POST: Report/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RequestId,CustomerId,DepartmentId,RequestDetails,Status,CreatedAt,ResolvedAt")] TblSupportRequest tblSupportRequest)
        {
            if (ModelState.IsValid)
            {
                _context.Add(tblSupportRequest);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CustomerId"] = new SelectList(_context.TblCustomers, "CustomerId", "CustomerId", tblSupportRequest.CustomerId);
            ViewData["DepartmentId"] = new SelectList(_context.TblDepartments, "DepartmentId", "DepartmentId", tblSupportRequest.DepartmentId);
            return View(tblSupportRequest);
        }

        // GET: Report/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tblSupportRequest = await _context.TblSupportRequests.FindAsync(id);
            if (tblSupportRequest == null)
            {
                return NotFound();
            }
            ViewData["CustomerId"] = new SelectList(_context.TblCustomers, "CustomerId", "CustomerId", tblSupportRequest.CustomerId);
            ViewData["DepartmentId"] = new SelectList(_context.TblDepartments, "DepartmentId", "DepartmentId", tblSupportRequest.DepartmentId);
            return View(tblSupportRequest);
        }

        // POST: Report/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("RequestId,CustomerId,DepartmentId,RequestDetails,Status,CreatedAt,ResolvedAt")] TblSupportRequest tblSupportRequest)
        {
            if (id != tblSupportRequest.RequestId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tblSupportRequest);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TblSupportRequestExists(tblSupportRequest.RequestId))
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
            ViewData["CustomerId"] = new SelectList(_context.TblCustomers, "CustomerId", "CustomerId", tblSupportRequest.CustomerId);
            ViewData["DepartmentId"] = new SelectList(_context.TblDepartments, "DepartmentId", "DepartmentId", tblSupportRequest.DepartmentId);
            return View(tblSupportRequest);
        }

        // GET: Report/Delete/5
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

        // POST: Report/Delete/5
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
    }
}
