using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Expense_Tracker_App.Data;
using Expense_Tracker_App.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Expense_Tracker_App.Service;
using X.PagedList;
using X.PagedList.Extensions;

namespace Expense_Tracker_App.Controllers
{
    [Authorize]
    public class BudgetsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IBudgetService _budgetService;

        public BudgetsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IBudgetService budgetService)
        {
            _context = context;
            _userManager = userManager;
            _budgetService = budgetService;
        }

        [NonAction]
        private void PopulateCategories()
        {
            var userId = GetUserId();
            var categories = _context.Categories
                .Where(c => c.UserId == userId)
                .ToList();

            categories.Insert(0, new Category { CategoryId = 0, Title = "Choose a category" });
            ViewBag.Categories = categories;
        }

        private string GetUserId() => _userManager.GetUserId(User);

        // GET: Budgets
        public async Task<IActionResult> Index(int? page, int pageSize = 10)
        {
            var budgets = await _context.Budgets
                .Include(b => b.Category)
                .OrderByDescending(b => b.Budget_StartDate)
                .ToListAsync();
            int pageNumber = page ?? 1;
            var pagedBudgets = budgets.ToPagedList(pageNumber, pageSize);

            PopulateCategories();
            ViewBag.CategoryId = null;
            ViewBag.StartDate = null;
            ViewBag.EndDate = null;
            ViewBag.MinAmount = null;
            ViewBag.MaxAmount = null;

            return View(pagedBudgets);
        }

        // GET: Budgets/Create
        public IActionResult CreateOrEdit(int id = 0)
        {
            PopulateCategories();
            if (id == 0)
                return View(new Budget());

            var budget = _context.Budgets
                .FirstOrDefault(t => t.BudgetID == id && t.UserId == GetUserId());
            if (budget == null)
                return NotFound();

            return View(budget);
        }

        // POST: Budgets/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrEdit([Bind("BudgetID,BudgetAmount," +
                                                    "Budget_StartDate,Budget_EndDate,UserId,CategoryId")] 
                                                    Budget budget)
        {
            budget.UserId = GetUserId();
            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState)
                {
                    foreach (var subError in error.Value.Errors)
                    {
                        Console.WriteLine($"Field: {error.Key}, Error: {subError.ErrorMessage}");
                    }
                }
                PopulateCategories();
                return View(budget);
            }

            if (budget.BudgetID == 0)
                _context.Add(budget);
            else
            {
                var existingBudget = await _context.Budgets
                    .FirstOrDefaultAsync(t => t.BudgetID == budget.BudgetID && t.UserId == GetUserId());

                if (existingBudget == null)
                    return NotFound();

                _context.Entry(existingBudget).CurrentValues.SetValues(budget);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // POST: Budgets/Delete/5
        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var budget = await _context.Budgets
                .FirstOrDefaultAsync(t => t.BudgetID == id && t.UserId == GetUserId());
            if (budget == null)
                return NotFound();

            _context.Budgets.Remove(budget);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
