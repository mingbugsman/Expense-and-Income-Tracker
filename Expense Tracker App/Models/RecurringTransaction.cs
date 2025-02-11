using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Expense_Tracker_App.Models
{
    public class RecurringTransaction
    {
        [Key]
        public int RecurringTransactionId { get; set; }


        [Column(TypeName="decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        public DateTime StartDate { get; set; }= DateTime.Now;

        public DateTime? EndDate { get; set; } = DateTime.Now.AddDays(30);

        [Required]
        public string Frequency { get; set; } = "Monthly"; // e.g., "Monthly", "Weekly"

        [Required]
        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public Category? Category { get; set; }
            

        public string? UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        [NotMapped]
        public string? FormattedAmount
        {
            get
            {
                return ((Category == null || Category.Type == "Expense") ? "-" : "+") + Amount.ToString("n2");
            }
        }
    }

}
