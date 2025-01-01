using System;
using System.Collections.Generic;

namespace Customer_sweetsoft_tech_support.Models;

public partial class TblAuditLog
{
    public int AuditId { get; set; }

    public string? TableName { get; set; }

    public int? RecordId { get; set; }

    public string? ActionType { get; set; }

    public string? OldValue { get; set; }

    public string? NewValue { get; set; }

    public int? ChangedBy { get; set; }

    public DateTime? ChangedAt { get; set; }

    public virtual TblUser? ChangedByNavigation { get; set; }
}
