using Expense_Tracker_App.Models;

namespace Expense_Tracker_App.Service
{
    public interface ITransactionService
    {
        Task<List<Transaction>> GetUserTransactionsAsync(string userId);
        Task<Transaction> GetTransactionByIdAsync(int id, string userId);
        Task CreateOrUpdateTransactionAsync(Transaction transaction, string userId);
        Task<bool> DeleteTransactionAsync(int id, string userId);
        List<Category> GetUserCategories(string userId);
    }
}
