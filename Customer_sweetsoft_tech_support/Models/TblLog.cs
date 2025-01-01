using System;
using System.Collections.Generic;

namespace Customer_sweetsoft_tech_support.Models;

public partial class TblLog
{
    public int LogId { get; set; }

    public int? UserId { get; set; }

    public string? Action { get; set; }

    public string? Description { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual TblUser? User { get; set; }
}
