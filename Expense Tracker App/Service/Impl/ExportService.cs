using System.Text;
using Expense_Tracker_App.Models;
using Expense_Tracker_App.Service;

namespace Expense_Tracker_App.Service.Impl
{
    public class ExportService : IExportService
    {
        private readonly ITransactionService _transactionService;
        private readonly IBudgetService _budgetService;

        public ExportService(ITransactionService transactionService, IBudgetService budgetService)
        {
            _transactionService = transactionService;
            _budgetService = budgetService;
        }

        public async Task<byte[]> ExportTransactionsToCsvAsync(
            string userId,
            string? searchTerm = null,
            int? categoryId = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            decimal? minAmount = null,
            decimal? maxAmount = null)
        {
            var transactions = await _transactionService.SearchTransactionsAsync(
                userId, searchTerm, categoryId, startDate, endDate, minAmount, maxAmount);

            // Add BOM for Excel to recognize UTF-8
            var csv = new StringBuilder();
            csv.AppendLine("Date,Category,Amount,Note");

            foreach (var transaction in transactions)
            {
                var amount = transaction.Category?.Type == "Expense" ? 
                    $"-{transaction.Amount}" : 
                    $"+{transaction.Amount}";
                
                var note = transaction.Note ?? "";
                // Escape quotes and handle special characters
                note = note.Replace("\"", "\"\"");
                
                csv.AppendLine($"{transaction.TransactionDate:dd-MM-yyyy}," +
                             $"{transaction.Category?.Title}," +
                             $"{amount}," +
                             $"\"{note}\"");
            }

            // Convert to UTF-8 with BOM
          
            
            return convertToUTF_8(csv);
        }
        public byte[] convertToUTF_8(StringBuilder csv)
        {
            var bom = new byte[] { 0xEF, 0xBB, 0xBF };
            var csvBytes = Encoding.UTF8.GetBytes(csv.ToString());
            var result = new byte[bom.Length + csvBytes.Length];
            Buffer.BlockCopy(bom, 0, result, 0, bom.Length);
            Buffer.BlockCopy(csvBytes, 0, result, bom.Length, csvBytes.Length);
            return result;
        }

        public async Task<byte[]> ExportBudgetsToCsvAsync(
            string userId,
            string? searchTerm = null,
            int? categoryId = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            decimal? minAmount = null,
            decimal? maxAmount = null)
        {
            var budgets = await _budgetService.SearchBudgetsAsync(
                userId, categoryId, startDate, endDate, minAmount, maxAmount);

            // Add BOM for Excel to recognize UTF-8
            var csv = new StringBuilder();
            csv.AppendLine("Category,Amount,Start Date,End Date,Note");

            foreach (var budget in budgets)
            {
                // Escape quotes and handle special characters                
                csv.AppendLine($"{budget.Category?.Title}," +
                             $"{budget.BudgetAmount}," +
                             $"{budget.Budget_StartDate:dd-MM-yyyy}," +
                             $"{budget.Budget_EndDate:dd-MM-yyyy}");
            }

            // Convert to UTF-8 with BOM

            return convertToUTF_8(csv);
        }
    }
} 