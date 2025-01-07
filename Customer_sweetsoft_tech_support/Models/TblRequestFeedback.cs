using System;
using System.Collections.Generic;

namespace Customer_sweetsoft_tech_support.Models;

public partial class TblRequestFeedback
{
    public int FeedbackId { get; set; }

    public int RequestId { get; set; }

    public int? FromUserId { get; set; }

    public int? FromCustomerId { get; set; }

    public int? ToUserId { get; set; }

    public int? ToCustomerId { get; set; }

    public string Feedback { get; set; } = null!;

    public short FeedbackType { get; set; }

    public bool? IsRead { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual TblCustomer? FromCustomer { get; set; }

    public virtual TblUser? FromUser { get; set; }

    public virtual TblSupportRequest Request { get; set; } = null!;

    public virtual TblCustomer? ToCustomer { get; set; }

    public virtual TblUser? ToUser { get; set; }
}
