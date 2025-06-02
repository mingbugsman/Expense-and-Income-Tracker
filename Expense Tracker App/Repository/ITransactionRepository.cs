using Expense_Tracker_App.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Expense_Tracker_App.Repository
{
    public interface ITransactionRepository
    {
        Task<List<Transaction>> GetUserTransactionsAsync(string userId);
        Task<Transaction?> GetTransactionByIdAsync(int id, string userId);
        Task<bool> CreateOrUpdateTransactionAsync(Transaction transaction, string userId);
        Task<bool> DeleteTransactionAsync(int id, string userId);
        Task<List<Category>> GetUserCategoriesAsync(string userId);
        Task<List<Transaction>> SearchTransactionsAsync(string userId, string? searchTerm, int? categoryId, DateTime? startDate, DateTime? endDate, decimal? minAmount, decimal? maxAmount);
        Task<string> GetTitleAsync(int categoryId);
    }
}
