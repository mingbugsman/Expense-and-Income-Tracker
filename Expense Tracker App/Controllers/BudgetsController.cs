using Microsoft.AspNetCore.Mvc;
using Expense_Tracker_App.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Expense_Tracker_App.Service;

namespace Expense_Tracker_App.Controllers
{
    [Authorize]
    public class BudgetsController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IBudgetService _budgetService;

        public BudgetsController(UserManager<ApplicationUser> userManager, IBudgetService budgetService)
        {
            _userManager = userManager;
            _budgetService = budgetService;
        }

        private string GetUserId() => _userManager.GetUserId(User);

        public async Task<IActionResult> Index(int? page, int pageSize = 10)
        {
            int pageNumber = page ?? 1;
            var pagedBudgets = await _budgetService.GetPagedBudgetsAsync(pageNumber, pageSize);
            var categories = await _budgetService.GetUserCategoriesAsync(GetUserId());

            ViewBag.Categories = categories;
            ViewBag.CategoryId = null;
            ViewBag.StartDate = null;
            ViewBag.EndDate = null;
            ViewBag.MinAmount = null;
            ViewBag.MaxAmount = null;

            return View(pagedBudgets);
        }

        public async Task<IActionResult> CreateOrEdit(int id = 0)
        {
            var categories = await _budgetService.GetUserCategoriesAsync(GetUserId());
            ViewBag.Categories = categories;

            if (id == 0)
                return View(new Budget());

            var budget = await _budgetService.GetBudgetByIdAsync(id, GetUserId());
            if (budget == null)
                return NotFound();

            return View(budget);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrEdit([Bind("BudgetID,BudgetAmount,Budget_StartDate,Budget_EndDate,UserId,CategoryId")] Budget budget)
        {
            budget.UserId = GetUserId();
            if (!ModelState.IsValid)
            {
                var categories = await _budgetService.GetUserCategoriesAsync(GetUserId());
                ViewBag.Categories = categories;
                return View(budget);
            }

            var success = await _budgetService.CreateOrUpdateBudgetAsync(budget);
            if (!success)
                return NotFound();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var success = await _budgetService.DeleteBudgetAsync(id, GetUserId());
            if (!success)
                return NotFound();

            return RedirectToAction(nameof(Index));
        }
    }
}
