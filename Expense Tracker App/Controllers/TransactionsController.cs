using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using Expense_Tracker_App.Models;
using Expense_Tracker_App.Service;
using X.PagedList;
using X.PagedList.Extensions;

namespace Expense_Tracker_App.Controllers
{
    [Authorize]
    public class TransactionsController : Controller
    {
        private readonly ITransactionService _transactionService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IExportService _exportService;

        public TransactionsController(
            ITransactionService transactionService, 
            UserManager<ApplicationUser> userManager,
            IExportService exportService)
        {
            _transactionService = transactionService;
            _userManager = userManager;
            _exportService = exportService;
        }

        private string GetUserId() => _userManager.GetUserId(User);

        // GET: Transactions
        public async Task<IActionResult> Index(
            string? searchTerm, 
            int? categoryId, 
            DateTime? startDate, 
            DateTime? endDate, 
            decimal? minAmount, 
            decimal? maxAmount, 
            int? page,
            int pageSize = 10)
        {
            var userId = GetUserId();
            int pageNumber = page ?? 1;

            // Get filtered transactions with pagination
            var transactions = await _transactionService.SearchTransactionsAsync(
                userId, searchTerm, categoryId, startDate, endDate, minAmount, maxAmount);

            var pagedTransactions = transactions.ToPagedList(pageNumber, pageSize);

            // Pass data to the view
            ViewBag.Categories = await _transactionService.GetUserCategoriesAsync(userId);
            ViewBag.SearchTerm = searchTerm;
            ViewBag.CategoryId = categoryId;
            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;
            ViewBag.MinAmount = minAmount;
            ViewBag.MaxAmount = maxAmount;

            return View(pagedTransactions);
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

        public async Task<IActionResult> Details(int id)
        {
            var transaction = await _transactionService.GetTransactionByIdAsync(id, GetUserId());
            if (transaction == null)
            {
                return NotFound();
            }

            return View(transaction);
        }

        // GET: Transactions/HelpCenter
        public IActionResult HelpCenter()
        {
            return View();
        }

        public async Task<IActionResult> Export(
            string? searchTerm = null,
            int? categoryId = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            decimal? minAmount = null,
            decimal? maxAmount = null)
        {
            var userId = GetUserId();
            var csvBytes = await _exportService.ExportTransactionsToCsvAsync(
                userId, searchTerm, categoryId, startDate, endDate, minAmount, maxAmount);

            var fileName = $"transactions_{DateTime.Now:yyyyMMdd}";
            if (startDate.HasValue && endDate.HasValue)
            {
                fileName += $"_{startDate:yyyyMMdd}-{endDate:yyyyMMdd}";
            }
            fileName += ".csv";

            return File(csvBytes, "text/csv", fileName);
        }
    }
}
