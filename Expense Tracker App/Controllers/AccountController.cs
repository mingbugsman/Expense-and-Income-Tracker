using Expense_Tracker_App.Data;
using Expense_Tracker_App.Models;
using Expense_Tracker_App.Models.AccountViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace Expense_Tracker_App.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _context = context;
        }


        // GET: Account/CompleteProfile
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> CompleteProfile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var model = new UpdateProfile
            {
                UserName = user.UserName ?? "",
                Email = user.Email ?? "",
                PhoneNumber = user.PhoneNumber ?? "",
                DateOfBirth = user.DateOfBirth.GetValueOrDefault(DateTime.Today)
            };
    

            return View(model);
        }

        // POST: Account/CompleteProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteProfile([Bind("UserName,Email,PhoneNumber,DateOfBirth,ProfileImg")] UpdateProfile model)
        {
            if (!ModelState.IsValid)
            {
                foreach (var err in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine(err.ErrorMessage);
                }
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }
            if (!Regex.IsMatch(model.Email, @"^.*[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$"))
            {
                ModelState.AddModelError("Email", "Email không hợp lệ!");
            }


            user.UserName = model.UserName;
            user.NormalizedUserName = model.UserName.ToUpper();
            user.Email = model.Email;
            user.NormalizedEmail = model.Email.ToUpper();
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

            await Console.Out.WriteLineAsync(user.UserName);
            await Console.Out.WriteLineAsync(user.Email);
            await Console.Out.WriteLineAsync(user.PhoneNumber);
            await Console.Out.WriteLineAsync(user.DateOfBirth.ToString());
            var result = await _userManager.UpdateAsync(user);
            await _context.SaveChangesAsync();
            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user);
                return RedirectToAction("Index", "DashBoard");
            }

            else
            {
                Console.WriteLine("User update failed!");
                foreach (var error in result.Errors)
                {
                    Console.WriteLine($"Error: {error.Description}");
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }


        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string? returnUrl = null)
        {
            if (_signInManager.IsSignedIn(User))
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
            if (_signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Index", "Dashboard");
            }
            
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, isPersistent: true, lockoutOnFailure: false);
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
            await _signInManager.SignOutAsync();
            return RedirectToAction(nameof(AccountController.Login), "Account");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register(string returnUrl = null)
        {
            if (_signInManager.IsSignedIn(User))
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
            if (_signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Index", "Dashboard");
            }
            
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("SetUpProfile", "Account");
                }
                AddErrors(result);
            }
            return View(model);
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
    }
}
