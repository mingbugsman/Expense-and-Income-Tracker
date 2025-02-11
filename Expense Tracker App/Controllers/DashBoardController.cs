using Expense_Tracker_App.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Expense_Tracker_App.Models;
using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace Expense_Tracker_App.Controllers
{
    [Authorize]
    public class DashBoardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public class SpineChartData
        {
            public string Day { get; set; }
            public decimal Income { get; set; }
            public decimal Expense { get; set; }
        }

        public DashBoardController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private string GetUserId() => _userManager.GetUserId(User);

        public async Task<IActionResult> Index()
        {
            string userId = GetUserId();
            DateTime StartDate = DateTime.Today.AddDays(-6); // 7 ngày trước
            DateTime EndDate = DateTime.Today; // Ngày hôm nay

            // Lấy giao dịch của User trong 7 ngày qua
            List<Transaction> SelectedTransactions = await _context.Transactions
                .Include(t => t.Category)
                .Where(t => t.UserId == userId && t.TransactionDate >= StartDate && t.TransactionDate <= EndDate)
                .ToListAsync();

            // Tổng thu nhập
            decimal TotalIncome = SelectedTransactions
                .Where(t => t.Category?.Type == "Income")
                .Sum(t => t.Amount);

            ViewBag.TotalIncome = TotalIncome.ToString("n2");

            // Tổng chi tiêu
            decimal TotalExpense = SelectedTransactions
                .Where(t => t.Category?.Type == "Expense")
                .Sum(t => t.Amount);

            ViewBag.TotalExpense = TotalExpense.ToString("n2");

            // Số dư (Balance)
            decimal Balance = TotalIncome - TotalExpense;
            CultureInfo culture = new("en-US") { NumberFormat = { CurrencyPositivePattern = 1 } };
            ViewBag.Balance = string.Format(culture, "{0:C}", Balance);

            // 🥧 **Doughnut Chart - Chi tiêu theo danh mục**
            ViewBag.DoughnutChartData = SelectedTransactions
                .Where(t => t.Category != null && t.Category.Type == "Expense")
                .GroupBy(t => t.Category!.CategoryId)
                .Select(g => new
                {
                    title = g.First().Category!.Title,
                    amount = g.Sum(t => t.Amount),
                    formattedAmount = g.Sum(t => t.Amount).ToString("n2")
                })
                .OrderByDescending(g => g.amount)
                .ToList();

            // 📈 **Spine Chart - Thu nhập & Chi tiêu theo ngày**
            List<SpineChartData> IncomeSummary = SelectedTransactions
                .Where(t => t.Category?.Type == "Income")
                .GroupBy(t => t.TransactionDate.Date)
                .Select(g => new SpineChartData { Day = g.Key.ToString("yyyy-MM-dd"), Income = g.Sum(t => t.Amount) })
                .ToList();

            List<SpineChartData> ExpenseSummary = SelectedTransactions
                .Where(t => t.Category?.Type == "Expense")
                .GroupBy(t => t.TransactionDate.Date)
                .Select(g => new SpineChartData { Day = g.Key.ToString("yyyy-MM-dd"), Expense = g.Sum(t => t.Amount) })
                .ToList();

            // Gộp dữ liệu của 7 ngày qua
            string[] last7Days = Enumerable.Range(0, 7)
                .Select(i => DateTime.Today.AddDays(i).ToString("dd-MMM"))
                .ToArray();

            ViewBag.SplineChartData = from day in last7Days
                                      join income in IncomeSummary on day equals income.Day into incomeGroup
                                      from income in incomeGroup.DefaultIfEmpty()
                                      join expense in ExpenseSummary on day equals expense.Day into expenseGroup
                                      from expense in expenseGroup.DefaultIfEmpty()
                                      select new
                                      {
                                          Day = day,
                                          Income = income?.Income ?? 0,
                                          Expense = expense?.Expense ?? 0
                                      };

            // 🆕 **Lấy 5 giao dịch gần nhất**
            ViewBag.RecentTransactions = SelectedTransactions
                .OrderByDescending(t => t.TransactionDate)
                .Take(5)
                .ToList();

            return View();
        }
    }
}
