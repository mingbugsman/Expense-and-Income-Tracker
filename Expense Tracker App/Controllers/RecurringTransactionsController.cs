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
using Expense_Tracker_App.Service;
using Microsoft.AspNetCore.Authorization;
using X.PagedList;
using X.PagedList.Extensions;

namespace Expense_Tracker_App.Controllers
{
    [Authorize]
    public class RecurringTransactionsController : Controller
    {
        private readonly IRecurringTransactionService _recurringTransactionService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICategoryService _categoryService;

        public RecurringTransactionsController(
            IRecurringTransactionService recurringTransactionService,
            UserManager<ApplicationUser> userManager,
            ICategoryService categoryService)
        {
            _recurringTransactionService = recurringTransactionService;
            _userManager = userManager;
            _categoryService = categoryService;
        }

        private string GetUserId() => _userManager.GetUserId(User);

        private async Task PopulateCategories()
        {
            var userId = GetUserId();
            var categories = await _categoryService.GetCategoriesByUserIdAsync(userId);
            categories.Insert(0, new Category { CategoryId = 0, Title = "Choose a category" });
            ViewBag.Categories = categories;
        }

        // GET: RecurringTransactions
        public async Task<IActionResult> Index(
            string? searchTerm,
            int? categoryId,
            string? frequency,
            DateTime? startDate,
            DateTime? endDate,
            decimal? minAmount,
            decimal? maxAmount,
            int? page,
            int pageSize = 10)
        {
            var userId = GetUserId();
            int pageNumber = page ?? 1;

            var transactions = await _recurringTransactionService.SearchRecurringTransactionsAsync(
                userId, searchTerm, categoryId, frequency, startDate, endDate, minAmount, maxAmount);
        
            var pagedTransactions = transactions.ToPagedList(pageNumber, pageSize);

            await PopulateCategories();
            ViewBag.SearchTerm = searchTerm;
            ViewBag.CategoryId = categoryId;
            ViewBag.Frequency = frequency;
            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;
            ViewBag.MinAmount = minAmount;
            ViewBag.MaxAmount = maxAmount;

            return View(pagedTransactions);
        }

        // GET: RecurringTransactions/CreateOrEdit
        public async Task<IActionResult> CreateOrEdit(int id = 0)
        {
            await PopulateCategories();
            if (id == 0)
                return View(new RecurringTransaction());

            var transaction = await _recurringTransactionService.GetRecurringTransactionByIdAsync(id, GetUserId());
            if (transaction == null)
                return NotFound();

            return View(transaction);
        }

        // POST: RecurringTransactions/CreateOrEdit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrEdit([Bind("RecurringTransactionId,Amount,StartDate,EndDate,Frequency,CustomIntervalDays,Note,CategoryId")] RecurringTransaction transaction)
        {
            if (!ModelState.IsValid)
            {
                await PopulateCategories();
                return View(transaction);
            }

            // Validate recurring transaction
            if (!transaction.IsValid())
            {
                ModelState.AddModelError("", "Invalid recurring transaction configuration. Please check the frequency and dates.");
                await PopulateCategories();
                return View(transaction);
            }

            transaction.UserId = GetUserId();

            if (transaction.RecurringTransactionId == 0)
                await _recurringTransactionService.AddRecurringTransactionAsync(transaction);
            else
                await _recurringTransactionService.UpdateRecurringTransactionAsync(transaction);

            return RedirectToAction(nameof(Index));
        }

        // POST: RecurringTransactions/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _recurringTransactionService.DeleteRecurringTransactionAsync(id, GetUserId());
            if (!success)
                return NotFound();

            return RedirectToAction(nameof(Index));
        }
    }
}
