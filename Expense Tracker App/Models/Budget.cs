using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Expense_Tracker_App.Models
{
    public class Budget
    {
        [Key]
        public int BudgetID { get; set; }

        [Column(TypeName = "decimal(18,2)")]

        // Limited amount for category.
        public decimal BudgetAmount { get; set; }

        [Column(TypeName = "DATETIME")]
        public DateTime Budget_StartDate { get; set; }

        [Column(TypeName = "DATETIME")]
        public DateTime Budget_EndDate { get; set; }


        // Thêm khóa ngoại đến ApplicationUser
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public Category? Category { get; set; }


    }
}
