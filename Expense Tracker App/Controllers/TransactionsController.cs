using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Expense_Tracker_App.Data;
using Expense_Tracker_App.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace Expense_Tracker_App.Controllers
{
    [Authorize]
    public class TransactionsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TransactionsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private string GetUserId() => _userManager.GetUserId(User);

        // GET: Transactions
        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();
            var transactions = _context.Transactions
                .Include(t => t.Category)
                .Where(t => t.UserId == userId);

            return View(await transactions.ToListAsync());
        }

        // GET: Transactions/CreateOrEdit
        public IActionResult CreateOrEdit(int id = 0)
        {
            PopulateCategories();
            if (id == 0)
                return View(new Transaction());

            var transaction = _context.Transactions
                .FirstOrDefault(t => t.TransactionId == id && t.UserId == GetUserId());

            if (transaction == null)
                return NotFound();

            return View(transaction);
        }

        // POST: Transactions/CreateOrEdit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrEdit([Bind("TransactionId,CategoryId,Amount,Note,TransactionDate")] Transaction transaction)
        {
          
            if (!ModelState.IsValid)
            {
                PopulateCategories();
                return View(transaction);
            }

            transaction.UserId = GetUserId(); // Đảm bảo chỉ thao tác với dữ liệu của user hiện tại

            if (transaction.TransactionId == 0)
                _context.Add(transaction);
            else
            {
                var existingTransaction = _context.Transactions
                    .FirstOrDefault(t => t.TransactionId == transaction.TransactionId && t.UserId == GetUserId());

                if (existingTransaction == null)
                    return NotFound();

                _context.Entry(existingTransaction).CurrentValues.SetValues(transaction);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // POST: Transactions/Delete
        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var transaction = await _context.Transactions
                .FirstOrDefaultAsync(t => t.TransactionId == id && t.UserId == GetUserId());

            if (transaction == null)
                return NotFound();

            _context.Transactions.Remove(transaction);
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
