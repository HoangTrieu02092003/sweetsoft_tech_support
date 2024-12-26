using admin_sweetsoft_tech_support.Models;

namespace admin_sweetsoft_tech_support.Attributes
{
    public class AuditLogService
    {
        private readonly RequestContext _context;

        public AuditLogService(RequestContext context)
        {
            _context = context;
        }

        public async Task LogAuditAction(string tableName, int recordId, string actionType, int? changedBy, string oldValue, string newValue)
        {
            var auditLog = new TblAuditLog
            {
                TableName = tableName,
                RecordId = recordId,
                ActionType = actionType,
                ChangedBy = changedBy,
                ChangedAt = DateTime.Now,
                OldValue = oldValue,
                NewValue = newValue
            };

            _context.TblAuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
        }
    }
}
