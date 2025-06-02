using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Expense_Tracker_App.Models;
using Expense_Tracker_App.Models.ViewModels;

namespace Expense_Tracker_App.Service
{
    public interface IAdminService
    {
        Task<AdminDashboardSummaryDto> GetDashboardSummaryAsync();
        Task<List<UserRoleViewModel>> GetUsersAsync(string searchTerm, string? role, int page, int pageSize);
        Task<CategoryPageDto> GetCategoriesAsync(string searchTerm, int page, int pageSize);
        Task<TransactionPageDto> GetTransactionsAsync(string searchTerm, int? categoryId, DateTime? startDate, DateTime? endDate, decimal? minAmount, decimal? maxAmount, int page, int pageSize);
        Task<BudgetPageDto> GetBudgetsAsync(string searchTerm, int? categoryId, string? userId, DateTime? startDate, DateTime? endDate, decimal? minAmount, decimal? maxAmount, int page, int pageSize);
        Task<ReportViewModel> GetReportAsync(string filterType, DateTime? startDate, DateTime? endDate);
    }

    public class AdminDashboardSummaryDto
    {
        public int TotalUsers { get; set; }
        public int TotalTransactions { get; set; }
        public int TotalCategories { get; set; }
        public int TotalBudgets { get; set; }
        public List<ApplicationUser> RecentUsers { get; set; }
        public List<Transaction> RecentTransactions { get; set; }
    }
    public class CategoryPageDto
    {
        public List<Category> Categories { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
    }
    public class TransactionPageDto
    {
        public List<Transaction> Transactions { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public List<Category> Categories { get; set; }
        public List<ApplicationUser> Users { get; set; }
    }
    public class BudgetPageDto
    {
        public List<Budget> Budgets { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public List<Category> Categories { get; set; }
        public List<ApplicationUser> Users { get; set; }
    }
}
