using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Customer_sweetsoft_tech_support.Models;

public partial class TblCustomer
{
    public int CustomerId { get; set; }

    [Required(ErrorMessage = "Họ và tên không được để trống.")]
    [StringLength(100, ErrorMessage = "Họ và tên không được vượt quá 100 ký tự.")]
    public string FullName { get; set; } = null!;

    [Required(ErrorMessage = "Email không được để trống.")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Số điện thoại không được để trống.")]
    [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
    public string Phone { get; set; } = null!;

    public string? TaxCode { get; set; }

    public string? Company { get; set; }

    [Required(ErrorMessage = "Tên đăng nhập không được để trống.")]
    [StringLength(50, ErrorMessage = "Tên đăng nhập không được vượt quá 50 ký tự.")]
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
