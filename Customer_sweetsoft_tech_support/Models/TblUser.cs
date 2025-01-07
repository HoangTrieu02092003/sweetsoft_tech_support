using System;
using System.Collections.Generic;

namespace Customer_sweetsoft_tech_support.Models;

public partial class TblUser
{
    public int UserId { get; set; }

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    public int? RoleId { get; set; }

    public int? DepartmentId { get; set; }

    public short Status { get; set; }

    public bool? IsAdmin { get; set; }

    public string? ResetToken { get; set; }

    public DateTime? ResetTokenExpiry { get; set; }

    public DateTime? LastLogin { get; set; }

    public int? FailedLoginAttempts { get; set; }

    public DateTime? LockoutTime { get; set; }

    public int? CreatedUser { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int? UpdatedUser { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual TblUser? CreatedUserNavigation { get; set; }

    public virtual TblDepartment? Department { get; set; }

    public virtual ICollection<TblUser> InverseCreatedUserNavigation { get; set; } = new List<TblUser>();

    public virtual ICollection<TblUser> InverseUpdatedUserNavigation { get; set; } = new List<TblUser>();

    public virtual TblRole? Role { get; set; }

    public virtual ICollection<TblCustomer> TblCustomerCreatedByNavigations { get; set; } = new List<TblCustomer>();

    public virtual ICollection<TblCustomer> TblCustomerUpdatedByNavigations { get; set; } = new List<TblCustomer>();
    public virtual TblUser? UpdatedUserNavigation { get; set; }
}
