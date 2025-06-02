using Expense_Tracker_App.Models;

public interface IBudgetRepository
{
    Task<Budget?> GetActiveBudgetAsync(string userId, int categoryId, DateTime now);
    Task<decimal> GetTotalSpentAsync(string userId, int categoryId, DateTime start, DateTime end);
    Task<List<Budget>> SearchBudgetsAsync(string userId, int? categoryId, DateTime? startDate, DateTime? endDate, decimal? minAmount, decimal? maxAmount);
    Task<X.PagedList.IPagedList<Budget>> GetPagedBudgetsAsync(int pageNumber, int pageSize);
    Task<Budget?> GetBudgetByIdAsync(int id, string userId);
    Task<bool> CreateOrUpdateBudgetAsync(Budget budget);
    Task<bool> DeleteBudgetAsync(int id, string userId);
    Task<List<Category>> GetUserCategoriesAsync(string userId);
}
