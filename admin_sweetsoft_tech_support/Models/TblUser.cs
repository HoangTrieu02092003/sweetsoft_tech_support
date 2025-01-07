using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace admin_sweetsoft_tech_support.Models;

public partial class TblUser
{
    public int UserId { get; set; }

    [Required(ErrorMessage = "Tên không được để trống.")]
    [RegularExpression(@"^[\p{L} ]+$", ErrorMessage = "Tên chỉ được chứa chữ cái và khoảng trắng.")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Tên phải từ 3 đến 50 ký tự.")]
    public string FullName { get; set; } = null!;

    [Required(ErrorMessage = "Email không được để trống.")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Số điện thoại không được để trống.")]
    [RegularExpression(@"^\d{10}$", ErrorMessage = "Số điện thoại phải chứa 10 chữ số.")]
    public string Phone { get; set; } = null!;

    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    public int? RoleId { get; set; }

    public int? DepartmentId { get; set; }

    public short Status { get; set; }

    public bool? IsAdmin { get; set; }

    public bool? IsDelete { get; set; }

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

    public virtual ICollection<TblRequestFeedback> TblRequestFeedbackFromUsers { get; set; } = new List<TblRequestFeedback>();

    public virtual ICollection<TblRequestFeedback> TblRequestFeedbackToUsers { get; set; } = new List<TblRequestFeedback>();

    public virtual ICollection<TblRequestTransfer> TblRequestTransferTransferredByNavigations { get; set; } = new List<TblRequestTransfer>();

    public virtual ICollection<TblRequestTransfer> TblRequestTransferTransferredHandleNavigations { get; set; } = new List<TblRequestTransfer>();

    public virtual ICollection<TblSession> TblSessions { get; set; } = new List<TblSession>();

    public virtual ICollection<TblUserPermission> TblUserPermissions { get; set; } = new List<TblUserPermission>();

    public virtual TblUser? UpdatedUserNavigation { get; set; }

    public object ToLogData()
    {
        return new
        {
            this.UserId,
            this.FullName,
            this.Email,
            this.Phone,
            this.Username,
            this.RoleId,
            this.DepartmentId,
            this.Status,
            this.IsAdmin,
            this.CreatedUser,
            this.CreatedAt,
            this.UpdatedUser,
            this.UpdatedAt
        };
    }
}
