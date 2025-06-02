using Expense_Tracker_App.Data;
using Expense_Tracker_App.Models;
using Expense_Tracker_App.Repository;

namespace Expense_Tracker_App.Service.Impl
{
    public class RecurringTransactionService : IRecurringTransactionService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<RecurringTransactionService> _logger;
        private readonly TimeSpan _interval = TimeSpan.FromSeconds(30); // kiểm tra mỗi phút
        private readonly ApplicationDbContext _context;
        private readonly IRecurringTransactionRepository _recurringTransactionRepository;

        public RecurringTransactionService(
            IServiceScopeFactory serviceScopeFactory,
            ILogger<RecurringTransactionService> logger,
            ApplicationDbContext context,
            IRecurringTransactionRepository recurringTransactionRepository)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
            _context = context;
            _recurringTransactionRepository = recurringTransactionRepository;
        }

        public async Task<List<RecurringTransaction>> GetUserRecurringTransactionsAsync(string userId)
        {
            return await _recurringTransactionRepository.GetUserRecurringTransactionsAsync(userId);
        }

        public async Task<RecurringTransaction?> GetRecurringTransactionByIdAsync(int id, string userId)
        {
            return await _recurringTransactionRepository.GetRecurringTransactionByIdAsync(id, userId);
        }

        public async Task AddRecurringTransactionAsync(RecurringTransaction transaction)
        {
            await _recurringTransactionRepository.AddRecurringTransactionAsync(transaction);
        }

        public async Task UpdateRecurringTransactionAsync(RecurringTransaction transaction)
        {
            await _recurringTransactionRepository.UpdateRecurringTransactionAsync(transaction);
        }

        public async Task<bool> DeleteRecurringTransactionAsync(int id, string userId)
        {
            return await _recurringTransactionRepository.DeleteRecurringTransactionAsync(id, userId);
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
            return await _recurringTransactionRepository.SearchRecurringTransactionsAsync(userId, searchTerm, categoryId, frequency, startDate, endDate, minAmount, maxAmount);
        }
    }
}
