using admin_sweetsoft_tech_support.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch.Internal;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Security.Claims;
using System.Threading.Tasks;

namespace admin_sweetsoft_tech_support.Controllers
{
    public class ActivityLogsController : Controller
    {
        private readonly string _logDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Activitys");

        // Hiển thị tất cả log
        public async Task<IActionResult> Index(string? date = "", string? searchTerm = null, string filterOption = "", int page = 1)
        {
            var currentUserIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserIdString) || !int.TryParse(currentUserIdString, out int currentUserId))
            {
                TempData["ReturnUrl"] = Request.Path.ToString();
                return RedirectToAction("Login", "Admin");
            }
            List<ActivityLogEntry> logs;
            var pageSize = 5; // số lượng log mỗi trang
            var skip = (page - 1) * pageSize;
            var totalLogs = await CountLogsAsync(); // Đếm tổng số log async

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
                logs = logs.Where(log =>
                    log.User.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    log.Action.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    log.Title?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) == true)
                    .ToList();
            }

            var totalPages = (int)Math.Ceiling(totalLogs / (double)pageSize);
            ViewData["SearchTerm"] = searchTerm;
            ViewData["Date"] = date;
            ViewData["FilterOption"] = filterOption;
            ViewData["TotalPages"] = totalPages;
            ViewData["CurrentPage"] = page;

            return View(Tuple.Create(logs, new List<object>()));
        }

        private async Task<List<ActivityLogEntry>> ReadLogsForPaginationAsync(int skip, int pageSize)
        {
            var logs = new List<ActivityLogEntry>();
            if (!Directory.Exists(_logDirectory))
            {
                return logs;
            }

            var logFiles = Directory.GetFiles(_logDirectory, "*.log", SearchOption.AllDirectories);
            foreach (var file in logFiles)
            {
                var fileLogs = await ReadLogFileAsync(file);
                logs.AddRange(fileLogs);
            }

            return logs.OrderByDescending(log => log.Timestamp)
                        .Skip(skip)
                        .Take(pageSize)
                        .ToList();
        }

        private async Task<List<ActivityLogEntry>> ReadLogsForDatePaginationAsync(DateTime date, int skip, int pageSize)
        {
            var logs = new List<ActivityLogEntry>();
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

            return logs.OrderByDescending(log => log.Timestamp)
                        .Skip(skip)
                        .Take(pageSize)
                        .ToList();
        }

        private async Task<List<ActivityLogEntry>> ReadLogsByDateAsync(DateTime date)
        {
            var logs = new List<ActivityLogEntry>();
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

        private async Task<List<ActivityLogEntry>> ReadLogsForMonthPaginationAsync(DateTime month, int skip, int pageSize)
        {
            var logs = new List<ActivityLogEntry>();
            var logDirectoryForMonth = Path.Combine(_logDirectory, month.ToString("yyyy"), month.ToString("MM"));

            if (Directory.Exists(logDirectoryForMonth))
            {
                var dayDirectories = Directory.GetDirectories(logDirectoryForMonth);

                foreach (var dayDirectory in dayDirectories)
                {
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

        private async Task<List<ActivityLogEntry>> ReadLogsByMonthAsync(DateTime month)
        {
            var logs = new List<ActivityLogEntry>();
            var logDirectoryForMonth = Path.Combine(_logDirectory, month.ToString("yyyy"), month.ToString("MM"));

            if (Directory.Exists(logDirectoryForMonth))
            {
                var dayDirectories = Directory.GetDirectories(logDirectoryForMonth);

                foreach (var dayDirectory in dayDirectories)
                {
                    var logFiles = Directory.GetFiles(dayDirectory, $"{month:yyyy-MM}-{Path.GetFileName(dayDirectory)}-*.log");

                    foreach (var file in logFiles)
                    {
                        var fileLogs = await ReadLogFileAsync(file);
                        logs.AddRange(fileLogs);
                    }
                }
            }

            return logs.OrderByDescending(log => log.Timestamp).ToList();
        }

        private async Task<List<ActivityLogEntry>> ReadLogsForYearPaginationAsync(DateTime year, int skip, int pageSize)
        {
            var logs = new List<ActivityLogEntry>();
            var logDirectoryForYear = Path.Combine(_logDirectory, year.ToString("yyyy"));

            if (Directory.Exists(logDirectoryForYear))
            {
                var monthDirectories = Directory.GetDirectories(logDirectoryForYear);

                foreach (var monthDirectory in monthDirectories)
                {
                    var dayDirectories = Directory.GetDirectories(monthDirectory);

                    foreach (var dayDirectory in dayDirectories)
                    {
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

        private async Task<List<ActivityLogEntry>> ReadLogsByYearAsync(DateTime year)
        {
            var logs = new List<ActivityLogEntry>();
            var logDirectoryForYear = Path.Combine(_logDirectory, year.ToString("yyyy"));

            if (Directory.Exists(logDirectoryForYear))
            {
                var monthDirectories = Directory.GetDirectories(logDirectoryForYear);

                foreach (var monthDirectory in monthDirectories)
                {
                    var dayDirectories = Directory.GetDirectories(monthDirectory);

                    foreach (var dayDirectory in dayDirectories)
                    {
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

        private async Task<List<ActivityLogEntry>> ReadLogFileAsync(string filePath)
        {
            var logs = new List<ActivityLogEntry>();
            var lines = await System.IO.File.ReadAllLinesAsync(filePath);
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

        private async Task<int> CountLogsAsync()
        {
            var totalLogs = 0;
            if (Directory.Exists(_logDirectory))
            {
                var logFiles = Directory.GetFiles(_logDirectory, "*.log", SearchOption.AllDirectories);
                foreach (var file in logFiles)
                {
                    totalLogs += await CountLinesInFileAsync(file);
                }
            }
            return totalLogs;
        }

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

        private ActivityLogEntry? ParseLogLine(string line)
        {
            try
            {
                var parts = line.Split(", ");
                if (parts.Length < 4) return new ActivityLogEntry();

                return new ActivityLogEntry
                {
                    Timestamp = DateTime.ParseExact(parts[0], "dd/MM/yyyy HH\\:mm", CultureInfo.InvariantCulture),
                    Title = parts[1],
                    Action = parts[2],
                    User = parts[3]
                };
            }
            catch
            {
                return new ActivityLogEntry();
            }
        }
    }
}
