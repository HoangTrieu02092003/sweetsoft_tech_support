using System;
using System.Collections.Generic;

namespace admin_sweetsoft_tech_support.Models;

public partial class TblRequestsProcessing
{
    public int ProcessId { get; set; }

    public int? RequestId { get; set; }

    public int? DepartmentId { get; set; }

    public short? IsCompleted { get; set; }

    public DateTime? ProcessedAt { get; set; }

    public string? Note { get; set; }

    public virtual TblDepartment? Department { get; set; }

    public virtual TblSupportRequest? Request { get; set; }
}
