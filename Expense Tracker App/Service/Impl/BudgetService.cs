using Expense_Tracker_App.Data;
using Expense_Tracker_App.Models;
using Expense_Tracker_App.Enum;
using Microsoft.EntityFrameworkCore;
using X.PagedList;
using X.PagedList.Extensions;

namespace Expense_Tracker_App.Service.Impl
{
    public class BudgetService : IBudgetService
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationLogService _notificationService;
        private readonly ILogger<BudgetService> _logger;
        private readonly IBudgetRepository _budgetRepository;
        public BudgetService(ApplicationDbContext context, INotificationLogService notificationService, ILogger<BudgetService> logger, IBudgetRepository budgetRepository)
        {
            _context = context;
            _notificationService = notificationService;
            _logger = logger;
            _budgetRepository = budgetRepository;
        }

        public async Task<bool> IsTransactionAllowed(string userId, int categoryId, decimal amount)
        {
            var now = DateTime.UtcNow;
            await Console.Out.WriteLineAsync(userId.ToString() + " " +categoryId + " " +amount.ToString());

            // Lấy ngân sách đang hoạt động cho category của user
            var budget = await _budgetRepository.GetActiveBudgetAsync(userId, categoryId, now);

            if (budget == null)
            {
                return true; 
            }

            // Tính tổng số tiền đã chi tiêu trong khoảng thời gian ngân sách
            var totalSpent = await _budgetRepository.GetTotalSpentAsync(userId, categoryId, budget.Budget_StartDate, budget.Budget_EndDate);


            _logger.LogInformation("TOTAL SPENT : " +totalSpent.ToString());

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

            string message = NotificationMessageFactory.BudgetLimitExceed(amount, categoryName, DateTime.UtcNow);

            await _notificationService.AddLogAsync(userId,NotificationType.BudgetLimitExceed, message);
        }

        public async Task<List<Budget>> SearchBudgetsAsync(string userId, int? categoryId, DateTime? startDate, DateTime? endDate, decimal? minAmount, decimal? maxAmount)
        {
            return await _budgetRepository.SearchBudgetsAsync(userId, categoryId, startDate, endDate, minAmount, maxAmount);
        }

        public async Task<IPagedList<Budget>> GetPagedBudgetsAsync(int pageNumber, int pageSize)
        {
            return await _budgetRepository.GetPagedBudgetsAsync(pageNumber, pageSize);
        }

        public async Task<Budget?> GetBudgetByIdAsync(int id, string userId)
        {
            return await _budgetRepository.GetBudgetByIdAsync(id, userId);
        }

        public async Task<bool> CreateOrUpdateBudgetAsync(Budget budget)
        {
            try
            {
                return await _budgetRepository.CreateOrUpdateBudgetAsync(budget);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating/updating budget");
                return false;
            }
        }

        public async Task<bool> DeleteBudgetAsync(int id, string userId)
        {
            try
            {
                return await _budgetRepository.DeleteBudgetAsync(id, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting budget");
                return false;
            }
        }

        public async Task<List<Category>> GetUserCategoriesAsync(string userId)
        {
            return await _budgetRepository.GetUserCategoriesAsync(userId);
        }
    }
}
