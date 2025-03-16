using Expense_Tracker_App.Models;
using Microsoft.EntityFrameworkCore;

namespace Expense_Tracker_App.Service
{
    public interface ITransactionService
    {
        Task<List<Transaction>> GetUserTransactionsAsync(string userId);
        Task<Transaction> GetTransactionByIdAsync(int id, string userId);
        Task<bool> CreateOrUpdateTransactionAsync(Transaction transaction, string userId);
        Task<bool> DeleteTransactionAsync(int id, string userId);
        Task<List<Category>> GetUserCategoriesAsync(string userId);
        

    }
}
