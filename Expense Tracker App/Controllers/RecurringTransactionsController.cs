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
    public class RecurringTransactionsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public RecurringTransactionsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private string GetUserId() => _userManager.GetUserId(User);

        // GET: RecurringTransactions
        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();
            var applicationDbContext = _context.RecurringTransactions
                .Include(r => r.Category)
                .Where(t => t.UserId == userId);
            return View(await applicationDbContext.ToListAsync());
        }


        // GET: RecurringTransactions/CreateOrEdit
        public async Task<IActionResult> CreateOrEdit(int id = 0)
        {
            PopulateCategories();
            if (id == 0)
            {
                return View(new RecurringTransaction());
            }
            var recurringTransaction = await _context.RecurringTransactions
                .FirstOrDefaultAsync(t => t.RecurringTransactionId == id && t.UserId == GetUserId());
            if (recurringTransaction == null)
            {
                NotFound();
            }

            return View(recurringTransaction);
        }

        // POST: RecurringTransactions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrEdit([Bind("RecurringTransactionId,Amount,StartDate,EndDate,Frequency,CategoryId,UserId")] RecurringTransaction recurringTransaction)
        {
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
                return View(recurringTransaction);
            }
            recurringTransaction.UserId = GetUserId();
            if (recurringTransaction.RecurringTransactionId == 0)
            {
                _context.Add(recurringTransaction);
            } else
            {
                var existingRecurringTransaction = await _context.RecurringTransactions
                    .FirstOrDefaultAsync(t => t.RecurringTransactionId ==  recurringTransaction.RecurringTransactionId && t.UserId == GetUserId());
                if (existingRecurringTransaction == null)
                    return NotFound();

                _context.Entry(recurringTransaction).CurrentValues.SetValues(recurringTransaction);

            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }



        // POST: RecurringTransactions/Delete/5
        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var recurringTransaction = await _context.RecurringTransactions
                .FirstOrDefaultAsync(t => t.RecurringTransactionId == id && t.UserId == GetUserId());
            if (recurringTransaction == null)
                return NotFound();

            _context.RecurringTransactions.Remove(recurringTransaction);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
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


    }
}
