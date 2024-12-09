using System;
using System.Collections.Generic;

namespace admin_sweetsoft_tech_support.Models;

public partial class TblFaq
{
    public int FaqId { get; set; }

    public string Question { get; set; } = null!;

    public string Answer { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
