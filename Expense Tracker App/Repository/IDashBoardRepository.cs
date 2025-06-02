using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Expense_Tracker_App.Models;

namespace Expense_Tracker_App.Repository
{
    public interface IDashBoardRepository
    {
        Task<List<Transaction>> GetUserTransactionsForPeriodAsync(string userId, DateTime startDate, DateTime endDate);
    }
}
