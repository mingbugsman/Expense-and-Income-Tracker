using Expense_Tracker_App.Models;

using Expense_Tracker_App.Data;
using Expense_Tracker_App.Service;
using Microsoft.EntityFrameworkCore;
using Expense_Tracker_App.Enum;
using Expense_Tracker_App.Repository;

public class TransactionService : ITransactionService
{
    private readonly ApplicationDbContext _context;
    private INotificationLogService _notificationService;
    private IBudgetService _budgetService;  
    private readonly ITransactionRepository _transactionRepository;

    public TransactionService(ApplicationDbContext context, INotificationLogService notificationService, IBudgetService budgetService, ITransactionRepository transactionRepository)
    {
        _context = context;
        _notificationService = notificationService;
        _budgetService = budgetService;
        _transactionRepository = transactionRepository;
    }

    public async Task<List<Transaction>> GetUserTransactionsAsync(string userId)
    {
        return await _transactionRepository.GetUserTransactionsAsync(userId);
    }

    public async Task<Transaction> GetTransactionByIdAsync(int id, string userId) 
    { 
        return await _transactionRepository.GetTransactionByIdAsync(id, userId);
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
            await _transactionRepository.CreateOrUpdateTransactionAsync(transaction, userId);

            message = NotificationMessageFactory.Transaction(
                "thêm", await GetTitle(transaction.CategoryId), transaction.Amount, transaction.TransactionDate);
        }
        else
        {
            var existingTransaction = await _transactionRepository.GetTransactionByIdAsync(transaction.TransactionId, userId);

            if (existingTransaction == null)
                return false;

            await _transactionRepository.CreateOrUpdateTransactionAsync(transaction, userId);

            message = NotificationMessageFactory.Transaction(
                "cập nhật", await GetTitle(transaction.CategoryId), transaction.Amount, transaction.TransactionDate);
        }

        await _notificationService.AddLogAsync(userId, NotificationType.Transaction, message);
        return true;
    }

    public async Task<bool> DeleteTransactionAsync(int id, string userId)
    {
        var transaction = await _transactionRepository.GetTransactionByIdAsync(id, userId);

        if (transaction == null)
            return false;

        await _transactionRepository.DeleteTransactionAsync(id, userId);
        string message = NotificationMessageFactory
                .Transaction( "xóa", await GetTitle(transaction.CategoryId), transaction.Amount, transaction.TransactionDate);
        await _notificationService.AddLogAsync(userId, NotificationType.Transaction, message);
        return true;
    }

    public async Task<List<Category>> GetUserCategoriesAsync(string userId)
    {
        return await _transactionRepository.GetUserCategoriesAsync(userId);
    }

    private async Task<string> GetTitle(int categoryId)
    {
        return await _transactionRepository.GetTitleAsync(categoryId);
    }

    public async Task<List<Transaction>> SearchTransactionsAsync(string userId, string? searchTerm, int? categoryId, DateTime? startDate, DateTime? endDate, decimal? minAmount, decimal? maxAmount)
    {
        return await _transactionRepository.SearchTransactionsAsync(userId, searchTerm, categoryId, startDate, endDate, minAmount, maxAmount);
    }
}
