using System;
using System.Collections.Generic;

namespace admin_sweetsoft_tech_support.Models;

public partial class TblCustomer
{
    public int CustomerId { get; set; }

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string? TaxCode { get; set; }

    public string? Company { get; set; }

    public string Product { get; set; } = null!;

    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    public short Status { get; set; }

    public string? ResetToken { get; set; }

    public DateTime? ResetTokenExpiry { get; set; }

    public string? Token { get; set; }

    public DateTime? TokenExpiry { get; set; }

    public int? CreatedUser { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int? UpdatedUser { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual TblUser? CreatedUserNavigation { get; set; }

    public virtual ICollection<TblSupportRequest> TblSupportRequests { get; set; } = new List<TblSupportRequest>();

    public virtual TblUser? UpdatedUserNavigation { get; set; }
}
