namespace Expense_Tracker_App.Models.ViewModels
{
    public class ReportViewModel
    {
        // Thêm các thuộc tính báo cáo ở đây sau này
        public string Placeholder { get; set; } = "Chưa có dữ liệu báo cáo. Hãy bổ sung sau!";

        public List<string> WeeklyLabels { get; set; } = new(); // Tuần
        public List<int> WeeklyUserCounts { get; set; } = new(); // Số lượng người dùng theo tuần
        public List<string> MonthlyLabels { get; set; } = new(); // Tháng
        public List<decimal> MonthlyTotals { get; set; } = new(); // Tổng chi tiêu từng tháng
        public List<int> MonthlyUserCounts { get; set; } = new(); // Số lượng người dùng theo tháng
        public List<string> YearlyLabels { get; set; } = new(); // Năm
        public List<int> YearlyUserCounts { get; set; } = new(); // Số lượng người dùng theo năm
        public List<string> CategoryLabels { get; set; } = new(); // Tên danh mục
        public List<decimal> CategoryTotals { get; set; } = new(); // Tổng chi tiêu từng danh mục

        public List<string> FilterLabels { get; set; } = new(); // Nhãn cho filter (ngày/tháng/năm)
        public List<int> TransactionCounts { get; set; } = new(); // Số lượng giao dịch theo filter
        public List<int> ActiveUserCounts { get; set; } = new(); // Số lượng người dùng hoạt động theo filter
        public string FilterType { get; set; } = "month"; // Loại filter: day/month/year
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
