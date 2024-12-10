using System;
using System.Collections.Generic;

namespace admin_sweetsoft_tech_support.Models;

public partial class TblUserPermission
{
    public int UserPermissionId { get; set; }

    public int? UserId { get; set; }

    public int? PermissionId { get; set; }

    public virtual TblPermission? Permission { get; set; }

    public virtual TblUser? User { get; set; }
}
