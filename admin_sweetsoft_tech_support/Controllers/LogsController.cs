using admin_sweetsoft_tech_support.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

namespace admin_sweetsoft_tech_support.Controllers
{
    public class LogsController : Controller
    {
        private readonly RequestContext _context;

        public LogsController(RequestContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate, int page = 1)
        {
            var logs = _context.TblLogs.AsQueryable();

            // Lọc dữ liệu

            if (startDate.HasValue)
                logs = logs.Where(l => l.CreatedAt >= startDate);

            if (endDate.HasValue)
                logs = logs.Where(l => l.CreatedAt <= endDate);

            var pageSize = 6; // số lượng mỗi trang
            var skip = (page - 1) * pageSize;

            var requestContext = logs
                .OrderByDescending(l => l.CreatedAt)
                .Skip(skip) // bỏ qua dữ liệu đã xem ở các trang trước
                .Take(pageSize);

            var totalUsers = await logs.CountAsync();

            // Tính tổng số trang
            var totalPages = (int)Math.Ceiling(totalUsers / (double)pageSize);
            ViewData["TotalPages"] = totalPages;
            ViewData["CurrentPage"] = page;
            return View(await requestContext.ToListAsync());
        }

        // Chi tiết log
        public async Task<IActionResult> Details(int id)
        {
            var log = await _context.TblLogs.FindAsync(id);
            if (log == null)
            {
                return NotFound();
            }
            return View(log);
        }

        public async Task<IActionResult> ExportToExcel()
        {
            var logs = await _context.TblLogs.ToListAsync();

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Logs");
                worksheet.Cells[1, 1].Value = "ID";
                worksheet.Cells[1, 2].Value = "User ID";
                worksheet.Cells[1, 3].Value = "Action";
                worksheet.Cells[1, 4].Value = "Description";
                worksheet.Cells[1, 5].Value = "Created At";

                for (int i = 0; i < logs.Count; i++)
                {
                    worksheet.Cells[i + 2, 1].Value = logs[i].LogId;
                    worksheet.Cells[i + 2, 2].Value = logs[i].UserId;
                    worksheet.Cells[i + 2, 3].Value = logs[i].Action;
                    worksheet.Cells[i + 2, 4].Value = logs[i].Description;
                    worksheet.Cells[i + 2, 5].Value = logs[i].CreatedAt?.ToString("dd/MM/yyyy");
                }

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "logs.xlsx");
            }
        }
    }
}
