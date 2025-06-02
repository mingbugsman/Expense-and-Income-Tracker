using Expense_Tracker_App.Data;
using Expense_Tracker_App.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Expense_Tracker_App.Repository.Impl
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly ApplicationDbContext _context;
        public TransactionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Transaction>> GetUserTransactionsAsync(string userId)
        {
            return await _context.Transactions
                .Include(t => t.Category)
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();
        }

        public async Task<Transaction?> GetTransactionByIdAsync(int id, string userId)
        {
            return await _context.Transactions
                .FirstOrDefaultAsync(t => t.TransactionId == id && t.UserId == userId);
        }

        public async Task<bool> CreateOrUpdateTransactionAsync(Transaction transaction, string userId)
        {
            transaction.UserId = userId;
            if (transaction.TransactionId == 0)
            {
                _context.Add(transaction);
                await _context.SaveChangesAsync();
            }
            else
            {
                var existingTransaction = await _context.Transactions
                    .FirstOrDefaultAsync(t => t.TransactionId == transaction.TransactionId && t.UserId == userId);
                if (existingTransaction == null)
                    return false;
                _context.Entry(existingTransaction).CurrentValues.SetValues(transaction);
                await _context.SaveChangesAsync();
            }
            return true;
        }

        public async Task<bool> DeleteTransactionAsync(int id, string userId)
        {
            var transaction = await _context.Transactions
                .FirstOrDefaultAsync(t => t.TransactionId == id && t.UserId == userId);
            if (transaction == null)
                return false;
            _context.Transactions.Remove(transaction);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Category>> GetUserCategoriesAsync(string userId)
        {
            var categories = await _context.Categories
                .Where(c => c.UserId == userId)
                .ToListAsync();
            categories.Insert(0, new Category { CategoryId = 0, Title = "Choose a category" });
            return categories;
        }

        public async Task<string> GetTitleAsync(int categoryId)
        {
            var category = await _context.Categories
                .Where(c => c.CategoryId == categoryId)
                .FirstOrDefaultAsync();
            return category?.Title ?? "Unknown";
        }

        public async Task<List<Transaction>> SearchTransactionsAsync(string userId, string? searchTerm, int? categoryId, DateTime? startDate, DateTime? endDate, decimal? minAmount, decimal? maxAmount)
        {
            var query = _context.Transactions
                .Include(t => t.Category)
                .Where(t => t.UserId == userId);

            if (!string.IsNullOrWhiteSpace(searchTerm))
                query = query.Where(t => t.Note != null && t.Note.Contains(searchTerm));
            if (categoryId.HasValue && categoryId.Value > 0)
                query = query.Where(t => t.CategoryId == categoryId.Value);
            if (startDate.HasValue)
                query = query.Where(t => t.TransactionDate >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(t => t.TransactionDate <= endDate.Value);
            if (minAmount.HasValue)
                query = query.Where(t => t.Amount >= minAmount.Value);
            if (maxAmount.HasValue)
                query = query.Where(t => t.Amount <= maxAmount.Value);

            return await query.OrderByDescending(t => t.TransactionDate).ToListAsync();
        }
    }
}
