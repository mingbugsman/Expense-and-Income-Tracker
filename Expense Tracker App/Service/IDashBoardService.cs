using System.Threading.Tasks;
using System.Collections.Generic;
using Expense_Tracker_App.Models;

namespace Expense_Tracker_App.Service
{
    public interface IDashBoardService
    {
        Task<DashBoardSummaryDto> GetDashBoardSummaryAsync(string userId);
    }

    public class DashBoardSummaryDto
    {
        public decimal TotalIncome { get; set; }
        public decimal TotalExpense { get; set; }
        public string Balance { get; set; }
        public List<object> DoughnutChartData { get; set; }
        public List<object> SplineChartData { get; set; }
        public List<Transaction> RecentTransactions { get; set; }
    }
}
