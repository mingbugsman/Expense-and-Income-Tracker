using Expense_Tracker_App.Data;
using Expense_Tracker_App.Models;
using Microsoft.EntityFrameworkCore;
using X.PagedList;
using X.PagedList.Extensions;

namespace Expense_Tracker_App.Repository
{
    public class BudgetRepository : IBudgetRepository
    {
        private readonly ApplicationDbContext _context;
        public BudgetRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Budget?> GetActiveBudgetAsync(string userId, int categoryId, DateTime now)
        {
            return await _context.Budgets
                .Where(b => b.UserId == userId
                         && b.CategoryId == categoryId
                         && b.Budget_StartDate <= now
                         && b.Budget_EndDate >= now)
                .FirstOrDefaultAsync();
        }

        public async Task<decimal> GetTotalSpentAsync(string userId, int categoryId, DateTime start, DateTime end)
        {
            return await _context.Transactions
                .Where(t => t.UserId == userId
                         && t.CategoryId == categoryId
                         && t.TransactionDate >= start
                         && t.TransactionDate <= end)
                .SumAsync(t => t.Amount);
        }

        public async Task<List<Budget>> SearchBudgetsAsync(string userId, int? categoryId, DateTime? startDate, DateTime? endDate, decimal? minAmount, decimal? maxAmount)
        {
            var query = _context.Budgets
                .Include(b => b.Category)
                .Where(b => b.UserId == userId);

            if (categoryId.HasValue && categoryId.Value > 0)
                query = query.Where(b => b.CategoryId == categoryId.Value);
            if (startDate.HasValue)
                query = query.Where(b => b.Budget_StartDate >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(b => b.Budget_EndDate <= endDate.Value);
            if (minAmount.HasValue)
                query = query.Where(b => b.BudgetAmount >= minAmount.Value);
            if (maxAmount.HasValue)
                query = query.Where(b => b.BudgetAmount <= maxAmount.Value);

            return await query.OrderByDescending(b => b.Budget_StartDate).ToListAsync();
        }

        public async Task<IPagedList<Budget>> GetPagedBudgetsAsync(int pageNumber, int pageSize)
        {
            var budgets = await _context.Budgets
                .Include(b => b.Category)
                .OrderByDescending(b => b.Budget_StartDate)
                .ToListAsync();
            return budgets.ToPagedList(pageNumber, pageSize);
        }

        public async Task<Budget?> GetBudgetByIdAsync(int id, string userId)
        {
            return await _context.Budgets
                .FirstOrDefaultAsync(t => t.BudgetID == id && t.UserId == userId);
        }

        public async Task<bool> CreateOrUpdateBudgetAsync(Budget budget)
        {
            if (budget.BudgetID == 0)
            {
                _context.Add(budget);
            }
            else
            {
                var existingBudget = await _context.Budgets
                    .FirstOrDefaultAsync(t => t.BudgetID == budget.BudgetID && t.UserId == budget.UserId);
                if (existingBudget == null)
                    return false;
                _context.Entry(existingBudget).CurrentValues.SetValues(budget);
            }
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteBudgetAsync(int id, string userId)
        {
            var budget = await _context.Budgets
                .FirstOrDefaultAsync(t => t.BudgetID == id && t.UserId == userId);
            if (budget == null)
                return false;
            _context.Budgets.Remove(budget);
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
    }
}
