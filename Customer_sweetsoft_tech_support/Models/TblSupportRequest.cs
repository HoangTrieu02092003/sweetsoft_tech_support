using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Customer_sweetsoft_tech_support.Models;

public partial class TblSupportRequest
{
    public int RequestId { get; set; }

    public int? CustomerId { get; set; }

    public int? DepartmentId { get; set; }
    [RegularExpression(@"^[\p{L} ]+$", ErrorMessage = "Tên chỉ được chứa chữ cái và khoảng trắng.")]
    public string RequestTitle { get; set; } = null!;

    public string Product { get; set; } = null!;

    public string RequestDetails { get; set; } = null!;

    public short Status { get; set; }

    public bool? IsDelete { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ResolvedAt { get; set; }

    public virtual TblCustomer? Customer { get; set; }

    public virtual TblDepartment? Department { get; set; }

    public virtual ICollection<TblRequestsProcessing> TblRequestsProcessings { get; set; } = new List<TblRequestsProcessing>();
}
