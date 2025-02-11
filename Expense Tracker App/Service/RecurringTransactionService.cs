using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Expense_Tracker_App.Data;
using Expense_Tracker_App.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class RecurringTransactionService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<RecurringTransactionService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromHours(1); // Kiểm tra mỗi giờ

    public RecurringTransactionService(IServiceScopeFactory serviceScopeFactory, ILogger<RecurringTransactionService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    var now = DateTime.UtcNow.Date;
                    var transactionsToAdd = dbContext.RecurringTransactions
                        .Where(rt => rt.StartDate <= now && (!rt.EndDate.HasValue || rt.EndDate >= now))
                        .ToList();

                    foreach (var recurringTransaction in transactionsToAdd)
                    {
                        var newTransaction = new Transaction
                        {
                            Amount = recurringTransaction.Amount,
                            TransactionDate = now,
                            CategoryId = recurringTransaction.CategoryId,
                            UserId = recurringTransaction.UserId,
                            Note = $"Recurring transaction on {now:yyyy-MM-dd}"
                        };

                        dbContext.Transactions.Add(newTransaction);
                    }

                    await dbContext.SaveChangesAsync();
                }

                _logger.LogInformation("Recurring transactions processed at: {Time}", DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing recurring transactions.");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }
}
