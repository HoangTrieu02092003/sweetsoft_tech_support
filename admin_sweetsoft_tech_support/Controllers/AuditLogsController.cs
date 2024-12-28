
using admin_sweetsoft_tech_support.Attributes;
using admin_sweetsoft_tech_support.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using OfficeOpenXml;
using System.Dynamic;
using System.Security.Claims;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace admin_sweetsoft_tech_support.Controllers
{
    public class AuditLogsController : Controller
    {
        private readonly RequestContext _context;
        private readonly AuditLogService _auditLogService;

        public AuditLogsController(RequestContext context, AuditLogService auditLogService)
        {
            _context = context;
            _auditLogService = auditLogService;
        }
        // Danh sách Audit Logs
        public async Task<IActionResult> Index(string tableName, string actionType, int page = 1, int filePage = 1)
        {
            var auditLogs = _context.TblAuditLogs.AsQueryable();

            // Lọc theo table_name
            if (!string.IsNullOrEmpty(tableName))
                auditLogs = auditLogs.Where(log => log.TableName.Contains(tableName));

            // Lọc theo action_type
            if (!string.IsNullOrEmpty(actionType))
                auditLogs = auditLogs.Where(log => log.ActionType == actionType);

            var logsFromDb = await auditLogs.Skip((page - 1) * 10).Take(10).ToListAsync();
            var logsFromFile = await GetLogsFromFile();
            var paginatedFileLogs = logsFromFile
               .Skip((filePage - 1) * 10)
               .Take(10)
               .ToList();
            var logs = logsFromDb.Select(log =>
            {
                dynamic logItem = new ExpandoObject();
                logItem.AuditId = log.AuditId;
                logItem.TableName = log.TableName;
                logItem.RecordId = log.RecordId;
                logItem.ActionType = log.ActionType;
                logItem.OldValue = log.OldValue;
                logItem.NewValue = log.NewValue;
                logItem.ChangedBy = log.ChangedBy;
                logItem.ChangedAt = log.ChangedAt;
                logItem.ChangedByNavigation = log.ChangedByNavigation;
                return logItem;
            }).ToList();

            var totalUsers = await auditLogs.CountAsync();

            // Tính tổng số trang
            var totalPages = (int)Math.Ceiling((double)totalUsers / 10);
            var FileTotalPages = (int)Math.Ceiling((double)logsFromFile.Count / 10);

            ViewData["DbPagination"] = new Pagination { CurrentPage = page, TotalPages = totalPages };
            ViewData["FilePagination"] = new Pagination { CurrentPage = filePage, TotalPages = FileTotalPages };
            return View(new Tuple<List<dynamic>, List<dynamic>>(logs, paginatedFileLogs));
        }

        private async Task<List<dynamic>> GetLogsFromFile()
        {
            var logs = new List<dynamic>();
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Logs", "auditApplication.log");

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

                        var tableName = logDetails.FirstOrDefault(detail => detail.StartsWith("TableName"))?.Split('=')[1].Trim();
                        var recordId = logDetails.FirstOrDefault(detail => detail.StartsWith("RecordId"))?.Split('=')[1].Trim();
                        var actionType = logDetails.FirstOrDefault(detail => detail.StartsWith("ActionType"))?.Split('=')[1].Trim();
                        var oldValue = logDetails.FirstOrDefault(detail => detail.StartsWith("OldValue"))?.Split('=')[1].Trim();
                        var newValue = logDetails.FirstOrDefault(detail => detail.StartsWith("NewValue"))?.Split('=')[1].Trim();
                        var changedBy = logDetails.FirstOrDefault(detail => detail.StartsWith("ChangedBy"))?.Split('=')[1].Trim();
                        var changedByUser = await _context.TblUsers.FindAsync(int.Parse(changedBy));
                        var changedByNavigation = changedByUser?.FullName;
                        logs.Add(new
                        {
                            TableName = tableName,
                            RecordId = recordId,
                            ActionType = actionType,
                            OldValue = oldValue,
                            NewValue = newValue,
                            ChangedBy = changedBy,
                            ChangedByNavigation = new { FullName = changedByNavigation },
                            ChangedAt = DateTime.Parse(dateTime),
                        });
                    }
                }
            }

            return logs;
        }

        // Chi tiết Audit Log
        public async Task<IActionResult> Details(int id)
        {
            var auditLog = await _context.TblAuditLogs
                .Where(a => a.AuditId == id) // Sử dụng Where thay vì FindAsync nếu bạn muốn thêm điều kiện
                .Include(a => a.ChangedByNavigation) // Bao gồm thông tin liên quan
                .FirstOrDefaultAsync();
            if (auditLog == null)
            {
                return NotFound();
            }
            return View(auditLog);
        }

        // Xuất file
        public async Task<IActionResult> ExportToExcel()
        {
            var auditLogs = await _context.TblAuditLogs.ToListAsync();
            var auditLogFile = await GetLogsFromFile();

            var allLogs = auditLogs.Concat(auditLogFile).ToList();
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("AuditLogs");
                worksheet.Cells[1, 1].Value = "Table Name";
                worksheet.Cells[1, 2].Value = "Record ID";
                worksheet.Cells[1, 3].Value = "Action Type";
                worksheet.Cells[1, 4].Value = "Changed By";
                worksheet.Cells[1, 5].Value = "Changed At";
                worksheet.Cells[1, 6].Value = "Old Value";
                worksheet.Cells[1, 7].Value = "New Value";

                for (int i = 0; i < auditLogs.Count; i++)
                {
                    var log = auditLogs[i];
                    worksheet.Cells[i + 2, 1].Value = log.TableName;
                    worksheet.Cells[i + 2, 2].Value = log.RecordId;
                    worksheet.Cells[i + 2, 3].Value = log.ActionType;
                    worksheet.Cells[i + 2, 4].Value = log.ChangedBy;
                    worksheet.Cells[i + 2, 5].Value = log.ChangedAt?.ToString("yyyy-MM-dd HH:mm");
                    worksheet.Cells[i + 2, 6].Value = log.OldValue;
                    worksheet.Cells[i + 8, 7].Value = log.NewValue;
                }
                var stream = new MemoryStream(package.GetAsByteArray());
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "AuditLogs.xlsx");
            }

        }
    }
}
