using admin_sweetsoft_tech_support.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch.Internal;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Security.Claims;
using System.Threading.Tasks;

namespace admin_sweetsoft_tech_support.Controllers
{
    public class AuditLogsController : Controller
    {
        private readonly string _logDirectory = Path.Combine(Directory.GetCurrentDirectory(), "logs");

        // Hiển thị tất cả log
        public async Task<IActionResult> Index(string date = "", string searchTerm = null, string filterOption = "", int page = 1)
        {
            var currentUserIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserIdString) || !int.TryParse(currentUserIdString, out int currentUserId))
            {
                TempData["ReturnUrl"] = Request.Path.ToString();
                return RedirectToAction("Login", "Admin");
            }
            List<AuditLogEntry> logs;
            var pageSize = 5; // số lượng log mỗi trang
            var skip = (page - 1) * pageSize;
            var totalLogs = await CountLogsAsync(); // Đếm tổng số log async

            // Kiểm tra filterOption và ngày, xử lý từng trường hợp
            if (string.IsNullOrEmpty(filterOption))
            {
                logs = await ReadLogsForPaginationAsync(skip, pageSize);
            }
            else
            {
                date = DateTime.Today.ToString("yyyy-MM-dd");
                if (filterOption == "day")
                {
                    if (DateTime.TryParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
                    {
                        logs = await ReadLogsForDatePaginationAsync(parsedDate, skip, pageSize);
                        var dateLogs = await ReadLogsByDateAsync(parsedDate);
                        totalLogs = dateLogs.Count;
                    }
                    else
                    {
                        return BadRequest("Invalid date format. Expected format: yyyy-MM-dd.");
                    }
                }
                else if (filterOption == "month")
                {
                    var monthDate = date.Substring(5, 2);
                    if (DateTime.TryParseExact(monthDate, "MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedMonth))
                    {
                        logs = await ReadLogsForMonthPaginationAsync(parsedMonth, skip, pageSize);
                        var monthLogs = await ReadLogsByMonthAsync(parsedMonth);
                        totalLogs = monthLogs.Count;
                    }
                    else
                    {
                        return BadRequest("Invalid month format. Expected format: yyyy-MM.");
                    }
                }
                else if (filterOption == "year")
                {
                    var yearMonthDate = date.Substring(0, 4);
                    if (DateTime.TryParseExact(yearMonthDate, "yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedYear))
                    {
                        logs = await ReadLogsForYearPaginationAsync(parsedYear, skip, pageSize);
                        var yearLogs = await ReadLogsByYearAsync(parsedYear);
                        totalLogs = yearLogs.Count;
                    }
                    else
                    {
                        return BadRequest("Invalid year format. Expected format: yyyy.");
                    }
                }
                else
                {
                    return BadRequest("Invalid filter option.");
                }
            }
            if (!string.IsNullOrEmpty(searchTerm))
            {
                // Lọc logs theo tiêu chí tìm kiếm
                logs = logs.Where(log =>
                    log.User.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    log.Action.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    log.Module?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) == true ||
                    log.Message.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
            
            var totalPages = (int)Math.Ceiling(totalLogs / (double)pageSize); // Tính tổng số trang
            ViewData["SearchTerm"] = searchTerm;
            ViewData["Date"] = date;
            ViewData["FilterOption"] = filterOption;
            ViewData["TotalPages"] = totalPages;
            ViewData["CurrentPage"] = page;

            return View(logs);
        }

        // Đọc log cho phân trang (Async)
        private async Task<List<AuditLogEntry>> ReadLogsForPaginationAsync(int skip, int pageSize)
        {
            var logs = new List<AuditLogEntry>();
            if (!Directory.Exists(_logDirectory))
            {
                return logs;
            }

            var logFiles = Directory.GetFiles(_logDirectory, "*.log", SearchOption.AllDirectories);
            foreach (var file in logFiles)
            {
                var fileLogs = await ReadLogFileAsync(file); // Đọc log file async
                logs.AddRange(fileLogs);
            }
    
            return logs.OrderByDescending(log => log.Timestamp)
                        .Skip(skip) // Bỏ qua số dòng đã đọc
                        .Take(pageSize) // Lấy số dòng cần thiết cho trang
                        .ToList();
        }

        // Đọc log theo ngày phân trang (Async)
        private async Task<List<AuditLogEntry>> ReadLogsForDatePaginationAsync(DateTime date, int skip, int pageSize)
        {
            var logs = new List<AuditLogEntry>();
            var logDirectoryForDate = Path.Combine(_logDirectory, date.ToString("yyyy"), date.ToString("MM"), date.ToString("dd"));
            if (Directory.Exists(logDirectoryForDate))
            {
                var logFiles = Directory.GetFiles(logDirectoryForDate, $"{date:yyyy-MM-dd}-*.log");

                foreach (var file in logFiles)
                {
                    Console.WriteLine(file);
                    var fileLogs = await ReadLogFileAsync(file);
                    logs.AddRange(fileLogs);
                }
            }

            return logs.OrderByDescending(log => log.Timestamp)
                        .Skip(skip) // Bỏ qua số dòng đã đọc
                        .Take(pageSize) // Lấy số dòng cần thiết cho trang
                        .ToList();
        }

        // Đọc log theo ngày (Async)
        private async Task<List<AuditLogEntry>> ReadLogsByDateAsync(DateTime date)
        {
            var logs = new List<AuditLogEntry>();
            var logDirectoryForDate = Path.Combine(_logDirectory, date.ToString("yyyy"), date.ToString("MM"), date.ToString("dd"));
            if (Directory.Exists(logDirectoryForDate))
            {
                var logFiles = Directory.GetFiles(logDirectoryForDate, $"{date:yyyy-MM-dd}-*.log");

                foreach (var file in logFiles)
                {
                    var fileLogs = await ReadLogFileAsync(file);
                    logs.AddRange(fileLogs);
                }
            }

            return logs.OrderByDescending(log => log.Timestamp).ToList();
        }

        // Đọc log theo tháng phân trang (Async)
        private async Task<List<AuditLogEntry>> ReadLogsForMonthPaginationAsync(DateTime month, int skip, int pageSize)
        {
            var logs = new List<AuditLogEntry>();
            var logDirectoryForMonth = Path.Combine(_logDirectory, month.ToString("yyyy"), month.ToString("MM"));

            if (Directory.Exists(logDirectoryForMonth))
            {
                // Lấy tất cả thư mục con trong tháng (tức là các ngày từ 01 đến 31)
                var dayDirectories = Directory.GetDirectories(logDirectoryForMonth);

                foreach (var dayDirectory in dayDirectories)
                {
                    // Lấy tất cả tệp log trong thư mục của từng ngày
                    var logFiles = Directory.GetFiles(dayDirectory, $"{month:yyyy-MM}-{Path.GetFileName(dayDirectory)}-*.log");

                    foreach (var file in logFiles)
                    {
                        var fileLogs = await ReadLogFileAsync(file);
                        logs.AddRange(fileLogs);
                    }
                }
            }

            return logs.OrderByDescending(log => log.Timestamp)
                        .Skip(skip)
                        .Take(pageSize)
                        .ToList();
        }


        // Đọc log theo tháng (Async)
        private async Task<List<AuditLogEntry>> ReadLogsByMonthAsync(DateTime month)
        {
            var logs = new List<AuditLogEntry>();
            var logDirectoryForMonth = Path.Combine(_logDirectory, month.ToString("yyyy"), month.ToString("MM"));

            if (Directory.Exists(logDirectoryForMonth))
            {
                // Lấy tất cả thư mục con trong tháng (tức là các ngày từ 01 đến 31)
                var dayDirectories = Directory.GetDirectories(logDirectoryForMonth);

                foreach (var dayDirectory in dayDirectories)
                {
                    // Lấy tất cả tệp log trong thư mục của từng ngày
                    var logFiles = Directory.GetFiles(dayDirectory, $"{month:yyyy-MM}-{Path.GetFileName(dayDirectory)}-*.log");

                    foreach (var file in logFiles)
                    {
                        Console.WriteLine(file);  // Để debug
                        var fileLogs = await ReadLogFileAsync(file);
                        logs.AddRange(fileLogs);
                    }
                }
            }

            return logs.OrderByDescending(log => log.Timestamp).ToList();
        }


        // Đọc log theo năm phân trang (Async)
        private async Task<List<AuditLogEntry>> ReadLogsForYearPaginationAsync(DateTime year, int skip, int pageSize)
        {
            var logs = new List<AuditLogEntry>();
            var logDirectoryForYear = Path.Combine(_logDirectory, year.ToString("yyyy"));

            if (Directory.Exists(logDirectoryForYear))
            {
                // Lấy tất cả thư mục con của năm (tức là các tháng từ 01 đến 12)
                var monthDirectories = Directory.GetDirectories(logDirectoryForYear);

                foreach (var monthDirectory in monthDirectories)
                {
                    // Lấy tất cả thư mục con của tháng (tức là các ngày từ 01 đến 31)
                    var dayDirectories = Directory.GetDirectories(monthDirectory);

                    foreach (var dayDirectory in dayDirectories)
                    {
                        // Lấy tất cả tệp log trong thư mục của ngày
                        var logFiles = Directory.GetFiles(dayDirectory, $"{year:yyyy}-{Path.GetFileName(monthDirectory)}-{Path.GetFileName(dayDirectory)}-*.log");

                        foreach (var file in logFiles)
                        {
                            var fileLogs = await ReadLogFileAsync(file);
                            logs.AddRange(fileLogs);
                        }
                    }
                }
            }

            return logs.OrderByDescending(log => log.Timestamp)
                        .Skip(skip)
                        .Take(pageSize)
                        .ToList();
        }

        // theo năm không phân trang
        private async Task<List<AuditLogEntry>> ReadLogsByYearAsync(DateTime year)
        {
            var logs = new List<AuditLogEntry>();
            var logDirectoryForYear = Path.Combine(_logDirectory, year.ToString("yyyy"));

            if (Directory.Exists(logDirectoryForYear))
            {
                // Lấy tất cả thư mục con của năm (tức là các tháng từ 01 đến 12)
                var monthDirectories = Directory.GetDirectories(logDirectoryForYear);

                foreach (var monthDirectory in monthDirectories)
                {
                    // Lấy tất cả thư mục con của tháng (tức là các ngày từ 01 đến 31)
                    var dayDirectories = Directory.GetDirectories(monthDirectory);

                    foreach (var dayDirectory in dayDirectories)
                    {
                        // Lấy tất cả tệp log trong thư mục của ngày
                        var logFiles = Directory.GetFiles(dayDirectory, $"{year:yyyy}-{Path.GetFileName(monthDirectory)}-{Path.GetFileName(dayDirectory)}-*.log");

                        foreach (var file in logFiles)
                        {
                            var fileLogs = await ReadLogFileAsync(file);
                            logs.AddRange(fileLogs);
                        }
                    }
                }
            }

            return logs.OrderByDescending(log => log.Timestamp).ToList();
        }

        // Đọc log từ một file (Async)
        private async Task<List<AuditLogEntry>> ReadLogFileAsync(string filePath)
        {
            var logs = new List<AuditLogEntry>();
            var lines = await System.IO.File.ReadAllLinesAsync(filePath); // Đọc file async
            foreach (var line in lines)
            {
                var log = ParseLogLine(line);
                if (log != null)
                {
                    logs.Add(log);
                }
            }
            return logs;
        }

        // Đếm tổng số log trong tất cả các file (Async)
        private async Task<int> CountLogsAsync()
        {
            var totalLogs = 0;
            if (Directory.Exists(_logDirectory))
            {
                var logFiles = Directory.GetFiles(_logDirectory, "*.log", SearchOption.AllDirectories);
                foreach (var file in logFiles)
                {
                    totalLogs += await CountLinesInFileAsync(file); // Đếm số dòng async
                }

            }
            return totalLogs;
        }

        // Đếm số dòng trong một file (Async)
        private async Task<int> CountLinesInFileAsync(string filePath)
        {
            var lineCount = 0;
            using (var reader = new StreamReader(filePath))
            {
                while (await reader.ReadLineAsync() != null)
                {
                    lineCount++;
                }
            }
            return lineCount;
        }

        // Phân tích một dòng log
        private AuditLogEntry ParseLogLine(string line)
        {
            try
            {
                var parts = line.Split(", ");
                if (parts.Length < 7) return null;

                return new AuditLogEntry
                {
                    Timestamp = DateTime.ParseExact(parts[0], "dd/MM/yyyy HH\\:mm", CultureInfo.InvariantCulture),
                    
                    Action = parts[1],
                    User = parts[2],
                    Module = string.IsNullOrWhiteSpace(parts[3]) ? null : parts[3],
                    OldValue = string.IsNullOrWhiteSpace(parts[4]) ? null : parts[4],
                    NewValue = string.IsNullOrWhiteSpace(parts[5]) ? null : parts[5],
                    Message = parts[6]
                };
            }
            catch
            {
                return null;
            }
        }
    }
}