using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Expense_Tracker_App.Models;
using Expense_Tracker_App.Data;
using Expense_Tracker_App.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Expense_Tracker_App.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public AdminController(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Dashboard()
        {
            try
            {
                ViewData["Title"] = "Admin Dashboard";

                // Get total counts
                ViewBag.TotalUsers = await _context.Users.CountAsync();
                ViewBag.TotalTransactions = await _context.Transactions.CountAsync();
                ViewBag.TotalCategories = await _context.Categories.CountAsync();
                ViewBag.TotalBudgets = await _context.Budgets.CountAsync();

                // Get recent users
                ViewBag.RecentUsers = await _context.Users
                    .OrderByDescending(u => u.Id)
                    .Take(5)
                    .Select(u => new ApplicationUser
                    {
                        Id = u.Id,
                        UserName = u.UserName ?? string.Empty,
                        Email = u.Email ?? string.Empty
                    })
                    .ToListAsync();

                // Get recent transactions
                ViewBag.RecentTransactions = await _context.Transactions
                    .Include(t => t.Category)
                    .Include(t => t.User)
                    .OrderByDescending(t => t.TransactionDate)
                    .Take(5)
                    .Select(t => new Transaction
                    {
                        TransactionId = t.TransactionId,
                        Note = t.Note ?? string.Empty,
                        Amount = t.Amount,
                        TransactionDate = t.TransactionDate,
                        Category = t.Category != null ? new Category { Title = t.Category.Title ?? string.Empty } : null,
                        User = t.User != null ? new ApplicationUser { UserName = t.User.UserName ?? string.Empty } : null
                    })
                    .ToListAsync();

                return View();
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"Error in Dashboard action: {ex.Message}");
                
                // Initialize with default values
                ViewBag.TotalUsers = 0;
                ViewBag.TotalTransactions = 0;
                ViewBag.TotalCategories = 0;
                ViewBag.TotalBudgets = 0;
                ViewBag.RecentUsers = new List<ApplicationUser>();
                ViewBag.RecentTransactions = new List<Transaction>();
                
                return View();
            }
        }

        public async Task<IActionResult> Users(
            string searchTerm = "", 
            string? role = null,
            int page = 1, 
            int pageSize = 10)
        {
            try
            {
                ViewData["Title"] = "User Management";
                ViewData["CurrentSearch"] = searchTerm;
                ViewData["SelectedRole"] = role;
                
                var query = _context.Users.AsQueryable();
                
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    searchTerm = searchTerm.ToLower();
                    query = query.Where(u => 
                        (u.UserName != null && u.UserName.ToLower().Contains(searchTerm)) ||
                        (u.Email != null && u.Email.ToLower().Contains(searchTerm)) ||
                        (u.FullName != null && u.FullName.ToLower().Contains(searchTerm))
                    );
                }

                var totalUsers = await query.CountAsync();
                var users = await query
                    .OrderBy(u => u.UserName)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(u => new ApplicationUser
                    {
                        Id = u.Id,
                        UserName = u.UserName ?? string.Empty,
                        Email = u.Email ?? string.Empty,
                        PhoneNumber = u.PhoneNumber ?? string.Empty,
                        FullName = u.FullName ?? string.Empty
                    })
                    .ToListAsync();

                var userRoles = new List<UserRoleViewModel>();

                foreach (var user in users)
                {
                    try
                    {
                        var roles = await _userManager.GetRolesAsync(user);
                        if (string.IsNullOrEmpty(role) || roles.Contains(role))
                        {
                            userRoles.Add(new UserRoleViewModel
                            {
                                User = user,
                                Roles = roles?.ToList() ?? new List<string>()
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error getting roles for user {user.Id}: {ex.Message}");
                        userRoles.Add(new UserRoleViewModel
                        {
                            User = user,
                            Roles = new List<string>()
                        });
                    }
                }

                ViewBag.TotalPages = (int)Math.Ceiling(totalUsers / (double)pageSize);
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

        public async Task<IActionResult> Categories(
            string searchTerm = "", 
            int page = 1, 
            int pageSize = 10)
        {
            ViewData["Title"] = "Category Management";
            ViewData["CurrentSearch"] = searchTerm;

            var query = _context.Categories.AsQueryable();
            
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(c => 
                    (c.Title != null && c.Title.ToLower().Contains(searchTerm))
                );
            }

            var totalCategories = await query.CountAsync();
            var categories = await query
                .OrderBy(c => c.Title)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.TotalPages = (int)Math.Ceiling(totalCategories / (double)pageSize);
            ViewBag.CurrentPage = page;

            return View(categories);
        }

        public async Task<IActionResult> Transactions(
            string searchTerm = "", 
            int? categoryId = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            decimal? minAmount = null,
            decimal? maxAmount = null,
            int page = 1, 
            int pageSize = 10)
        {
            ViewData["Title"] = "Transaction Management";
            ViewData["CurrentSearch"] = searchTerm;
            ViewData["SelectedCategory"] = categoryId;
            ViewData["StartDate"] = startDate?.ToString("yyyy-MM-dd");
            ViewData["EndDate"] = endDate?.ToString("yyyy-MM-dd");
            ViewData["MinAmount"] = minAmount;
            ViewData["MaxAmount"] = maxAmount;

            var query = _context.Transactions
                .Include(t => t.Category)
                .Include(t => t.User)
                .AsQueryable();
            
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(t => 
                    (t.Note != null && t.Note.ToLower().Contains(searchTerm)) ||
                    (t.Category != null && t.Category.Title != null && t.Category.Title.ToLower().Contains(searchTerm)) ||
                    (t.User != null && t.User.UserName != null && t.User.UserName.ToLower().Contains(searchTerm))
                );
            }

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(t => t.CategoryId == categoryId.Value);
            }

            if (startDate.HasValue)
            {
                query = query.Where(t => t.TransactionDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(t => t.TransactionDate <= endDate.Value);
            }

            if (minAmount.HasValue)
            {
                query = query.Where(t => t.Amount >= minAmount.Value);
            }

            if (maxAmount.HasValue)
            {
                query = query.Where(t => t.Amount <= maxAmount.Value);
            }

            var totalTransactions = await query.CountAsync();
            var transactions = await query
                .OrderByDescending(t => t.TransactionDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.TotalPages = (int)Math.Ceiling(totalTransactions / (double)pageSize);
            ViewBag.CurrentPage = page;
            ViewBag.Categories = await _context.Categories.ToListAsync();
            ViewBag.Users = await _context.Users
                .Select(u => new ApplicationUser
                {
                    Id = u.Id,
                    UserName = u.UserName ?? string.Empty,
                    Email = u.Email ?? string.Empty
                })
                .ToListAsync();

            return View(transactions);
        }

        public async Task<IActionResult> Budgets(
            string searchTerm = "", 
            int? categoryId = null,
            string? userId = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            decimal? minAmount = null,
            decimal? maxAmount = null,
            int page = 1, 
            int pageSize = 10)
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

                var query = _context.Budgets
                    .Include(b => b.Category)
                    .Include(b => b.User)
                    .AsQueryable();
                
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    searchTerm = searchTerm.ToLower();
                    query = query.Where(b => 
                        (b.User != null && b.User.UserName != null && b.User.UserName.ToLower().Contains(searchTerm)) ||
                        (b.Category != null && b.Category.Title != null && b.Category.Title.ToLower().Contains(searchTerm))
                    );
                }

                if (categoryId.HasValue && categoryId.Value > 0)
                {
                    query = query.Where(b => b.CategoryId == categoryId.Value);
                }

                if (!string.IsNullOrWhiteSpace(userId))
                {
                    query = query.Where(b => b.UserId == userId);
                }

                if (startDate.HasValue)
                {
                    query = query.Where(b => b.Budget_StartDate >= startDate.Value);
                }

                if (endDate.HasValue)
                {
                    query = query.Where(b => b.Budget_EndDate <= endDate.Value);
                }

                if (minAmount.HasValue)
                {
                    query = query.Where(b => b.BudgetAmount >= minAmount.Value);
                }

                if (maxAmount.HasValue)
                {
                    query = query.Where(b => b.BudgetAmount <= maxAmount.Value);
                }

                var totalBudgets = await query.CountAsync();
                var budgets = await query
                    .OrderByDescending(b => b.Budget_StartDate)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                ViewBag.TotalPages = (int)Math.Ceiling(totalBudgets / (double)pageSize);
                ViewBag.CurrentPage = page;

                ViewBag.Users = await _context.Users
                    .Select(u => new ApplicationUser
                    {
                        Id = u.Id,
                        UserName = u.UserName ?? string.Empty,
                        Email = u.Email ?? string.Empty
                    })
                    .ToListAsync();

                ViewBag.Categories = await _context.Categories
                    .Select(c => new Category
                    {
                        CategoryId = c.CategoryId,
                        Title = c.Title ?? string.Empty
                    })
                    .ToListAsync();

                return View(budgets);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Budgets action: {ex.Message}");
                ViewBag.Users = new List<ApplicationUser>();
                ViewBag.Categories = new List<Category>();
                return View(new List<Budget>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddBudget(Budget budget)
        {
            if (ModelState.IsValid)
            {
                _context.Budgets.Add(budget);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Budgets));
            }
            return View(budget);
        }

        [HttpPost]
        public async Task<IActionResult> EditBudget(Budget budget)
        {
            if (ModelState.IsValid)
            {
                _context.Budgets.Update(budget);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Budgets));
            }
            return View(budget);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteBudget(int id)
        {
            var budget = await _context.Budgets.FindAsync(id);
            if (budget != null)
            {
                _context.Budgets.Remove(budget);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Budgets));
        }

        [HttpGet]
        public async Task<IActionResult> GetBudget(int id)
        {
            var budget = await _context.Budgets.FindAsync(id);
            if (budget == null)
            {
                return NotFound();
            }
            return Json(budget);
        }

        [HttpGet]
        public IActionResult Reports(string filterType = "month", DateTime? startDate = null, DateTime? endDate = null)
        {
            ViewData["Title"] = "Reports";
            // Xác định khoảng thời gian
            var now = DateTime.Now;
            startDate ??= filterType == "year" ? new DateTime(now.Year - 4, 1, 1) :
                        filterType == "month" ? new DateTime(now.Year, 1, 1) :
                        now.Date.AddDays(-29); // 30 ngày gần nhất
            endDate ??= now.Date.AddDays(1);

            var labels = new List<string>();
            var transactionCounts = new List<int>();
            var activeUserCounts = new List<int>();

            if (filterType == "day")
            {
                // Theo ngày trong khoảng
                for (var date = startDate.Value.Date; date < endDate.Value.Date; date = date.AddDays(1))
                {
                    labels.Add(date.ToString("dd/MM/yyyy"));
                    var count = _context.Transactions.Count(t => t.TransactionDate.Date == date);
                    transactionCounts.Add(count);
                    var activeUsers = _context.Transactions.Where(t => t.TransactionDate.Date == date).Select(t => t.UserId).Distinct().Count();
                    activeUserCounts.Add(activeUsers);
                }
            }
            else if (filterType == "month")
            {
                // Theo tháng trong khoảng
                var start = new DateTime(startDate.Value.Year, startDate.Value.Month, 1);
                var end = new DateTime(endDate.Value.Year, endDate.Value.Month, 1);
                for (var date = start; date <= end; date = date.AddMonths(1))
                {
                    labels.Add(date.ToString("MM/yyyy"));
                    var nextMonth = date.AddMonths(1);
                    var count = _context.Transactions.Count(t => t.TransactionDate >= date && t.TransactionDate < nextMonth);
                    transactionCounts.Add(count);
                    var activeUsers = _context.Transactions.Where(t => t.TransactionDate >= date && t.TransactionDate < nextMonth).Select(t => t.UserId).Distinct().Count();
                    activeUserCounts.Add(activeUsers);
                }
            }
            else if (filterType == "year")
            {
                // Theo năm trong khoảng
                for (var year = startDate.Value.Year; year <= endDate.Value.Year; year++)
                {
                    labels.Add(year.ToString());
                    var yearStart = new DateTime(year, 1, 1);
                    var yearEnd = yearStart.AddYears(1);
                    var count = _context.Transactions.Count(t => t.TransactionDate >= yearStart && t.TransactionDate < yearEnd);
                    transactionCounts.Add(count);
                    var activeUsers = _context.Transactions.Where(t => t.TransactionDate >= yearStart && t.TransactionDate < yearEnd).Select(t => t.UserId).Distinct().Count();
                    activeUserCounts.Add(activeUsers);
                }
            }

            var model = new ReportViewModel
            {
                FilterLabels = labels,
                TransactionCounts = transactionCounts,
                ActiveUserCounts = activeUserCounts,
                FilterType = filterType,
                StartDate = startDate,
                EndDate = endDate
            };
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