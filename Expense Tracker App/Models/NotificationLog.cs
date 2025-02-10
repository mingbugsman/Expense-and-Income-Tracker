using Expense_Tracker_App.Enum;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Expense_Tracker_App.Models
{
    public class NotificationLog
    {
        [Key]
        public int Log_Id { get; set; }
        public string Message { get; set; } 

        public NotificationType Log_Type { get; set;}

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string? UserId;
        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }
    }
}
