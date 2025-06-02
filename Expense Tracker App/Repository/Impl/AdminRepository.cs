using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Expense_Tracker_App.Data;
using Expense_Tracker_App.Models;
using Microsoft.EntityFrameworkCore;

namespace Expense_Tracker_App.Repository.Impl
{
    public class AdminRepository : IAdminRepository
    {
        private readonly ApplicationDbContext _context;
        public AdminRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<int> GetTotalUsersAsync() => await _context.Users.CountAsync();
        public async Task<int> GetTotalTransactionsAsync() => await _context.Transactions.CountAsync();
        public async Task<int> GetTotalCategoriesAsync() => await _context.Categories.CountAsync();
        public async Task<int> GetTotalBudgetsAsync() => await _context.Budgets.CountAsync();
        public async Task<List<ApplicationUser>> GetRecentUsersAsync(int count) => await _context.Users.OrderByDescending(u => u.Id).Take(count).Select(u => new ApplicationUser { Id = u.Id, UserName = u.UserName ?? string.Empty, Email = u.Email ?? string.Empty }).ToListAsync();
        public async Task<List<Transaction>> GetRecentTransactionsAsync(int count) => await _context.Transactions.Include(t => t.Category).Include(t => t.User).OrderByDescending(t => t.TransactionDate).Take(count).Select(t => new Transaction { TransactionId = t.TransactionId, Note = t.Note ?? string.Empty, Amount = t.Amount, TransactionDate = t.TransactionDate, Category = t.Category != null ? new Category { Title = t.Category.Title ?? string.Empty } : null, User = t.User != null ? new ApplicationUser { UserName = t.User.UserName ?? string.Empty } : null }).ToListAsync();
        public async Task<List<ApplicationUser>> GetUsersAsync(string searchTerm, int skip, int take)
        {
            var query = _context.Users.AsQueryable();
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(u => u.UserName != null && u.UserName.ToLower().Contains(searchTerm) || u.Email != null && u.Email.ToLower().Contains(searchTerm) || u.FullName != null && u.FullName.ToLower().Contains(searchTerm));
            }
            return await query.OrderBy(u => u.UserName).Skip(skip).Take(take).Select(u => new ApplicationUser { Id = u.Id, UserName = u.UserName ?? string.Empty, Email = u.Email ?? string.Empty, PhoneNumber = u.PhoneNumber ?? string.Empty, FullName = u.FullName ?? string.Empty }).ToListAsync();
        }
        public async Task<int> GetUsersCountAsync(string searchTerm)
        {
            var query = _context.Users.AsQueryable();
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(u => u.UserName != null && u.UserName.ToLower().Contains(searchTerm) || u.Email != null && u.Email.ToLower().Contains(searchTerm) || u.FullName != null && u.FullName.ToLower().Contains(searchTerm));
            }
            return await query.CountAsync();
        }
        public async Task<List<Category>> GetCategoriesAsync(string searchTerm, int skip, int take)
        {
            var query = _context.Categories.AsQueryable();
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(c => c.Title != null && c.Title.ToLower().Contains(searchTerm));
            }
            return await query.OrderBy(c => c.Title).Skip(skip).Take(take).ToListAsync();
        }
        public async Task<int> GetCategoriesCountAsync(string searchTerm)
        {
            var query = _context.Categories.AsQueryable();
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(c => c.Title != null && c.Title.ToLower().Contains(searchTerm));
            }
            return await query.CountAsync();
        }
        public async Task<List<Transaction>> GetTransactionsAsync(string searchTerm, int? categoryId, DateTime? startDate, DateTime? endDate, decimal? minAmount, decimal? maxAmount, int skip, int take)
        {
            var query = _context.Transactions.Include(t => t.Category).Include(t => t.User).AsQueryable();
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(t => t.Note != null && t.Note.ToLower().Contains(searchTerm) || t.Category != null && t.Category.Title != null && t.Category.Title.ToLower().Contains(searchTerm) || t.User != null && t.User.UserName != null && t.User.UserName.ToLower().Contains(searchTerm));
            }
            if (categoryId.HasValue && categoryId.Value > 0) query = query.Where(t => t.CategoryId == categoryId.Value);
            if (startDate.HasValue) query = query.Where(t => t.TransactionDate >= startDate.Value);
            if (endDate.HasValue) query = query.Where(t => t.TransactionDate <= endDate.Value);
            if (minAmount.HasValue) query = query.Where(t => t.Amount >= minAmount.Value);
            if (maxAmount.HasValue) query = query.Where(t => t.Amount <= maxAmount.Value);
            return await query.OrderByDescending(t => t.TransactionDate).Skip(skip).Take(take).ToListAsync();
        }
        public async Task<int> GetTransactionsCountAsync(string searchTerm, int? categoryId, DateTime? startDate, DateTime? endDate, decimal? minAmount, decimal? maxAmount)
        {
            var query = _context.Transactions.AsQueryable();
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(t => t.Note != null && t.Note.ToLower().Contains(searchTerm) || t.Category != null && t.Category.Title != null && t.Category.Title.ToLower().Contains(searchTerm) || t.User != null && t.User.UserName != null && t.User.UserName.ToLower().Contains(searchTerm));
            }
            if (categoryId.HasValue && categoryId.Value > 0) query = query.Where(t => t.CategoryId == categoryId.Value);
            if (startDate.HasValue) query = query.Where(t => t.TransactionDate >= startDate.Value);
            if (endDate.HasValue) query = query.Where(t => t.TransactionDate <= endDate.Value);
            if (minAmount.HasValue) query = query.Where(t => t.Amount >= minAmount.Value);
            if (maxAmount.HasValue) query = query.Where(t => t.Amount <= maxAmount.Value);
            return await query.CountAsync();
        }
        public async Task<List<Category>> GetAllCategoriesAsync() => await _context.Categories.ToListAsync();
        public async Task<List<ApplicationUser>> GetAllUsersAsync() => await _context.Users.Select(u => new ApplicationUser { Id = u.Id, UserName = u.UserName ?? string.Empty, Email = u.Email ?? string.Empty }).ToListAsync();
        public async Task<List<Budget>> GetBudgetsAsync(string searchTerm, int? categoryId, string? userId, DateTime? startDate, DateTime? endDate, decimal? minAmount, decimal? maxAmount, int skip, int take)
        {
            var query = _context.Budgets.Include(b => b.Category).Include(b => b.User).AsQueryable();
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(b => b.User != null && b.User.UserName != null && b.User.UserName.ToLower().Contains(searchTerm) || b.Category != null && b.Category.Title != null && b.Category.Title.ToLower().Contains(searchTerm));
            }
            if (categoryId.HasValue && categoryId.Value > 0) query = query.Where(b => b.CategoryId == categoryId.Value);
            if (!string.IsNullOrWhiteSpace(userId)) query = query.Where(b => b.UserId == userId);
            if (startDate.HasValue) query = query.Where(b => b.Budget_StartDate >= startDate.Value);
            if (endDate.HasValue) query = query.Where(b => b.Budget_EndDate <= endDate.Value);
            if (minAmount.HasValue) query = query.Where(b => b.BudgetAmount >= minAmount.Value);
            if (maxAmount.HasValue) query = query.Where(b => b.BudgetAmount <= maxAmount.Value);
            return await query.OrderByDescending(b => b.Budget_StartDate).Skip(skip).Take(take).ToListAsync();
        }
        public async Task<int> GetBudgetsCountAsync(string searchTerm, int? categoryId, string? userId, DateTime? startDate, DateTime? endDate, decimal? minAmount, decimal? maxAmount)
        {
            var query = _context.Budgets.AsQueryable();
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(b => b.User != null && b.User.UserName != null && b.User.UserName.ToLower().Contains(searchTerm) || b.Category != null && b.Category.Title != null && b.Category.Title.ToLower().Contains(searchTerm));
            }
            if (categoryId.HasValue && categoryId.Value > 0) query = query.Where(b => b.CategoryId == categoryId.Value);
            if (!string.IsNullOrWhiteSpace(userId)) query = query.Where(b => b.UserId == userId);
            if (startDate.HasValue) query = query.Where(b => b.Budget_StartDate >= startDate.Value);
            if (endDate.HasValue) query = query.Where(b => b.Budget_EndDate <= endDate.Value);
            if (minAmount.HasValue) query = query.Where(b => b.BudgetAmount >= minAmount.Value);
            if (maxAmount.HasValue) query = query.Where(b => b.BudgetAmount <= maxAmount.Value);
            return await query.CountAsync();
        }
    }
}
