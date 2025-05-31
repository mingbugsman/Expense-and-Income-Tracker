using Expense_Tracker_App.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Expense_Tracker_App.Service
{
    public interface IRecurringTransactionService
    {
        Task<List<RecurringTransaction>> GetUserRecurringTransactionsAsync(string userId);
        Task<RecurringTransaction?> GetRecurringTransactionByIdAsync(int id, string userId);
        Task AddRecurringTransactionAsync(RecurringTransaction transaction);
        Task UpdateRecurringTransactionAsync(RecurringTransaction transaction);
        Task<bool> DeleteRecurringTransactionAsync(int id, string userId);
        Task<List<RecurringTransaction>> SearchRecurringTransactionsAsync(
            string userId, 
            string? searchTerm, 
            int? categoryId, 
            string? frequency,
            DateTime? startDate, 
            DateTime? endDate, 
            decimal? minAmount, 
            decimal? maxAmount);
    }
} 