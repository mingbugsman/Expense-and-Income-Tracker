using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Expense_Tracker_App.Data;
using Expense_Tracker_App.Models;
using Microsoft.EntityFrameworkCore;
using Expense_Tracker_App.Service;
using Expense_Tracker_App.Repository;

namespace Expense_Tracker_App.Service.Impl
{
    public class DashBoardService : IDashBoardService
    {
        private readonly ApplicationDbContext _context;
        private readonly IDashBoardRepository _repository;
        public DashBoardService(ApplicationDbContext context, IDashBoardRepository repository)
        {
            _context = context;
            _repository = repository;
        }

        public async Task<DashBoardSummaryDto> GetDashBoardSummaryAsync(string userId)
        {
            DateTime StartDate = DateTime.Today.AddDays(-6);
            DateTime EndDate = DateTime.Today.AddDays(1);

            List<Transaction> SelectedTransactions = await _repository.GetUserTransactionsForPeriodAsync(userId, StartDate, EndDate);

            decimal TotalIncome = SelectedTransactions
                .Where(t => t.Category?.Type == "Income")
                .Sum(t => t.Amount);

            decimal TotalExpense = SelectedTransactions
                .Where(t => t.Category?.Type == "Expense")
                .Sum(t => t.Amount);

            decimal Balance = TotalIncome - TotalExpense;
            CultureInfo culture = new("vi-VN");
            string formattedBalance = string.Format(culture, "{0:c0}", Balance);

            var doughnutChartData = SelectedTransactions
                .Where(t => t.Category != null && t.Category.Type == "Expense")
                .GroupBy(t => t.Category!.CategoryId)
                .Select(g => new
                {
                    title = g.First().Category!.Title,
                    amount = g.Sum(t => t.Amount),
                    formattedAmount = g.Sum(t => t.Amount).ToString("n2")
                })
                .OrderByDescending(g => g.amount)
                .ToList<object>();

            var incomeSummary = SelectedTransactions
                .Where(t => t.Category?.Type == "Income")
                .GroupBy(t => t.TransactionDate.Date)
                .Select(g => new { Day = g.Key.ToString("yyyy-MM-dd"), Income = g.Sum(t => t.Amount) })
                .ToList();

            var expenseSummary = SelectedTransactions
                .Where(t => t.Category?.Type == "Expense")
                .GroupBy(t => t.TransactionDate.Date)
                .Select(g => new { Day = g.Key.ToString("yyyy-MM-dd"), Expense = g.Sum(t => t.Amount) })
                .ToList();

            string[] last7Days = Enumerable.Range(0, 7)
                .Select(i => DateTime.Today.AddDays(-i).ToString("yyyy-MM-dd"))
                .Reverse()
                .ToArray();

            var splineChartData = (from day in last7Days
                                   join income in incomeSummary on day equals income.Day into incomeGroup
                                   from income in incomeGroup.DefaultIfEmpty()
                                   join expense in expenseSummary on day equals expense.Day into expenseGroup
                                   from expense in expenseGroup.DefaultIfEmpty()
                                   select new
                                   {
                                       Day = day,
                                       Income = income?.Income ?? 0,
                                       Expense = expense?.Expense ?? 0
                                   }).ToList<object>();

            var recentTransactions = SelectedTransactions
                .OrderByDescending(t => t.TransactionDate)
                .Take(5)
                .ToList();

            return new DashBoardSummaryDto
            {
                TotalIncome = TotalIncome,
                TotalExpense = TotalExpense,
                Balance = formattedBalance,
                DoughnutChartData = doughnutChartData,
                SplineChartData = splineChartData,
                RecentTransactions = recentTransactions
            };
        }
    }
}
