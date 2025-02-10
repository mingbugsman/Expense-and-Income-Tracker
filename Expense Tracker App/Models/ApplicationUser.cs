using Microsoft.AspNetCore.Identity;

namespace Expense_Tracker_App.Models
{
    public class ApplicationUser : IdentityUser
    {
        public DateTime DateOfBirth { get; set; }


        // Quan hệ 1-n với Category
        public virtual ICollection<Category> Categories { get; set; } = [];

        // Quan hệ 1-n với Transaction
        public virtual ICollection<Transaction> Transactions { get; set; } = [];

        // Quan hệ 1-n với Budget
        public virtual ICollection<Budget> Budgets { get; set; } = [];

        // Quạn hệ 1-n với Recurring Transaction
        public virtual ICollection<RecurringTransaction> RecurringTransactions { get; set; } = [];


        // Quan hệ 1-n với Notification LOG
        public virtual ICollection<NotificationLog> NotificationLogs { get; set; } = [];

    }
}
