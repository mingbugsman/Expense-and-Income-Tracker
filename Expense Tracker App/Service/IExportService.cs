using System.Threading.Tasks;

namespace Expense_Tracker_App.Service
{
    public interface IExportService
    {
        Task<byte[]> ExportTransactionsToCsvAsync(
            string userId,
            string? searchTerm = null,
            int? categoryId = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            decimal? minAmount = null,
            decimal? maxAmount = null);

        Task<byte[]> ExportBudgetsToCsvAsync(
            string userId,
            string? searchTerm = null,
            int? categoryId = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            decimal? minAmount = null,
            decimal? maxAmount = null);
    }
} 