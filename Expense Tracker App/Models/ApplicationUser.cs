using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Expense_Tracker_App.Models
{
    public class ApplicationUser : IdentityUser

    {
        [MaxLength(100)]
        [Required(ErrorMessage ="Tên người dùng không được để trống")]
        
        public override string UserName { get; set; } 

        [MaxLength(100)]
        public override string NormalizedUserName { get; set; }

        [Required(ErrorMessage = "Email không được để trống")]
        [MaxLength(150)]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
            ErrorMessage = "Email không hợp lệ!")]
        public override string Email { get; set; }


        [MaxLength(15)]
        [Required(ErrorMessage ="Số điện thoại không được để trống")]
        [Phone(ErrorMessage ="Sai cấu trúc của số điện thoại")]
        public override string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Ngày sinh không được để trống.")]
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

          public byte[]? ProfileImg { get; set; } // Lưu ảnh dưới dạng BLOB


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
