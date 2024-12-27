using admin_sweetsoft_tech_support.Attributes;
using admin_sweetsoft_tech_support.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System.Dynamic;

namespace admin_sweetsoft_tech_support.Controllers
{
    public class LogsController : Controller
    {
        private readonly RequestContext _context;
        private readonly LogService _logService;

        public LogsController(RequestContext context, LogService logService)
        {
            _context = context;
            _logService = logService;
        }

        public async Task<IActionResult> Index(string startDate, string endDate, int page = 1)
        {
            // Lọc logs từ database nếu có điều kiện ngày
            var query = _context.TblLogs.AsQueryable();

            if (!string.IsNullOrEmpty(startDate))
            {
                DateTime start = DateTime.Parse(startDate);
                query = query.Where(log => log.CreatedAt >= start);
            }

            if (!string.IsNullOrEmpty(endDate))
            {
                DateTime end = DateTime.Parse(endDate);
                query = query.Where(log => log.CreatedAt <= end);
            }

            var logsFromDb = await query.Skip((page - 1) * 10).Take(10).ToListAsync();

            // Đọc logs từ file
            var logsFromFile = GetLogsFromFile();

            // Tạo danh sách log từ database dưới dạng dynamic
            var logs = logsFromDb.Select(log =>
            {
                dynamic logItem = new ExpandoObject();
                logItem.UserId = log.UserId;
                logItem.Action = log.Action;
                logItem.Description = log.Description;
                logItem.CreatedAt = log.CreatedAt;
                return logItem;
            }).ToList();

            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = (int)Math.Ceiling((double)query.Count() / 10);

            return View(new Tuple<List<dynamic>, List<dynamic>>(logs, logsFromFile));
        }

        private List<dynamic> GetLogsFromFile()
        {
            var logs = new List<dynamic>();
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Logs", "application.log");

            if (System.IO.File.Exists(filePath))
            {
                var logLines = System.IO.File.ReadAllLines(filePath);

                foreach (var line in logLines)
                {
                    // Tách các phần từ log
                    var logParts = line.Split(new string[] { ": " }, StringSplitOptions.None);

                    if (logParts.Length == 2)
                    {
                        var dateTime = logParts[0];
                        var logDetails = logParts[1].Split(", ");

                        var userId = logDetails.FirstOrDefault(detail => detail.StartsWith("UserId"))?.Split('=')[1].Trim();
                        var action = logDetails.FirstOrDefault(detail => detail.StartsWith("Action"))?.Split('=')[1].Trim();
                        var description = logDetails.FirstOrDefault(detail => detail.StartsWith("Description"))?.Split('=')[1].Trim();

                        logs.Add(new
                        {
                            CreatedAt = DateTime.Parse(dateTime),
                            UserId = userId,
                            Action = action,
                            Description = description
                        });
                    }
                }
            }

            return logs;
        }

        public async Task<IActionResult> ExportToExcel()
        {
            // Lấy dữ liệu từ database
            var logsFromDb = await _context.TblLogs.ToListAsync();

            // Giả sử bạn đã lấy logs từ file vào logsFromFile
            var logsFromFile = GetLogsFromFile();

            // Kết hợp cả hai nguồn dữ liệu
            var allLogs = logsFromDb.Concat(logsFromFile).ToList();

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Logs");

                // Đặt tiêu đề cho các cột
                worksheet.Cells[1, 1].Value = "User ID";
                worksheet.Cells[1, 2].Value = "Action";
                worksheet.Cells[1, 3].Value = "Description";
                worksheet.Cells[1, 4].Value = "Created At";

                // Xuất dữ liệu vào file Excel
                for (int i = 0; i < allLogs.Count; i++)
                {
                    var log = allLogs[i];
                    worksheet.Cells[i + 2, 2].Value = log.UserId;
                    worksheet.Cells[i + 2, 3].Value = log.Action;
                    worksheet.Cells[i + 2, 4].Value = log.Description;
                    worksheet.Cells[i + 2, 5].Value = log.CreatedAt?.ToString("dd/MM/yyyy HH:mm");
                }

                // Lưu vào bộ nhớ và trả về file
                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "logs.xlsx");
            }
        }
    }
}
