using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Customer_sweetsoft_tech_support.Models;

public partial class TblCustomer
{
    public int CustomerId { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập họ và tên.")]
    public string FullName { get; set; } = null!;

    [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
    [Required(ErrorMessage = "Vui lòng nhập email.")]
    public string Email { get; set; } = null!;

    [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
    [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
    public string Phone { get; set; } = null!;

    [RegularExpression("^[0-9]{10}$", ErrorMessage = "Mã số thuế phải là 10 chữ số.")]
    public string? TaxCode { get; set; }

    public string? Company { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập.")]
    public string Username { get; set; } = null!;

    [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
    public string Password { get; set; } = null!;

    public short Status { get; set; }

    public bool IsDelete { get; set; }

    public string? ResetToken { get; set; }

    public DateTime? ResetTokenExpiry { get; set; }

    public string? Token { get; set; }

    public DateTime? TokenExpiry { get; set; }

    public int? CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual TblUser? CreatedByNavigation { get; set; }

    public virtual ICollection<TblSupportRequest> TblSupportRequests { get; set; } = new List<TblSupportRequest>();

    public virtual TblUser? UpdatedByNavigation { get; set; }
}
