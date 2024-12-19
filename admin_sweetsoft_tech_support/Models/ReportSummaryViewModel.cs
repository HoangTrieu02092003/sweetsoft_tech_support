namespace report.Models
{

   
       
        public class ReportSummaryViewModel
        {
            public int CustomerId { get; set; }
            public string CustomerName { get; set; }
            public int TotalRequests { get; set; }
            public List<RequestDetail> Detail { get; set; } // Danh sách chi tiết các yêu cầu
        }

    }



