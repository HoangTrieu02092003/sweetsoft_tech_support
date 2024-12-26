
using admin_sweetsoft_tech_support.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using OfficeOpenXml;
using System.Security.Claims;

namespace admin_sweetsoft_tech_support.Controllers
{
    public class AuditLogsController : Controller
    {
        private readonly RequestContext _context;

        public AuditLogsController(RequestContext context)
        {
            _context = context;
        }
        // Danh sách Audit Logs
        public async Task<IActionResult> Index(string tableName, string actionType, int page = 1)
        {
            var auditLogs = _context.TblAuditLogs.AsQueryable();

            // Lọc theo table_name
            if (!string.IsNullOrEmpty(tableName))
                auditLogs = auditLogs.Where(log => log.TableName.Contains(tableName));

            // Lọc theo action_type
            if (!string.IsNullOrEmpty(actionType))
                auditLogs = auditLogs.Where(log => log.ActionType == actionType);

            var pageSize = 6; // số lượng người dùng mỗi trang
            var skip = (page - 1) * pageSize;
            
            var requestContext = auditLogs
                .Include(l => l.ChangedByNavigation)
                .OrderByDescending(a => a.ChangedAt)
                .Skip(skip) // bỏ qua dữ liệu đã xem ở các trang trước
                .Take(pageSize);

            var totalUsers = await auditLogs.CountAsync();

            // Tính tổng số trang
            var totalPages = (int)Math.Ceiling(totalUsers / (double)pageSize);
            
            ViewData["TotalPages"] = totalPages;
            ViewData["CurrentPage"] = page;
            return View(await requestContext.ToListAsync());
        }

        // Chi tiết Audit Log
        public async Task<IActionResult> Details(int id)
        {
            var auditLog = await _context.TblAuditLogs.FindAsync(id);
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

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("AuditLogs");
                worksheet.Cells[1, 1].Value = "ID";
                worksheet.Cells[1, 2].Value = "Table Name";
                worksheet.Cells[1, 3].Value = "Record ID";
                worksheet.Cells[1, 4].Value = "Action Type";
                worksheet.Cells[1, 5].Value = "Changed By";
                worksheet.Cells[1, 6].Value = "Changed At";
                worksheet.Cells[1, 7].Value = "Old Value";
                worksheet.Cells[1, 8].Value = "New Value";

                for (int i = 0; i < auditLogs.Count; i++)
                {
                    var log = auditLogs[i];
                    worksheet.Cells[i + 2, 1].Value = log.AuditId;
                    worksheet.Cells[i + 2, 2].Value = log.TableName;
                    worksheet.Cells[i + 2, 3].Value = log.RecordId;
                    worksheet.Cells[i + 2, 4].Value = log.ActionType;
                    worksheet.Cells[i + 2, 5].Value = log.ChangedBy;
                    worksheet.Cells[i + 2, 6].Value = log.ChangedAt?.ToString("yyyy-MM-dd HH:mm");
                    worksheet.Cells[i + 2, 7].Value = log.OldValue;
                    worksheet.Cells[i + 8, 8].Value = log.NewValue;
                }
                var stream = new MemoryStream(package.GetAsByteArray());
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "AuditLogs.xlsx");
            }

        }
    }
}
