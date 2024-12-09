using System;
using System.Collections.Generic;

namespace admin_sweetsoft_tech_support.Models;

public partial class TblPermission
{
    public int PermissionId { get; set; }

    public string PermissionName { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<TblUserPermission> TblUserPermissions { get; set; } = new List<TblUserPermission>();
}
