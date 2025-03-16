using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using Expense_Tracker_App.Models;
using Expense_Tracker_App.Service;

namespace Expense_Tracker_App.Controllers
{
    [Authorize]
    public class TransactionsController : Controller
    {
        private readonly ITransactionService _transactionService;
        private readonly UserManager<ApplicationUser> _userManager;

        public TransactionsController(ITransactionService transactionService, UserManager<ApplicationUser> userManager)
        {
            _transactionService = transactionService;
            _userManager = userManager;
        }

        private string GetUserId() => _userManager.GetUserId(User);

        // GET: Transactions
        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();
            var transactions = await _transactionService.GetUserTransactionsAsync(userId);
            return View(transactions);
        }

        // GET: Transactions/CreateOrEdit
        public async Task<IActionResult> CreateOrEdit(int id = 0)
        {
            ViewBag.Categories = await _transactionService.GetUserCategoriesAsync(GetUserId());

            if (id == 0)
                return View(new Transaction());

            var transaction = await _transactionService.GetTransactionByIdAsync(id, GetUserId());
            if (transaction == null)
                return NotFound();

            return View(transaction);
        }

        // POST: Transactions/CreateOrEdit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrEdit([Bind("TransactionId,CategoryId,Amount,Note,TransactionDate")] Transaction transaction, IFormFile? billImage)
        {
            var userId = GetUserId();

            if (!ModelState.IsValid)
            {
                return View(transaction);
            }
            // Xử lý file ảnh hóa đơn (nếu có)
            if (billImage != null && billImage.Length > 0)
            {
                if (!billImage.ContentType.StartsWith("image/"))
                {
                    ModelState.AddModelError("BillImage", "Chỉ hỗ trợ file ảnh!");
                    ViewBag.Categories = await _transactionService.GetUserCategoriesAsync(userId);
                    return View(transaction);
                }

                using (var memoryStream = new MemoryStream())
                {
                    await billImage.CopyToAsync(memoryStream);
                    transaction.BillImage = memoryStream.ToArray();
                }
            }

            bool success = await _transactionService.CreateOrUpdateTransactionAsync(transaction, userId);

            if (!success)
            {
                ModelState.AddModelError(string.Empty, "Không thể thêm/cập nhật giao dịch. Vui lòng kiểm tra lại.");
                ViewBag.Categories = await _transactionService.GetUserCategoriesAsync(userId);
                return View(transaction);
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Transactions/Delete
        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            bool deleted = await _transactionService.DeleteTransactionAsync(id, GetUserId());

            if (!deleted)
                return NotFound();

            return RedirectToAction(nameof(Index));
        }
    }
}
