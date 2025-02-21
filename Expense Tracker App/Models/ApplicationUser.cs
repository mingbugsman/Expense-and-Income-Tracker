using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Expense_Tracker_App.Models
{
    public class ApplicationUser : IdentityUser

    {
        [MaxLength(100)]
        
        public override string UserName { get; set; } // Giảm từ 256 -> 100

        [MaxLength(100)]
        public override string NormalizedUserName { get; set; } // Giảm từ 256 -> 100

        [MaxLength(150)]
        public override string Email { get; set; } // Giảm từ 256 -> 150

        [MaxLength(15)]
        public override string PhoneNumber { get; set; }
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
