using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Expense_Tracker_App.Models
{
    public class Transaction
    {
        [Key]
        public int TransactionId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Column(TypeName = "nvarchar(75)")]
        public string? Note { get; set; }
        public DateTime TransactionDate { get; set; } =  DateTime.Now;

        public byte[]? BillImage { get; set; } // BLOB 


        // CATEGORY ID
        public int CategoryId { get; set; }
        public Category? Category { get; set; }


        // Thêm khóa ngoại đến ApplicationUser
        public string? UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }
        [NotMapped]
        public string? FormattedAmount
        {
            get
            {
                return ((Category == null || Category.Type == "Expense") ? "-" : "+" ) + Amount.ToString("n2");
            }
        }
    }
}
