namespace Expense_Tracker_App.Enum
{
    public enum NotificationType
    {
        Category,
        Transaction, // Giao dịch (thêm hoặc cập nhật gì đó idk)
        RecurringTransaction, // Giao dịch định kỳ được thêm
        BudgetLimitExceed, // Vượt quá ngân sách
        GeneralInfo, // Thông báo chung
        Warning // Các cảnh báo
    }
}
