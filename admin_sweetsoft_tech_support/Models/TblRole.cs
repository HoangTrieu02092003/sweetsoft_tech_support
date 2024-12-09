using System;
using System.Collections.Generic;

namespace admin_sweetsoft_tech_support.Models;

public partial class TblRole
{
    public int RoleId { get; set; }

    public string RoleName { get; set; } = null!;

    public virtual ICollection<TblUser> TblUsers { get; set; } = new List<TblUser>();
}
