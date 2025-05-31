using Expense_Tracker_App.Models;
using Expense_Tracker_App.Models.AccountViewModels;
using Expense_Tracker_App.Service;
using Microsoft.AspNetCore.Identity;

namespace Expense_Tracker_App.Service.Impl
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<IdentityResult> RegisterUserAsync(RegisterViewModel model)
        {
            var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
            return await _userManager.CreateAsync(user, model.Password);
        }

        public async Task<SignInResult> LoginUserAsync(LoginViewModel model)
        {
            return await _signInManager.PasswordSignInAsync(model.Email, model.Password, isPersistent: true, lockoutOnFailure: false);
        }

        public async Task LogoutUserAsync()
        {
            await _signInManager.SignOutAsync();
        }

        public async Task<IdentityResult> ChangePasswordAsync(ChangePasswordViewModel model, ApplicationUser user)
        {
            return await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
        }

        public async Task<ApplicationUser> GetCurrentUserAsync()
        {
            return await _userManager.GetUserAsync(_signInManager.Context.User);
        }

        public async Task<IdentityResult> UpdateUserProfileAsync(UpdateProfile model, ApplicationUser user)
        {
            user.FullName = model.FullName;
            user.Email = model.Email;
            user.NormalizedEmail = model.Email.ToUpper();
            user.UserName = model.Email;
            user.NormalizedUserName = model.Email.ToUpper();
            user.PhoneNumber = model.PhoneNumber;
            user.DateOfBirth = model.DateOfBirth;

            if (model.ProfileImg != null && model.ProfileImg.Length > 0)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await model.ProfileImg.CopyToAsync(memoryStream);
                    user.ProfileImg = memoryStream.ToArray();
                }
            }

            return await _userManager.UpdateAsync(user);
        }

        public async Task<bool> IsUserSignedInAsync()
        {
            return _signInManager.IsSignedIn(_signInManager.Context.User);
        }
    }
}