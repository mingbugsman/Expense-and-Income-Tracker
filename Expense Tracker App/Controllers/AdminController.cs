using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Expense_Tracker_App.Models;
using Expense_Tracker_App.Data;
using Expense_Tracker_App.Models.ViewModels;
using Expense_Tracker_App.Service;

namespace Expense_Tracker_App.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IAdminService _adminService;

        public AdminController(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context,
            IAdminService adminService)
        {
            _userManager = userManager;
            _context = context;
            _adminService = adminService;
        }

        public async Task<IActionResult> Dashboard()
        {
            try
            {
                ViewData["Title"] = "Admin Dashboard";
                var summary = await _adminService.GetDashboardSummaryAsync();
                ViewBag.TotalUsers = summary.TotalUsers;
                ViewBag.TotalTransactions = summary.TotalTransactions;
                ViewBag.TotalCategories = summary.TotalCategories;
                ViewBag.TotalBudgets = summary.TotalBudgets;
                ViewBag.RecentUsers = summary.RecentUsers;
                ViewBag.RecentTransactions = summary.RecentTransactions;
                return View();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Dashboard action: {ex.Message}");
                ViewBag.TotalUsers = 0;
                ViewBag.TotalTransactions = 0;
                ViewBag.TotalCategories = 0;
                ViewBag.TotalBudgets = 0;
                ViewBag.RecentUsers = new List<ApplicationUser>();
                ViewBag.RecentTransactions = new List<Transaction>();
                return View();
            }
        }

        public async Task<IActionResult> Users(string searchTerm = "", string? role = null, int page = 1, int pageSize = 10)
        {
            try
            {
                ViewData["Title"] = "User Management";
                ViewData["CurrentSearch"] = searchTerm;
                ViewData["SelectedRole"] = role;
                var userRoles = await _adminService.GetUsersAsync(searchTerm, role, page, pageSize);
                ViewBag.TotalPages = (int)Math.Ceiling(userRoles.Count / (double)pageSize);
                ViewBag.CurrentPage = page;
                ViewBag.Roles = new List<string> { "Admin", "User" };
                return View(userRoles);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Users action: {ex.Message}");
                return View(new List<UserRoleViewModel>());
            }
        }

        public async Task<IActionResult> Categories(string searchTerm = "", int page = 1, int pageSize = 10)
        {
            ViewData["Title"] = "Category Management";
            ViewData["CurrentSearch"] = searchTerm;
            var result = await _adminService.GetCategoriesAsync(searchTerm, page, pageSize);
            ViewBag.TotalPages = result.TotalPages;
            ViewBag.CurrentPage = result.CurrentPage;
            return View(result.Categories);
        }

        public async Task<IActionResult> Transactions(string searchTerm = "", int? categoryId = null, DateTime? startDate = null, DateTime? endDate = null, decimal? minAmount = null, decimal? maxAmount = null, int page = 1, int pageSize = 10)
        {
            ViewData["Title"] = "Transaction Management";
            ViewData["CurrentSearch"] = searchTerm;
            ViewData["SelectedCategory"] = categoryId;
            ViewData["StartDate"] = startDate?.ToString("yyyy-MM-dd");
            ViewData["EndDate"] = endDate?.ToString("yyyy-MM-dd");
            ViewData["MinAmount"] = minAmount;
            ViewData["MaxAmount"] = maxAmount;
            var result = await _adminService.GetTransactionsAsync(searchTerm, categoryId, startDate, endDate, minAmount, maxAmount, page, pageSize);
            ViewBag.TotalPages = result.TotalPages;
            ViewBag.CurrentPage = result.CurrentPage;
            ViewBag.Categories = result.Categories;
            ViewBag.Users = result.Users;
            return View(result.Transactions);
        }

        public async Task<IActionResult> Budgets(string searchTerm = "", int? categoryId = null, string? userId = null, DateTime? startDate = null, DateTime? endDate = null, decimal? minAmount = null, decimal? maxAmount = null, int page = 1, int pageSize = 10)
        {
            try
            {
                ViewData["Title"] = "Budget Management";
                ViewData["CurrentSearch"] = searchTerm;
                ViewData["SelectedCategory"] = categoryId;
                ViewData["SelectedUser"] = userId;
                ViewData["StartDate"] = startDate?.ToString("yyyy-MM-dd");
                ViewData["EndDate"] = endDate?.ToString("yyyy-MM-dd");
                ViewData["MinAmount"] = minAmount;
                ViewData["MaxAmount"] = maxAmount;
                var result = await _adminService.GetBudgetsAsync(searchTerm, categoryId, userId, startDate, endDate, minAmount, maxAmount, page, pageSize);
                ViewBag.TotalPages = result.TotalPages;
                ViewBag.CurrentPage = result.CurrentPage;
                ViewBag.Users = result.Users;
                ViewBag.Categories = result.Categories;
                return View(result.Budgets);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Budgets action: {ex.Message}");
                ViewBag.Users = new List<ApplicationUser>();
                ViewBag.Categories = new List<Category>();
                return View(new List<Budget>());
            }
        }

        [HttpGet]
        public IActionResult Reports(string filterType = "month", DateTime? startDate = null, DateTime? endDate = null)
        {
            ViewData["Title"] = "Reports";
            var model = _adminService.GetReportAsync(filterType, startDate, endDate).Result;
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleUserRole(string userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            if (await _userManager.IsInRoleAsync(user, role))
            {
                await _userManager.RemoveFromRoleAsync(user, role);
            }
            else
            {
                await _userManager.AddToRoleAsync(user, role);
            }

            return RedirectToAction(nameof(Users));
        }

        [HttpPost]
        public IActionResult AddCategory(Category category)
        {
            if (ModelState.IsValid)
            {
                _context.Categories.Add(category);
                _context.SaveChanges();
                return RedirectToAction(nameof(Categories));
            }
            return View(category);
        }

        [HttpPost]
        public IActionResult EditCategory(Category category)
        {
            if (ModelState.IsValid)
            {
                _context.Categories.Update(category);
                _context.SaveChanges();
                return RedirectToAction(nameof(Categories));
            }
            return View(category);
        }

        [HttpPost]
        public IActionResult DeleteCategory(int id)
        {
            var category = _context.Categories.Find(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(Categories));
        }

        [HttpGet]
        public IActionResult GetCategory(int id)
        {
            var category = _context.Categories.Find(id);
            if (category == null)
            {
                return NotFound();
            }
            return Json(category);
        }

        [HttpPost]
        public IActionResult AddTransaction(Transaction transaction)
        {
            if (ModelState.IsValid)
            {
                _context.Transactions.Add(transaction);
                _context.SaveChanges();
                return RedirectToAction(nameof(Transactions));
            }
            return View(transaction);
        }

        [HttpPost]
        public IActionResult EditTransaction(Transaction transaction)
        {
            if (ModelState.IsValid)
            {
                _context.Transactions.Update(transaction);
                _context.SaveChanges();
                return RedirectToAction(nameof(Transactions));
            }
            return View(transaction);
        }

        [HttpPost]
        public IActionResult DeleteTransaction(int id)
        {
            var transaction = _context.Transactions.Find(id);
            if (transaction != null)
            {
                _context.Transactions.Remove(transaction);
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(Transactions));
        }

        [HttpGet]
        public IActionResult GetTransaction(int id)
        {
            var transaction = _context.Transactions.Find(id);
            if (transaction == null)
            {
                return NotFound();
            }
            return Json(transaction);
        }
    }
}