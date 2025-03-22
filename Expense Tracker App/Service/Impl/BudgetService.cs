using Expense_Tracker_App.Data;
using Expense_Tracker_App.Models;
using Expense_Tracker_App.Enum;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol;

namespace Expense_Tracker_App.Service.Impl
{
    public class BudgetService : IBudgetService
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationLogService _notificationService;
        private readonly ILogger<BudgetService> logger;
        public BudgetService(ApplicationDbContext context, INotificationLogService notificationService, ILogger<BudgetService> logger)
        {
            _context = context;
            _notificationService = notificationService;
            this.logger = logger;
        }

        public async Task<bool> IsTransactionAllowed(string userId, int categoryId, decimal amount)
        {
            var now = DateTime.UtcNow;
            await Console.Out.WriteLineAsync(userId.ToString() + " " +categoryId + " " +amount.ToString());

            // Lấy ngân sách đang hoạt động cho category của user
            var budget = await _context.Budgets
                .Where(b => b.UserId == userId
                         && b.CategoryId == categoryId
                         && b.Budget_StartDate <= now
                         && b.Budget_EndDate >= now)
                .FirstOrDefaultAsync();

            if (budget == null)
            {
                return true; 
            }

            // Tính tổng số tiền đã chi tiêu trong khoảng thời gian ngân sách
            var totalSpent = await _context.Transactions
                .Where(t => t.UserId == userId
                         && t.CategoryId == categoryId
                         && t.TransactionDate >= budget.Budget_StartDate
                         && t.TransactionDate <= budget.Budget_EndDate)
                .SumAsync(t => t.Amount);


            logger.LogInformation("TOTAL SPENT : " +totalSpent.ToString());

            // Kiểm tra nếu thêm giao dịch mới sẽ vượt quá ngân sách
            if (totalSpent + amount > budget.BudgetAmount)
            {
                await NotifyBudgetExceeded(userId, categoryId, amount);
                return false; // Không cho phép giao dịch
            }

            return true; 
        }

        public async Task NotifyBudgetExceeded(string userId, int categoryId, decimal amount)
        {
            var category = await _context.Categories.FindAsync(categoryId);
            string categoryName = category?.Title ?? "Unknown";

            string message = NotificationMessageFactory.GenerateMessage(NotificationType.BudgetLimitExceed, amount, categoryName, DateTime.Now);

            await _notificationService.AddLogAsync(userId,NotificationType.BudgetLimitExceed, message);
        }
    }
}
