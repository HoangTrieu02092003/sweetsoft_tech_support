using System;
using System.Collections.Generic;

namespace admin_sweetsoft_tech_support.Models;

public partial class TblSession
{
    public int SessionId { get; set; }

    public int? UserId { get; set; }

    public string SessionToken { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public DateTime? ExpiresAt { get; set; }

    public virtual TblUser? User { get; set; }
}
