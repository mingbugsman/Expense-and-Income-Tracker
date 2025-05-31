using System.ComponentModel.DataAnnotations;

namespace Expense_Tracker_App.Models.AccountViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
} 