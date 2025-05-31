using Expense_Tracker_App.Models;
using Expense_Tracker_App.Models.AccountViewModels;
using Microsoft.AspNetCore.Identity;

namespace Expense_Tracker_App.Service
{
    public interface IAccountService
    {
        Task<IdentityResult> RegisterUserAsync(RegisterViewModel model);
        Task<SignInResult> LoginUserAsync(LoginViewModel model);
        Task LogoutUserAsync();
        Task<IdentityResult> ChangePasswordAsync(ChangePasswordViewModel model, ApplicationUser user);
        Task<ApplicationUser> GetCurrentUserAsync();
        Task<IdentityResult> UpdateUserProfileAsync(UpdateProfile model, ApplicationUser user);
        Task<bool> IsUserSignedInAsync();
    }
}