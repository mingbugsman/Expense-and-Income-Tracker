using Expense_Tracker_App.Data;
using Expense_Tracker_App.Models;
using Expense_Tracker_App.Enum;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Expense_Tracker_App.Repository
{
    public class RecurringTransactionRepository : IRecurringTransactionRepository
    {
        private readonly ApplicationDbContext _context;
        public RecurringTransactionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<RecurringTransaction>> GetUserRecurringTransactionsAsync(string userId)
        {
            return await _context.RecurringTransactions
                .Include(r => r.Category)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.StartDate)
                .ToListAsync();
        }

        public async Task<RecurringTransaction?> GetRecurringTransactionByIdAsync(int id, string userId)
        {
            return await _context.RecurringTransactions
                .Include(r => r.Category)
                .FirstOrDefaultAsync(r => r.RecurringTransactionId == id && r.UserId == userId);
        }

        public async Task AddRecurringTransactionAsync(RecurringTransaction transaction)
        {
            _context.Add(transaction);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateRecurringTransactionAsync(RecurringTransaction transaction)
        {
            var existingTransaction = await GetRecurringTransactionByIdAsync(transaction.RecurringTransactionId, transaction.UserId);
            if (existingTransaction != null)
            {
                _context.Entry(existingTransaction).CurrentValues.SetValues(transaction);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> DeleteRecurringTransactionAsync(int id, string userId)
        {
            var transaction = await GetRecurringTransactionByIdAsync(id, userId);
            if (transaction == null)
                return false;
            _context.RecurringTransactions.Remove(transaction);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<RecurringTransaction>> SearchRecurringTransactionsAsync(
            string userId,
            string? searchTerm,
            int? categoryId,
            string? frequency,
            DateTime? startDate,
            DateTime? endDate,
            decimal? minAmount,
            decimal? maxAmount)
        {
            var query = _context.RecurringTransactions
                .Include(r => r.Category)
                .Where(r => r.UserId == userId);

            if (!string.IsNullOrWhiteSpace(searchTerm))
                query = query.Where(r => r.Note.Contains(searchTerm));
            if (categoryId.HasValue && categoryId.Value > 0)
                query = query.Where(r => r.CategoryId == categoryId.Value);
            if (!string.IsNullOrEmpty(frequency))
            {
                int frequencyValue = frequency.ToLower() switch
                {
                    "daily" => (int)FrequencyType.Daily,
                    "weekly" => (int)FrequencyType.Weekly,
                    "monthly" => (int)FrequencyType.Monthly,
                    "yearly" => (int)FrequencyType.Yearly,
                    "custom" => (int)FrequencyType.Custom,
                    _ => -1
                };
                if (frequencyValue != -1)
                    query = query.Where(r => (int)r.Frequency == frequencyValue);
            }
            if (startDate.HasValue)
                query = query.Where(r => r.StartDate >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(r => r.EndDate <= endDate.Value || r.EndDate == null);
            if (minAmount.HasValue)
                query = query.Where(r => r.Amount >= minAmount.Value);
            if (maxAmount.HasValue)
                query = query.Where(r => r.Amount <= maxAmount.Value);

            return await query
                .OrderByDescending(r => r.StartDate)
                .Select(r => new RecurringTransaction
                {
                    RecurringTransactionId = r.RecurringTransactionId,
                    Amount = r.Amount,
                    StartDate = r.StartDate,
                    EndDate = r.EndDate,
                    Frequency = r.Frequency,
                    CustomIntervalDays = r.CustomIntervalDays,
                    Note = r.Note,
                    CategoryId = r.CategoryId,
                    Category = r.Category,
                    UserId = r.UserId
                })
                .ToListAsync();
        }
    }
}
