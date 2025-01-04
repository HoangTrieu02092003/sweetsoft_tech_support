using admin_sweetsoft_tech_support.Attributes;
using admin_sweetsoft_tech_support.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace admin_sweetsoft_tech_support.Controllers
{
    public class NotificationsController : Controller
    {
        private readonly string _logDirectoryNoti = Path.Combine(Directory.GetCurrentDirectory(), "Notifications");
        private readonly RequestContext _context;

        public NotificationsController(RequestContext context)
        {
            _context = context;
        }
        // Hiển thị tất cả log
        public async Task<IActionResult> Index(string date = "", string searchTerm = null, string filterOption = "", int page = 1)
        {
            List<NotificationEntry> logs;
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
                    log.Content.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    log.Status.ToString().Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    log.Timestamp.ToString().Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
                    ).ToList();
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
        private async Task<List<NotificationEntry>> ReadLogsForPaginationAsync(int skip, int pageSize)
        {
            var logs = new List<NotificationEntry>();
            if (!Directory.Exists(_logDirectoryNoti))
            {
                return logs;
            }

            var logFiles = Directory.GetFiles(_logDirectoryNoti, "*.log", SearchOption.AllDirectories);
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
        private async Task<List<NotificationEntry>> ReadLogsForDatePaginationAsync(DateTime date, int skip, int pageSize)
        {
            var logs = new List<NotificationEntry>();
            var logDirectoryForDate = Path.Combine(_logDirectoryNoti, date.ToString("yyyy"), date.ToString("MM"), date.ToString("dd"));
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
        private async Task<List<NotificationEntry>> ReadLogsByDateAsync(DateTime date)
        {
            var logs = new List<NotificationEntry>();
            var logDirectoryForDate = Path.Combine(_logDirectoryNoti, date.ToString("yyyy"), date.ToString("MM"), date.ToString("dd"));
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
        private async Task<List<NotificationEntry>> ReadLogsForMonthPaginationAsync(DateTime month, int skip, int pageSize)
        {
            var logs = new List<NotificationEntry>();
            var logDirectoryForMonth = Path.Combine(_logDirectoryNoti, month.ToString("yyyy"), month.ToString("MM"));

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
        private async Task<List<NotificationEntry>> ReadLogsByMonthAsync(DateTime month)
        {
            var logs = new List<NotificationEntry>();
            var logDirectoryForMonth = Path.Combine(_logDirectoryNoti, month.ToString("yyyy"), month.ToString("MM"));

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
        private async Task<List<NotificationEntry>> ReadLogsForYearPaginationAsync(DateTime year, int skip, int pageSize)
        {
            var logs = new List<NotificationEntry>();
            var logDirectoryForYear = Path.Combine(_logDirectoryNoti, year.ToString("yyyy"));

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
        private async Task<List<NotificationEntry>> ReadLogsByYearAsync(DateTime year)
        {
            var logs = new List<NotificationEntry>();
            var logDirectoryForYear = Path.Combine(_logDirectoryNoti, year.ToString("yyyy"));

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
        private async Task<List<NotificationEntry>> ReadLogFileAsync(string filePath)
        {
            var logs = new List<NotificationEntry>();
            var lines = await System.IO.File.ReadAllLinesAsync(filePath); // Đọc file async
            foreach (var line in lines)
            {
                var log = await ParseLogLine(line);
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
            if (Directory.Exists(_logDirectoryNoti))
            {
                var logFiles = Directory.GetFiles(_logDirectoryNoti, "*.log", SearchOption.AllDirectories);
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
        private async Task<NotificationEntry> ParseLogLine(string line)
        {
            try
            {
                
                var parts = line.Split(", ");
                var user = await _context.TblUsers
                .Where(u => u.UserId == int.Parse(parts[2]))
                .FirstOrDefaultAsync();
                if (parts.Length < 4) return null;
                return new NotificationEntry
                {
                    Timestamp = DateTime.ParseExact(parts[0], "dd/MM/yyyy HH\\:mm", CultureInfo.InvariantCulture),
                    Status = parts[1],  // Gán giá trị đã chuyển đổi
                    User = user.FullName,
                    Content = parts[3]
                };
            }
            catch
            {
                return null;
            }
        }
    }
}