    using Expense_Tracker_App.Models;

    using Expense_Tracker_App.Data;
    using Expense_Tracker_App.Service;
    using Microsoft.EntityFrameworkCore;
    using Expense_Tracker_App.Enum;

    public class TransactionService : ITransactionService
    {
        private readonly ApplicationDbContext _context;
        private INotificationLogService _notificationService;
        private IBudgetService _budgetService;  

        public TransactionService(ApplicationDbContext context, INotificationLogService notificationService, IBudgetService budgetService)
        {
            _context = context;
            _notificationService = notificationService;
            _budgetService = budgetService;
        }

        public async Task<List<Transaction>> GetUserTransactionsAsync(string userId)
        {
            return await _context.Transactions
                .Include(t => t.Category)
                .Where(t => t.UserId == userId)
                .ToListAsync();
        }
    
        public async Task<Transaction> GetTransactionByIdAsync(int id, string userId) 
        { 
    
            return await _context.Transactions
                .FirstOrDefaultAsync(t => t.TransactionId == id && t.UserId == userId);
        }

        public async Task<bool> CreateOrUpdateTransactionAsync(Transaction transaction, string userId)
        {
            transaction.UserId = userId;


            bool isAllowed = await _budgetService.IsTransactionAllowed(transaction.UserId, transaction.CategoryId, transaction.Amount);
            if (!isAllowed)
            {
                return false;
            }

            string message;
            if (transaction.TransactionId == 0)
            {
                _context.Add(transaction);
                await _context.SaveChangesAsync(); // Lưu trước

                message = NotificationMessageFactory.GenerateMessage(
                    NotificationType.Transaction, "thêm", await GetTitle(transaction.CategoryId),
                    transaction.Amount, transaction.TransactionDate);
            }
            else
            {
                var existingTransaction = await _context.Transactions
                    .FirstOrDefaultAsync(t => t.TransactionId == transaction.TransactionId && t.UserId == userId);

                if (existingTransaction == null)
                    return false;

                _context.Entry(existingTransaction).CurrentValues.SetValues(transaction);
                await _context.SaveChangesAsync(); // Lưu trước

                message = NotificationMessageFactory.GenerateMessage(
                    NotificationType.Transaction, "cập nhật", await GetTitle(transaction.CategoryId),
                    transaction.Amount, transaction.TransactionDate);
            }

            await _notificationService.AddLogAsync(userId, NotificationType.Transaction, message);
            return true;
        }


        public async Task<bool> DeleteTransactionAsync(int id, string userId)
        {
            var transaction = await _context.Transactions
                .FirstOrDefaultAsync(t => t.TransactionId == id && t.UserId == userId);
 
            if (transaction == null)
                return false;


            _context.Transactions.Remove(transaction);
            string message = NotificationMessageFactory
                    .GenerateMessage(NotificationType.Transaction, "xóa", GetTitle(transaction.CategoryId), transaction.Amount, transaction.TransactionDate);
            await _notificationService.AddLogAsync(userId, NotificationType.Transaction, message);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Category>> GetUserCategoriesAsync(string userId)
        {
            var categories = await _context.Categories
                .Where(c => c.UserId == userId)
                .ToListAsync(); // Đảm bảo dùng ToListAsync()

            categories.Insert(0, new Category { CategoryId = 0, Title = "Choose a category" });
            return categories;
        }

    private async Task<string> GetTitle(int categoryId)
    {
        var category = await _context.Categories
            .Where(c => c.CategoryId == categoryId)
            .FirstOrDefaultAsync();

        return category?.Title ?? "Unknown";
    }

}
