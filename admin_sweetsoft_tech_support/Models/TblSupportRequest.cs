using System;
using System.Collections.Generic;

namespace admin_sweetsoft_tech_support.Models;

public partial class TblSupportRequest
{
    public int RequestId { get; set; }

    public int? CustomerId { get; set; }

    public int? DepartmentId { get; set; }

    public string RequestDetails { get; set; } = null!;

    public short Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ResolvedAt { get; set; }

    public virtual TblCustomer? Customer { get; set; }

    public virtual TblDepartment? Department { get; set; }

    public virtual ICollection<TblRequestTransfer> TblRequestTransfers { get; set; } = new List<TblRequestTransfer>();

    public virtual ICollection<TblRequestsProcessing> TblRequestsProcessings { get; set; } = new List<TblRequestsProcessing>();
}
