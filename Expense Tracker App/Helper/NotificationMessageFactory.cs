using Expense_Tracker_App.Enum;
using System;
using System.Linq;

namespace Expense_Tracker_App.Service
{
    public static class NotificationMessageFactory
    {
        public static string Category(string action, string categoryName, DateTime date)
            => $"Bạn đã {action} loại giao dịch là {categoryName}. Ngày thực hiện: {date:dd/MM/yyyy}";

        public static string Transaction(string action, string transactionName, decimal amount, DateTime date)
            => $"Bạn đã {action} giao dịch {transactionName} với số tiền {amount:C}. Ngày giao dịch: {date:dd/MM/yyyy}";

        public static string RecurringTransaction(string name, string frequency, DateTime date)
            => $"Giao dịch định kỳ {name} vừa được tạo và sẽ diễn ra {frequency}. Ngày thực hiện: {date:dd/MM/yyyy}";

        public static string BudgetLimitExceed(decimal amount, string categoryName, DateTime date)
            => $"Cảnh báo! Bạn đã vượt quá ngân sách {amount:C} cho {categoryName}. Ngày thực hiện: {date:dd/MM/yyyy}";

        public static string GeneralInfo(string info, DateTime date)
            => $"{info} Ngày thực hiện: {date:dd/MM/yyyy}";

        public static string Warning(string content)
            => $"Cảnh báo hệ thống: {content}.";

        public static string Unknown() => "Không xác định loại thông báo.";
    }
}
