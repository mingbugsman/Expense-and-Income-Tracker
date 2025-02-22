using Expense_Tracker_App.Enum;
using System;
using System.Linq;

namespace Expense_Tracker_App.Service
{
    public static class NotificationMessageFactory
    {
        public static string GenerateMessage(NotificationType type, params object[] parameters)
        {
            return type switch
            {
                NotificationType.Category => $"Bạn đã {parameters.ElementAtOrDefault(0) ?? "thực hiện"} loại giao dịch là {parameters.ElementAtOrDefault(1)?? "không rõ." } Ngày thực hiên : {parameters.ElementAtOrDefault(2)}",
                NotificationType.Transaction =>
                    $"Bạn đã {parameters.ElementAtOrDefault(0) ?? "thực hiện"} giao dịch {parameters.ElementAtOrDefault(1) ?? "không rõ"} với số tiền {parameters.ElementAtOrDefault(2) ?? 0:C}. " +
                    $"Ngày giao dịch : {parameters.ElementAtOrDefault(3) ?? "không rõ"}",

                NotificationType.RecurringTransaction =>
                    $"Giao dịch định kỳ {parameters.ElementAtOrDefault(0) ?? "không rõ"} vừa được tạo và sẽ diễn ra {parameters.ElementAtOrDefault(1) ?? "không xác định"}. " +
                    $"Ngày thực hiên : {parameters.ElementAtOrDefault(2)}",

                NotificationType.BudgetLimitExceed =>
                    $"Cảnh báo! Bạn đã vượt quá ngân sách {parameters.ElementAtOrDefault(0) ?? 0:C} cho {parameters.ElementAtOrDefault(1) ?? "không rõ danh mục"}. " +
                    $"Ngày thực hiện : {parameters.ElementAtOrDefault(2)}",

                NotificationType.GeneralInfo =>
                    parameters.ElementAtOrDefault(0)?.ToString() ?? "Thông báo chung từ hệ thống. " +
                    $"Ngày thực hiện : {parameters.ElementAtOrDefault(1)}",

                NotificationType.Warning =>
                    $"Cảnh báo hệ thống: {parameters.ElementAtOrDefault(0) ?? "Không rõ nội dung"}.",

                _ => "Không xác định loại thông báo."
            };
        }
    }
}
