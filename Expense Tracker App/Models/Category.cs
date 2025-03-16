using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Expense_Tracker_App.Models
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }


        [Column(TypeName = "nvarchar(50)")]
        public string Title { get; set; }


        [Column(TypeName = "nvarchar(10)")]
        public string Type { get; set; } = "Expense";


        // Thêm khóa ngoại đến ApplicationUser
        public string? UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

    }
}
