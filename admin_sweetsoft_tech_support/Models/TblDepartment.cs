using System;
using System.Collections.Generic;

namespace admin_sweetsoft_tech_support.Models;

public partial class TblDepartment
{
    public int DepartmentId { get; set; }

    public string DepartmentName { get; set; } = null!;

    public short Status { get; set; }

    public virtual ICollection<TblRequestTransfer> TblRequestTransferFromDepartments { get; set; } = new List<TblRequestTransfer>();

    public virtual ICollection<TblRequestTransfer> TblRequestTransferToDepartments { get; set; } = new List<TblRequestTransfer>();

    public virtual ICollection<TblRequestsProcessing> TblRequestsProcessings { get; set; } = new List<TblRequestsProcessing>();

    public virtual ICollection<TblSupportRequest> TblSupportRequests { get; set; } = new List<TblSupportRequest>();

    public virtual ICollection<TblUser> TblUsers { get; set; } = new List<TblUser>();
}
