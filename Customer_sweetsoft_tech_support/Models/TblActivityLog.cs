using System;
using System.Collections.Generic;

namespace Customer_sweetsoft_tech_support.Models;

public partial class TblActivityLog
{
    public int ActivityId { get; set; }

    public int? RequestId { get; set; }

    public int? UserId { get; set; }

    public string? Action { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual TblSupportRequest? Request { get; set; }

    public virtual TblUser? User { get; set; }
}
