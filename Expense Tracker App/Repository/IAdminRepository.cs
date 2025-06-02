using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Expense_Tracker_App.Models;

namespace Expense_Tracker_App.Repository
{
    public interface IAdminRepository
    {
        Task<int> GetTotalUsersAsync();
        Task<int> GetTotalTransactionsAsync();
        Task<int> GetTotalCategoriesAsync();
        Task<int> GetTotalBudgetsAsync();
        Task<List<ApplicationUser>> GetRecentUsersAsync(int count);
        Task<List<Transaction>> GetRecentTransactionsAsync(int count);
        Task<List<ApplicationUser>> GetUsersAsync(string searchTerm, int skip, int take);
        Task<int> GetUsersCountAsync(string searchTerm);
        Task<List<Category>> GetCategoriesAsync(string searchTerm, int skip, int take);
        Task<int> GetCategoriesCountAsync(string searchTerm);
        Task<List<Transaction>> GetTransactionsAsync(string searchTerm, int? categoryId, DateTime? startDate, DateTime? endDate, decimal? minAmount, decimal? maxAmount, int skip, int take);
        Task<int> GetTransactionsCountAsync(string searchTerm, int? categoryId, DateTime? startDate, DateTime? endDate, decimal? minAmount, decimal? maxAmount);
        Task<List<Category>> GetAllCategoriesAsync();
        Task<List<ApplicationUser>> GetAllUsersAsync();
        Task<List<Budget>> GetBudgetsAsync(string searchTerm, int? categoryId, string? userId, DateTime? startDate, DateTime? endDate, decimal? minAmount, decimal? maxAmount, int skip, int take);
        Task<int> GetBudgetsCountAsync(string searchTerm, int? categoryId, string? userId, DateTime? startDate, DateTime? endDate, decimal? minAmount, decimal? maxAmount);
    }
}
