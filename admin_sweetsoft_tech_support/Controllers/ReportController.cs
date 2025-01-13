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
                    Status = r.Status == 0 ? "Chưa hoàn thành" :
                             r.Status == 1 ? "Hoàn thành" :
                             r.Status == 2 ? "Không xử lý được" :
                             "Không xác định",

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
                    Status = r.Status == 0 ? "Chưa hoàn thành" :
                             r.Status == 1 ? "Hoàn thành" :
                             r.Status == 2 ? "Không xử lý được" :
                             "Không xác định",

                    CreatedAt = r.CreatedAt
                })
                .ToListAsync();

            // Trả về view với danh sách yêu cầu chi tiết
            return View(requests);
        }

        [HttpGet("api/requests/monthly")]
        public async Task<IActionResult> GetMonthlyRequestSummary(DateTime? startDate, DateTime? endDate)
        {
            // Lấy ngày hiện tại
            var now = DateTime.Now;

            // Nếu không cung cấp startDate và endDate, gán giá trị mặc định cho cả hai
            var defaultStartDate = new DateTime(now.Year, 1, 1); // Ngày đầu tiên của năm hiện tại
            var defaultEndDate = now;                           // Ngày hiện tại

            // Đảm bảo startDate <= endDate
            if (startDate > endDate)
            {
                return BadRequest("Ngày bắt đầu không thể trễ hơn ngày kết thúc");
            }

            // Danh sách tất cả các tháng trong khoảng thời gian từ startDate đến endDate
            var months = new List<(int Year, int Month)>();
            var current = new DateTime(startDate.Value.Year, startDate.Value.Month, 1);

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
                    g.Key.Year,
                    g.Key.Month,
                    Count = g.Count()
                })
                .ToListAsync();

            // Tạo danh sách kết quả với số lượng yêu cầu cho tất cả các tháng
            var result = months.Select(month =>
            {
                var request = requests.FirstOrDefault(r => r.Year == month.Year && r.Month == month.Month);
                return new
                {
                    month.Year,
                    month.Month,
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
                return BadRequest("Ngày bắt đầu không thể trễ hơn ngày kết thúc.");
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
                    r.Department.DepartmentName,
                    r.RequestDetails,
                    Status = r.Status == 0 ? "Chưa hoàn thành" :
                             r.Status == 1 ? "Hoàn thành" :
                             r.Status == 2 ? "Không xử lý được" :
                             "Không xác định",
                    r.CreatedAt
                })
                .ToListAsync();

            if (!supportRequests.Any())
            {
                return NotFound("Không có dữ liệu trong khoảng thời gian này.");
            }

            using var package = new OfficeOpenXml.ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("SupportRequests");

            // Thêm tiêu đề "Thống kê yêu cầu"
            worksheet.Cells[1, 1].Value = $"Thống kê yêu cầu ";
            worksheet.Cells[1, 1, 1, 7].Merge = true; // Gộp các cột từ 1 đến 7
            worksheet.Cells[1, 1].Style.Font.Size = 14;
            worksheet.Cells[1, 1].Style.Font.Bold = true;
            worksheet.Cells[1, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

            // Thêm thông tin từ ngày đến ngày
            worksheet.Cells[2, 1].Value = $"Từ ngày: {startDate:yyyy-MM-dd}  -  Đến ngày: {endDate:yyyy-MM-dd}";
            worksheet.Cells[2, 1, 2, 7].Merge = true; // Gộp các cột từ 1 đến 7
            worksheet.Cells[2, 1].Style.Font.Italic = true;
            worksheet.Cells[2, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

            // Thiết lập tiêu đề cột
            worksheet.Cells[3, 1].Value = "STT";
            worksheet.Cells[3, 2].Value = "Mã yêu cầu";
            worksheet.Cells[3, 3].Value = "Tên khách hàng";
            worksheet.Cells[3, 4].Value = "Bộ phận tiếp nhận";
            worksheet.Cells[3, 5].Value = "Thông tin yêu cầu";
            worksheet.Cells[3, 6].Value = "Ngày tạo";
            worksheet.Cells[3, 7].Value = "Trạng thái";

            // Tô màu nền cho tiêu đề cột
            worksheet.Cells[3, 1, 3, 7].Style.Font.Bold = true;
            worksheet.Cells[3, 1, 3, 7].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            worksheet.Cells[3, 1, 3, 7].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#E6E6FA")); // Tím nhạt

            // Đổ dữ liệu vào Excel
            for (int i = 0; i < supportRequests.Count; i++)
            {
                var request = supportRequests[i];
                worksheet.Cells[i + 4, 1].Value = i + 1; // STT
                worksheet.Cells[i + 4, 2].Value = request.RequestId;
                worksheet.Cells[i + 4, 3].Value = request.CustomerName;
                worksheet.Cells[i + 4, 4].Value = request.DepartmentName;
                worksheet.Cells[i + 4, 5].Value = request.RequestDetails;
                worksheet.Cells[i + 4, 6].Value = request.CreatedAt.ToString("yyyy-MM-dd");
                worksheet.Cells[i + 4, 7].Value = request.Status;
            }

            // Tự động điều chỉnh độ rộng cột
            worksheet.Cells[1, 1, supportRequests.Count + 3, 7].AutoFitColumns();

            // Tên file theo khoảng thời gian
            var fileName = $"Thống kê yêu cầu từ {startDate:yyyy-MM-dd} đến {endDate:yyyy-MM-dd}.xlsx";

            // Trả về file Excel
            var excelData = package.GetAsByteArray();
            return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
    }
}