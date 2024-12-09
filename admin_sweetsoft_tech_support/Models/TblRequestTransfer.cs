using System;
using System.Collections.Generic;

namespace admin_sweetsoft_tech_support.Models;

public partial class TblRequestTransfer
{
    public int TransferId { get; set; }

    public int? RequestId { get; set; }

    public int? FromDepartmentId { get; set; }

    public int? ToDepartmentId { get; set; }

    public short? Priority { get; set; }

    public int? TransferredBy { get; set; }

    public DateTime TransferredAt { get; set; }

    public string? Note { get; set; }

    public virtual TblDepartment? FromDepartment { get; set; }

    public virtual TblSupportRequest? Request { get; set; }

    public virtual TblDepartment? ToDepartment { get; set; }

    public virtual TblUser? TransferredByNavigation { get; set; }
}
