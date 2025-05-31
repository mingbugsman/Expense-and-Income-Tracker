using Expense_Tracker_App.Data;
using Expense_Tracker_App.Models;
using Expense_Tracker_App.Models.AccountViewModels;
using Expense_Tracker_App.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Expense_Tracker_App.Service;
using Microsoft.EntityFrameworkCore;

namespace Expense_Tracker_App.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;
        private readonly IAccountService _accountService;
        private readonly IEmailService _emailService;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration,
            ApplicationDbContext context,
            IAccountService accountService,
            IEmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _context = context;
            _accountService = accountService;
            _emailService = emailService;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            // Get statistics
            ViewBag.TotalTransactions = await _context.Transactions
                .Where(t => t.UserId == user.Id)
                .CountAsync();

            ViewBag.TotalBudgets = await _context.Budgets
                .Where(b => b.UserId == user.Id)
                .CountAsync();

            ViewBag.TotalCategories = await _context.Categories
                .Where(c => c.UserId == user.Id)
                .CountAsync();

            return View();
        }
        

        // GET: Account/CompleteProfile
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> CompleteProfile()
        {
            var user = await _accountService.GetCurrentUserAsync();
            if (user == null)
            {
                return NotFound();
            }

            var model = new UpdateProfile
            {
                FullName = user.FullName ?? "",
                Email = user.Email ?? "",
                PhoneNumber = user.PhoneNumber ?? "",
                DateOfBirth = user.DateOfBirth.GetValueOrDefault(DateTime.Today),
                ProfileImgBase64 = user.ProfileImg != null ? Convert.ToBase64String(user.ProfileImg) : null
            };
    

            return View(model);
        }

        // POST: Account/CompleteProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteProfile([Bind("FullName,Email,PhoneNumber,DateOfBirth,ProfileImg")] UpdateProfile model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _accountService.GetCurrentUserAsync();
            if (user == null)
            {
                return NotFound();
            }

            var result = await _accountService.UpdateUserProfileAsync(model, user);
            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user);
                return RedirectToAction("Index", "DashBoard");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }


        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string? returnUrl = null)
        {
            if (await _accountService.IsUserSignedInAsync())
            {
                return RedirectToAction("Index", "DashBoard");
            }
            
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            if (await _accountService.IsUserSignedInAsync())
            {
                return RedirectToAction("Index", "Dashboard");
            }
            
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var result = await _accountService.LoginUserAsync(model);
                if (result.Succeeded)
                {
                    return RedirectToLocal(returnUrl);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return View(model);
                }
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _accountService.LogoutUserAsync();
            return RedirectToAction(nameof(AccountController.Login), "Account");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register(string returnUrl = null)
        {
            if (_accountService.IsUserSignedInAsync().Result)
            {
                return RedirectToAction("Index", "Dashboard");
            }
            
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string returnUrl = null)
        {
            if (await _accountService.IsUserSignedInAsync())
            {
                return RedirectToAction("Index", "Dashboard");
            }
            
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var result = await _accountService.RegisterUserAsync(model);
                if (result.Succeeded)
                {
                    var user = await _userManager.FindByEmailAsync(model.Email);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var callbackUrl = Url.Action(
                        "ConfirmEmail",
                        "Account",
                        new { userId = user.Id, code = code },
                        protocol: Request.Scheme);

                    await _emailService.SendEmailAsync(user.Email, "Confirm your email", callbackUrl);
                    return RedirectToAction(nameof(RegisterConfirmation));
                }
                AddErrors(result);
            }
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult RegisterConfirmation()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{userId}'.");
            }

            var result = await _userManager.ConfirmEmailAsync(user, code);
            if (result.Succeeded)
            {
                // Sign in the user after email confirmation
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("CompleteProfile", "Account");
            }

            return View("Error");
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(HomeController.Index), "Dashboard");
            }
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    return RedirectToAction(nameof(ForgotPasswordConfirmation));
                }

                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                var callbackUrl = Url.Action(
                    "ResetPassword",
                    "Account",
                    new { email = user.Email, code = code },
                    protocol: Request.Scheme);

                // Prepare email template data
                ViewData["ResetLink"] = callbackUrl;
                ViewData["UserName"] = user.UserName;

                // Render email template
                var emailBody = await ViewExtensions.RenderViewAsync<string>(
                    this,
                    "EmailTemplates/ResetPasswordEmail",
                    callbackUrl,
                    false
                );

                // Send confirmation email
                await _emailService.SendEmailAsync(
                    user.Email,
                    "Reset Password",
                    emailBody,
                    true);

                return RedirectToAction(nameof(ForgotPasswordConfirmation));
            }

            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string code = null)
        {
            if (code == null)
            {
                return BadRequest("A code must be supplied for password reset.");
            }
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            Console.WriteLine(model.Code);
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            Console.WriteLine("FOUND USER");
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        [HttpGet]
        [Authorize]
        public IActionResult ChangeEmail()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeEmail(ChangeEmailViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            // Check if new email is already taken
            var existingUser = await _userManager.FindByEmailAsync(model.NewEmail);
            if (existingUser != null)
            {
                ModelState.AddModelError(string.Empty, "This email is already taken.");
                return View(model);
            }

            // Generate email change token
            var code = await _userManager.GenerateChangeEmailTokenAsync(user, model.NewEmail);
            var callbackUrl = Url.Action(
                "ConfirmEmailChange",
                "Account",
                new { userId = user.Id, email = model.NewEmail, code = code },
                protocol: Request.Scheme);

            // Prepare email template data
            ViewData["ConfirmationLink"] = callbackUrl;
            ViewData["EmailType"] = "ChangeEmail";
            ViewData["UserName"] = user.UserName;

            // Render email template
            var emailBody = await ViewExtensions.RenderViewAsync<string>(
             this,
             "EmailTemplates/EmailConfirmation",
             callbackUrl,
             false
            );
            // Send confirmation email
            await _emailService.SendEmailAsync(
                model.NewEmail,
                "Confirm your new email",
                emailBody,
                true);

            return RedirectToAction(nameof(EmailChangeConfirmation));
        }

        [HttpGet]
        [Authorize]
        public IActionResult EmailChangeConfirmation()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmailChange(string userId, string email, string code)
        {
            if (userId == null || email == null || code == null)
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{userId}'.");
            }

            var result = await _userManager.ChangeEmailAsync(user, email, code);
            if (result.Succeeded)
            {
                // Update username to match new email
                user.UserName = email;
                user.NormalizedUserName = email.ToUpper();
                user.NormalizedEmail = email.ToUpper();
                
                // Update user in database
                var updateResult = await _userManager.UpdateAsync(user);
                if (updateResult.Succeeded)
                {
                    // Sign out the user
                    await _signInManager.SignOutAsync();
                    
                    // Sign in with new email
                    await _signInManager.SignInAsync(user, isPersistent: true);
                    
                    return RedirectToAction(nameof(Profile));
                }
                else
                {
                    foreach (var error in updateResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View("Error");
                }
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View("Error");
        }

        [HttpGet]
        [Authorize]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user);
                return RedirectToAction(nameof(Profile));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> EditUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var model = new UpdateProfile
            {
                FullName = user.FullName ?? "",
                Email = user.Email ?? "",
                PhoneNumber = user.PhoneNumber ?? "",
                DateOfBirth = user.DateOfBirth.GetValueOrDefault(DateTime.Today),
                ProfileImgBase64 = user.ProfileImg != null ? Convert.ToBase64String(user.ProfileImg) : null
            };

            return View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(string id, UpdateProfile model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var result = await _accountService.UpdateUserProfileAsync(model, user);
            if (result.Succeeded)
            {
                return RedirectToAction("Users", "Admin");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }
    }
}
