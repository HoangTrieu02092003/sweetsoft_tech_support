namespace report.Models
{
    public class ReportFilterModel
    {
        public int? CustomerId { get; set; } // Lọc theo khách hàng
        public int? DepartmentId { get; set; } // Lọc theo phòng ban
        public DateTime? StartDate { get; set; } // Lọc theo ngày bắt đầu
        public DateTime? EndDate { get; set; } // Lọc theo ngày kết thúc
        public int? Status { get; set; } // Lọc theo trạng thái

    }

}
