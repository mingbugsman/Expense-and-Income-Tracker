using System.ComponentModel.DataAnnotations;

namespace Expense_Tracker_App.Models.AccountViewModels
{
    public class ChangeEmailViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "New Email")]
        public string NewEmail { get; set; }
    }
} 