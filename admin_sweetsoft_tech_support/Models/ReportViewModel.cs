namespace report.Models
{
    public class ReportViewModel
    {
        public string CustomerName { get; set; }  // Thêm CustomerName
        public int RequestId { get; set; }
        public string RequestDetails { get; set; } // Chi tiết yêu cầu
        public string DepartmentName { get; set; } // Tên phòng ban gửi yêu cầu
        public string Status { get; set; } // Trạng thái yêu cầu
        public DateTime CreatedAt { get; set; } // Ngày gửi yêu cầu
    }

}
