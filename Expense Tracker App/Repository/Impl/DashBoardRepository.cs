using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Expense_Tracker_App.Data;
using Expense_Tracker_App.Models;
using Microsoft.EntityFrameworkCore;

namespace Expense_Tracker_App.Repository.Impl
{
    public class DashBoardRepository : IDashBoardRepository
    {
        private readonly ApplicationDbContext _context;
        public DashBoardRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Transaction>> GetUserTransactionsForPeriodAsync(string userId, DateTime startDate, DateTime endDate)
        {
            return await _context.Transactions
                .Include(t => t.Category)
                .Where(t => t.UserId == userId && t.TransactionDate >= startDate && t.TransactionDate <= endDate)
                .ToListAsync();
        }
    }
}
