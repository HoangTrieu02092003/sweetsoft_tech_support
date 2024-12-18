namespace report.Models
{
    public class RequestDetail
    {
        public int RequestId { get; set; }          // Mã yêu cầu
        public string RequestDetails { get; set; }  // Chi tiết yêu cầu
        public string Status { get; set; }          // Trạng thái yêu cầu ("Completed" hoặc "Pending")
        public DateTime CreatedAt { get; set; }     // Thời gian tạo yêu cầu
    }
}
