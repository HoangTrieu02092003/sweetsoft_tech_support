using admin_sweetsoft_tech_support.Attributes;
using admin_sweetsoft_tech_support.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Security.Claims;

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
        public async Task<IActionResult> Index(string date = "", string searchTerm = "", string filterOption = "", int page = 1)
        {
            var currentUserIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserIdString) || !int.TryParse(currentUserIdString, out int currentUserId))
            {
                TempData["ReturnUrl"] = Request.Path.ToString();
                return RedirectToAction("Login", "Admin");
            }

            var userRole = _context.TblUsers
                 .Where(u => u.UserId == currentUserId)
                 .FirstOrDefault();

            // Kiểm tra nếu người dùng không phải là admin
            if (userRole.IsAdmin == false)
            {
                TempData["ErrorMessage"] = "Tài khoản hiện tại không có quyền vào trang này.";
                return RedirectToAction("Index1", "Report");
            }
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
        // Hiển thị thông báo của người dùng hiện tại
        public async Task<IActionResult> MyNotifications(string date = "", string filterOption = "", int page = 1)
        {
            var pageSize = 5; // Số thông báo mỗi trang
            var skip = (page - 1) * pageSize;

            // Lấy userId của người dùng hiện tại
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(currentUserId))
            {
                return RedirectToAction("Login", "Admin");
            }

            var logDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Notifications");
            var logFiles = Directory.GetFiles(logDirectory, "*.log", SearchOption.AllDirectories);
            var allLogs = new List<NotificationEntry>();

            // Đọc thông báo từ tất cả các file log
            foreach (var file in logFiles)
            {
                var fileLogs = await ReadLogFileForMyAsync(file);
                allLogs.AddRange(fileLogs);
            }

            // Lọc thông báo của người dùng hiện tại
            var logs = allLogs
                .Where(log => log.User == currentUserId && log.isDelete == "0")
                .OrderByDescending(log => log.Timestamp)
                .Skip(skip)
                .Take(pageSize)
                .ToList();

            // Tổng số thông báo
            var totalLogs = allLogs.Count(log => log.User == currentUserId && log.isDelete == "0");
            var totalPages = (int)Math.Ceiling(totalLogs / (double)pageSize);

            // Ánh xạ UserId sang FullName
            foreach (var log in logs)
            {
                var userId = log.User;
                var user = await _context.TblUsers
                    .Where(u => u.UserId == int.Parse(userId))
                    .FirstOrDefaultAsync();

                log.User = user != null ? user.FullName : "Không rõ";
            }

            // Gửi dữ liệu về View
            ViewData["Date"] = date;
            ViewData["FilterOption"] = filterOption;
            ViewData["TotalPages"] = totalPages;
            ViewData["CurrentPage"] = page;

            return View(logs);
        }

        // Xóa thông báo (MyNotification)
        [HttpPost]
        public async Task<IActionResult> DeleteNotification(string id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var logDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Notifications");
            var logFiles = Directory.GetFiles(logDirectory, "*.log", SearchOption.AllDirectories);
            var isDeleted = false;

            // Duyệt qua tất cả các file log
            foreach (var file in logFiles)
            {
                var fileLogs = await ReadLogFileForMyAsync(file); // Đọc các log từ file

                // Duyệt qua tất cả các bản ghi và thay đổi isDelete từ "0" thành "1" nếu trùng với isDelete và userId
                foreach (var log in fileLogs)
                {                    
                    // Kiểm tra nếu log trùng với id và userId, sau đó cập nhật status
                    if (log.User == currentUserId && log.Id == id && log.isDelete == "0")
                    {                        
                        log.isDelete = "1";
                        isDeleted = true;
                    }
                }

                // Ghi lại lại file log với các dòng đã thay đổi status
                var linesToWrite = fileLogs.Select(log => $"{log.Timestamp:dd/MM/yyyy HH:mm}, {log.Status}, {log.User}, {log.Id}, {log.Title}, {log.isDelete}, {log.Content}");
                await System.IO.File.WriteAllLinesAsync(file, linesToWrite);
            }

            // Nếu có thay đổi, chuyển hướng về MyNotifications, nếu không chuyển về Index
            if (isDeleted)
            {
                return RedirectToAction(nameof(MyNotifications)); // Nếu có thay đổi, về trang MyNotifications
            }
            else
            {
                return RedirectToAction(nameof(Index),controllerName:"ReportController"); // Nếu không có bản ghi nào bị thay đổi
            }
        }

        // Đã xem thông báo (MyNotification)
        [HttpPost]
        public async Task<IActionResult> MarkAsRead(string id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var logDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Notifications");
            var logFiles = Directory.GetFiles(logDirectory, "*.log", SearchOption.AllDirectories);
            var isViewed = false;
            // Duyệt qua tất cả các file log
            foreach (var file in logFiles)
            {
                var fileLogs = await ReadLogFileForMyAsync(file); // Đọc các log từ file

                // Duyệt qua tất cả các bản ghi và thay đổi status từ "0" thành "1" nếu trùng với status và userId
                foreach (var log in fileLogs)
                {
                    // Kiểm tra nếu log trùng với id và userId, sau đó cập nhật status
                    if (log.Id == id && log.Status == "0")
                    {
                        
                        log.Status = "1";
                        isViewed = true;
                    }
                }

                // Ghi lại lại file log với các dòng đã thay đổi status
                var linesToWrite = fileLogs.Select(log => $"{log.Timestamp:dd/MM/yyyy HH:mm}, {log.Status}, {log.User}, {log.Id}, {log.Title}, {log.isDelete}, {log.Content}");
                await System.IO.File.WriteAllLinesAsync(file, linesToWrite);
            }

            // Nếu có thay đổi, chuyển hướng về MyNotifications, nếu không chuyển về Index
            if (isViewed)
            {
                return RedirectToAction(nameof(MyNotifications)); // Nếu có thay đổi, về trang MyNotifications
            }
            return RedirectToAction(nameof(MyNotifications)); // Nếu không có bản ghi nào bị thay đổi
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
                var fileLogs = await ReadLogFileForIndexAsync(file); // Đọc log file async
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
                    var fileLogs = await ReadLogFileForIndexAsync(file);
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
                    var fileLogs = await ReadLogFileForIndexAsync(file);
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
                        var fileLogs = await ReadLogFileForIndexAsync(file);
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
                        var fileLogs = await ReadLogFileForIndexAsync(file);
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
                            var fileLogs = await ReadLogFileForIndexAsync(file);
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
                            var fileLogs = await ReadLogFileForIndexAsync(file);
                            logs.AddRange(fileLogs);
                        }
                    }
                }
            }

            return logs.OrderByDescending(log => log.Timestamp).ToList();
        }

        // Đọc log từ một file (Async index)
        private async Task<List<NotificationEntry>> ReadLogFileForIndexAsync(string filePath)
        {
            var logs = new List<NotificationEntry>();
            var lines = await System.IO.File.ReadAllLinesAsync(filePath); // Đọc file async
            foreach (var line in lines)
            {
                var log = await ParseLogLineIndex(line);
                if (log != null)
                {
                    logs.Add(log);
                }
            }
            return logs;
        }

        // Đọc log từ một file (Async MyNotifications)
        private async Task<List<NotificationEntry>> ReadLogFileForMyAsync(string filePath)
        {
            var logs = new List<NotificationEntry>();
            var lines = await System.IO.File.ReadAllLinesAsync(filePath); // Đọc file async
            foreach (var line in lines)
            {
                var log = await ParseLogLineNoti(line);
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

        // Phân tích một dòng log cho index
        private async Task<NotificationEntry> ParseLogLineIndex(string line)
        {
            try
            {

                var parts = line.Split(", ");
                var user = await _context.TblUsers
                .Where(u => u.UserId == int.Parse(parts[2]))
                .FirstOrDefaultAsync();
                if (parts.Length < 7) return null;
                return new NotificationEntry
                {
                    Timestamp = DateTime.ParseExact(parts[0], "dd/MM/yyyy HH\\:mm", CultureInfo.InvariantCulture),
                    Status = parts[1],  // Gán giá trị đã chuyển đổi
                    User = user.FullName,
                    Id = parts[3],
                    Title = parts[4],
                    isDelete = parts[5],
                    Content = parts[6]
                };
            }
            catch
            {
                return null;
            }
        }

        // Phân tích 1 dòng log cho MyNotifications
        private async Task<NotificationEntry> ParseLogLineNoti(string line)
        {
            try
            {

                var parts = line.Split(", ");
                if (parts.Length < 7) return null;
                return new NotificationEntry
                {
                    Timestamp = DateTime.ParseExact(parts[0], "dd/MM/yyyy HH\\:mm", CultureInfo.InvariantCulture),
                    Status = parts[1],  // Gán giá trị đã chuyển đổi
                    User = parts[2],
                    Id = parts[3],
                    Title = parts[4],
                    isDelete = parts[5],
                    Content = parts[6]
                };
            }
            catch
            {
                return null;
            }
        }
    }
}