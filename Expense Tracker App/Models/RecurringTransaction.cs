using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Expense_Tracker_App.Models
{
    public enum FrequencyType
    {
        Daily,
        Weekly,
        Monthly,
        Yearly,
        Custom
    }

    public class RecurringTransaction
    {
        [Key]
        public int RecurringTransactionId { get; set; }

        [Column(TypeName="decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        public DateTime StartDate { get; set; } = DateTime.Now;

        public DateTime? EndDate { get; set; }

        [Required]
        public FrequencyType Frequency { get; set; } = FrequencyType.Monthly;

        // Custom interval in days (only used when Frequency is Custom)
        [Range(1, int.MaxValue, ErrorMessage = "Custom interval must be at least 1 day")]
        public int CustomIntervalDays { get; set; } = 0;

        [Required]
        [StringLength(75)]
        public string Note { get; set; }

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

        [NotMapped]
        public string? FrequencyString
        {
            get
            {
                return Frequency switch
                {
                    FrequencyType.Daily => "Daily",
                    FrequencyType.Weekly => "Weekly",
                    FrequencyType.Monthly => "Monthly",
                    FrequencyType.Yearly => "Yearly",
                    FrequencyType.Custom => $"Every {CustomIntervalDays} days",
                    _ => Frequency.ToString()
                };
            }
        }

        public bool IsValid()
        {
            if (Frequency == FrequencyType.Custom && CustomIntervalDays < 1)
            {
                return false;
            }

            if (EndDate.HasValue && EndDate.Value < StartDate)
            {
                return false;
            }

            return true;
        }
    }   
}
