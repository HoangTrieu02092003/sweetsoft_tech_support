using System;
using System.Collections.Generic;

namespace admin_sweetsoft_tech_support.Models;

public partial class TblNotification
{
    public int NotificationId { get; set; }

    public int? UserId { get; set; }

    public string? Message { get; set; }

    public short? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual TblUser? User { get; set; }
}
