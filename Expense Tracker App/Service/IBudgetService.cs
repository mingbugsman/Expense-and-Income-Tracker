using Expense_Tracker_App.Models;
using Expense_Tracker_App.Enum;
using Microsoft.EntityFrameworkCore;
using X.PagedList;

namespace Expense_Tracker_App.Service
{
    public interface IBudgetService
    {
        Task<bool> IsTransactionAllowed(string userId, int categoryId, decimal amount);
        Task NotifyBudgetExceeded(string userId, int categoryId, decimal amount);
        Task<List<Budget>> SearchBudgetsAsync(string userId, int? categoryId, DateTime? startDate, DateTime? endDate, decimal? minAmount, decimal? maxAmount);
        Task<IPagedList<Budget>> GetPagedBudgetsAsync(int pageNumber, int pageSize);
        Task<Budget?> GetBudgetByIdAsync(int id, string userId);
        Task<bool> CreateOrUpdateBudgetAsync(Budget budget);
        Task<bool> DeleteBudgetAsync(int id, string userId);
        Task<List<Category>> GetUserCategoriesAsync(string userId);
    }
}   
