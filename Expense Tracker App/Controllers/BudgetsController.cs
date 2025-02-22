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

namespace Expense_Tracker_App.Controllers
{
    public class BudgetsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public BudgetsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
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
        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();
            var budgets = _context.Budgets
                .Include(t => t.Category)

                .Where(t => t.UserId == userId);

            foreach (var item in budgets)
            {
                Console.WriteLine(item.UserId);
                Console.WriteLine(item.BudgetID);
                Console.WriteLine(item.BudgetAmount);
            }

            return View(await budgets.ToListAsync());
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
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrEdit([Bind("BudgetID,BudgetAmount," +
                                                    "Budget_StartDate,Budget_EndDate,UserId,CategoryId")] 
                                                    Budget budget)
        {
            budget.UserId = GetUserId(); // Đảm bảo chỉ thao tác với dữ liệu của user hiện tại
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
                var existingTransaction = _context.Transactions
                    .FirstOrDefault(t => t.TransactionId == budget.BudgetID && t.UserId == GetUserId());

                if (existingTransaction == null)
                    return NotFound();

                _context.Entry(existingTransaction).CurrentValues.SetValues(budget);
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
