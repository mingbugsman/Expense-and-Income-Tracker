using Expense_Tracker_App.Models;

namespace Expense_Tracker_App.Models.ViewModels
{
    public class UserRoleViewModel
    {
        public ApplicationUser User { get; set; }
        public List<string> Roles { get; set; }
    }
} 