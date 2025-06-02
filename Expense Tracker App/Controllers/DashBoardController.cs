using Expense_Tracker_App.Data;
using Microsoft.AspNetCore.Mvc;
using Expense_Tracker_App.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Expense_Tracker_App.Service;
namespace Expense_Tracker_App.Controllers
{
    [Authorize]
    public class DashBoardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IDashBoardService _dashBoardService;

        public class SpineChartData
        {
            public string Day { get; set; }
            public decimal Income { get; set; }
            public decimal Expense { get; set; }
        }

        public DashBoardController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IDashBoardService dashBoardService)
        {
            _context = context;
            _userManager = userManager;
            _dashBoardService = dashBoardService;
        }

        private string GetUserId() => _userManager.GetUserId(User);

        public async Task<IActionResult> Index()
        {
            string userId = GetUserId();
            var summary = await _dashBoardService.GetDashBoardSummaryAsync(userId);
            ViewBag.TotalIncome = summary.TotalIncome.ToString("n2");
            ViewBag.TotalExpense = summary.TotalExpense.ToString("n2");
            ViewBag.Balance = summary.Balance;
            ViewBag.DoughnutChartData = summary.DoughnutChartData;
            ViewBag.SplineChartData = summary.SplineChartData;
            ViewBag.RecentTransactions = summary.RecentTransactions;
            return View();
        }
    }
}
