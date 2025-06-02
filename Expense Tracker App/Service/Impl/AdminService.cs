using Expense_Tracker_App.Data;
using Expense_Tracker_App.Models;
using Expense_Tracker_App.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Expense_Tracker_App.Repository;

namespace Expense_Tracker_App.Service.Impl
{
    public class AdminService : IAdminService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAdminRepository _repository;
        public AdminService(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IAdminRepository repository)
        {
            _context = context;
            _userManager = userManager;
            _repository = repository;
        }

        public async Task<AdminDashboardSummaryDto> GetDashboardSummaryAsync()
        {
            var summary = new AdminDashboardSummaryDto();
            summary.TotalUsers = await _repository.GetTotalUsersAsync();
            summary.TotalTransactions = await _repository.GetTotalTransactionsAsync();
            summary.TotalCategories = await _repository.GetTotalCategoriesAsync();
            summary.TotalBudgets = await _repository.GetTotalBudgetsAsync();
            summary.RecentUsers = await _repository.GetRecentUsersAsync(5);
            summary.RecentTransactions = await _repository.GetRecentTransactionsAsync(5);
            return summary;
        }

        public async Task<List<UserRoleViewModel>> GetUsersAsync(string searchTerm, string? role, int page, int pageSize)
        {
            var users = await _repository.GetUsersAsync(searchTerm, (page - 1) * pageSize, pageSize);
            var userRoles = new List<UserRoleViewModel>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (string.IsNullOrEmpty(role) || roles.Contains(role))
                {
                    userRoles.Add(new UserRoleViewModel { User = user, Roles = roles?.ToList() ?? new List<string>() });
                }
            }
            return userRoles;
        }

        public async Task<CategoryPageDto> GetCategoriesAsync(string searchTerm, int page, int pageSize)
        {
            var totalCategories = await _repository.GetCategoriesCountAsync(searchTerm);
            var categories = await _repository.GetCategoriesAsync(searchTerm, (page - 1) * pageSize, pageSize);
            return new CategoryPageDto { Categories = categories, TotalPages = (int)Math.Ceiling(totalCategories / (double)pageSize), CurrentPage = page };
        }

        public async Task<TransactionPageDto> GetTransactionsAsync(string searchTerm, int? categoryId, DateTime? startDate, DateTime? endDate, decimal? minAmount, decimal? maxAmount, int page, int pageSize)
        {
            var totalTransactions = await _repository.GetTransactionsCountAsync(searchTerm, categoryId, startDate, endDate, minAmount, maxAmount);
            var transactions = await _repository.GetTransactionsAsync(searchTerm, categoryId, startDate, endDate, minAmount, maxAmount, (page - 1) * pageSize, pageSize);
            var categories = await _repository.GetAllCategoriesAsync();
            var users = await _repository.GetAllUsersAsync();
            return new TransactionPageDto { Transactions = transactions, TotalPages = (int)Math.Ceiling(totalTransactions / (double)pageSize), CurrentPage = page, Categories = categories, Users = users };
        }

        public async Task<BudgetPageDto> GetBudgetsAsync(string searchTerm, int? categoryId, string? userId, DateTime? startDate, DateTime? endDate, decimal? minAmount, decimal? maxAmount, int page, int pageSize)
        {
            var totalBudgets = await _repository.GetBudgetsCountAsync(searchTerm, categoryId, userId, startDate, endDate, minAmount, maxAmount);
            var budgets = await _repository.GetBudgetsAsync(searchTerm, categoryId, userId, startDate, endDate, minAmount, maxAmount, (page - 1) * pageSize, pageSize);
            var users = await _repository.GetAllUsersAsync();
            var categories = await _repository.GetAllCategoriesAsync();
            return new BudgetPageDto { Budgets = budgets, TotalPages = (int)Math.Ceiling(totalBudgets / (double)pageSize), CurrentPage = page, Users = users, Categories = categories };
        }

        public async Task<ReportViewModel> GetReportAsync(string filterType, DateTime? startDate, DateTime? endDate)
        {
            var now = DateTime.Now;
            startDate ??= filterType == "year" ? new DateTime(now.Year - 4, 1, 1) : filterType == "month" ? new DateTime(now.Year, 1, 1) : now.Date.AddDays(-29);
            endDate ??= now.Date.AddDays(1);
            var labels = new List<string>();
            var transactionCounts = new List<int>();
            var activeUserCounts = new List<int>();
            if (filterType == "day")
            {
                for (var date = startDate.Value.Date; date < endDate.Value.Date; date = date.AddDays(1))
                {
                    labels.Add(date.ToString("dd/MM/yyyy"));
                    var count = await _context.Transactions.CountAsync(t => t.TransactionDate.Date == date);
                    transactionCounts.Add(count);
                    var activeUsers = await _context.Transactions.Where(t => t.TransactionDate.Date == date).Select(t => t.UserId).Distinct().CountAsync();
                    activeUserCounts.Add(activeUsers);
                }
            }
            else if (filterType == "month")
            {
                var start = new DateTime(startDate.Value.Year, startDate.Value.Month, 1);
                var end = new DateTime(endDate.Value.Year, endDate.Value.Month, 1);
                for (var date = start; date <= end; date = date.AddMonths(1))
                {
                    labels.Add(date.ToString("MM/yyyy"));
                    var nextMonth = date.AddMonths(1);
                    var count = await _context.Transactions.CountAsync(t => t.TransactionDate >= date && t.TransactionDate < nextMonth);
                    transactionCounts.Add(count);
                    var activeUsers = await _context.Transactions.Where(t => t.TransactionDate >= date && t.TransactionDate < nextMonth).Select(t => t.UserId).Distinct().CountAsync();
                    activeUserCounts.Add(activeUsers);
                }
            }
            else if (filterType == "year")
            {
                for (var year = startDate.Value.Year; year <= endDate.Value.Year; year++)
                {
                    labels.Add(year.ToString());
                    var yearStart = new DateTime(year, 1, 1);
                    var yearEnd = yearStart.AddYears(1);
                    var count = await _context.Transactions.CountAsync(t => t.TransactionDate >= yearStart && t.TransactionDate < yearEnd);
                    transactionCounts.Add(count);
                    var activeUsers = await _context.Transactions.Where(t => t.TransactionDate >= yearStart && t.TransactionDate < yearEnd).Select(t => t.UserId).Distinct().CountAsync();
                    activeUserCounts.Add(activeUsers);
                }
            }
            return new ReportViewModel
            {
                FilterLabels = labels,
                TransactionCounts = transactionCounts,
                ActiveUserCounts = activeUserCounts,
                FilterType = filterType,
                StartDate = startDate,
                EndDate = endDate
            };
        }
    }
}
