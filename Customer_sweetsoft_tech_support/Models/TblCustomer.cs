using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Customer_sweetsoft_tech_support.Models;

public partial class TblCustomer
{
    public int CustomerId { get; set; }

    [Required(ErrorMessage = "Tên không được để trống.")]
    [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Tên chỉ được chứa chữ cái và khoảng trắng.")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Tên phải từ 3 đến 50 ký tự.")]
    public string FullName { get; set; } = null!;

    [Required(ErrorMessage = "Email không được để trống.")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Số điện thoại không được để trống.")]
    [RegularExpression(@"^\d{10}$", ErrorMessage = "Số điện thoại phải chứa 10 chữ số.")]
    public string Phone { get; set; } = null!;

    [Required(ErrorMessage = "Mã số thuế không được để trống.")]
    [RegularExpression(@"^\d{10}(-\d{3})?$", ErrorMessage = "Mã số thuế phải gồm 10 chữ số hoặc 13 chữ số (định dạng 0123456789 hoặc 0123456789-001).")]
    public string? TaxCode { get; set; }

    public string? Company { get; set; }

    [Required(ErrorMessage = "Tên đăng nhập không được để trống.")]
    [RegularExpression(@"^[a-zA-Z0-9_]{5,20}$", ErrorMessage = "Tên đăng nhập phải từ 5 đến 20 ký tự, chỉ chứa chữ cái, số và dấu gạch dưới.")]
    public string Username { get; set; } = null!;

    [Required(ErrorMessage = "Mật khẩu không được để trống.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Mật khẩu phải có ít nhất 8 ký tự.")]
    public string Password { get; set; } = null!;

    public short Status { get; set; }

    public bool? IsDelete { get; set; }

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
