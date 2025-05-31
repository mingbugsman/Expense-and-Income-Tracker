using Expense_Tracker_App.Data;
using Expense_Tracker_App.Enum;
using Expense_Tracker_App.Models;
using Expense_Tracker_App.Service.Impl;

namespace Expense_Tracker_App.Service
{
    public class RecurringTransactionCronJobService : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<RecurringTransactionService> _logger;
        private readonly TimeSpan _interval = TimeSpan.FromHours(1); // Check every hour

        public RecurringTransactionCronJobService(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<RecurringTransactionService> logger
        )
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(">>> RecurringTransactionService started at {time}", DateTime.UtcNow.AddHours(7));

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation(">>> Checking recurring transactions at {time}", DateTime.UtcNow.AddHours(7).Date);
                try
                {
                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationLogService>();

                        // If you're in Vietnam, add 7 hours to get the correct date
                        var now = DateTime.UtcNow.AddHours(7).Date;

                        var transactionsToAdd = dbContext.RecurringTransactions
                            .Where(rt => rt.StartDate <= now && (!rt.EndDate.HasValue || rt.EndDate >= now))
                            .ToList();

                        _logger.LogInformation("Found {Count} recurring transactions to process at {Time}.", transactionsToAdd.Count, now);

                        foreach (var recurringTransaction in transactionsToAdd)
                        {
                            _logger.LogInformation("Checking recurring transaction: UserId={UserId}, CategoryId={CategoryId}, Amount={Amount}, Date={Date}",
                                recurringTransaction.UserId, recurringTransaction.CategoryId, recurringTransaction.Amount, now);

                            // Check if transaction should be processed based on frequency
                            if (!ShouldProcessTransaction(recurringTransaction, now))
                            {
                                _logger.LogInformation("Transaction frequency condition not met for UserId={UserId} on {Date}. Skipping.", recurringTransaction.UserId, now);
                                continue;
                            }

                            var exists = dbContext.Transactions.Any(t =>
                                t.UserId == recurringTransaction.UserId &&
                                t.CategoryId == recurringTransaction.CategoryId &&
                                t.TransactionDate == now &&
                                t.Amount == recurringTransaction.Amount);

                            if (exists)
                            {
                                _logger.LogInformation("Transaction already exists for UserId={UserId} on {Date}. Skipping.", recurringTransaction.UserId, now);
                                continue;
                            }

                            var newTransaction = new Transaction
                            {
                                Amount = recurringTransaction.Amount,
                                TransactionDate = now,
                                CategoryId = recurringTransaction.CategoryId,
                                UserId = recurringTransaction.UserId,
                                Note = $"Recurring transaction on {now:yyyy-MM-dd}"
                            };

                            dbContext.Transactions.Add(newTransaction);
                            await dbContext.SaveChangesAsync(stoppingToken);

                            var category = await dbContext.Categories.FindAsync(recurringTransaction.CategoryId);
                            string message = $"Giao dịch định kỳ {category?.Title ?? "Unknown"} với số tiền {recurringTransaction.Amount:C} đã được thêm vào ngày {now:dd/MM/yyyy}";
                            await notificationService.AddLogAsync(recurringTransaction.UserId, NotificationType.RecurringTransaction, message);

                            _logger.LogInformation("Added recurring transaction for UserId={UserId} on {Date}.", recurringTransaction.UserId, now);
                        }
                    }

                    _logger.LogInformation("Recurring transactions processed successfully at {Time}.", DateTime.UtcNow.AddHours(7));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing recurring transactions at {Time}.", DateTime.UtcNow.AddHours(7));
                }

                await Task.Delay(_interval, stoppingToken);
            }
        }

        private bool ShouldProcessTransaction(RecurringTransaction transaction, DateTime currentDate)
        {
            var daysSinceStart = (currentDate - transaction.StartDate.Date).Days;

            switch (transaction.Frequency)
            {
                case FrequencyType.Daily:
                    return true;

                case FrequencyType.Weekly:
                    return daysSinceStart % 7 == 0;

                case FrequencyType.Monthly:
                    return currentDate.Day == transaction.StartDate.Day;

                case FrequencyType.Yearly:
                    return currentDate.Day == transaction.StartDate.Day && 
                           currentDate.Month == transaction.StartDate.Month;

                case FrequencyType.Custom:
                    return daysSinceStart % transaction.CustomIntervalDays == 0;

                default:
                    return false;
            }
        }
    }
}
