using Expense_Tracker_App.Models;
using Expense_Tracker_App.Enum;

namespace Expense_Tracker_App.Service
{
    public interface IBudgetService
    {
        Task<bool> IsTransactionAllowed(string userId, int categoryId, decimal amount);
        Task NotifyBudgetExceeded(string userId, int categoryId, decimal amount);
    }
}
