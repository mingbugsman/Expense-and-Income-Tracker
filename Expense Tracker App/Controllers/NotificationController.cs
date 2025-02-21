using Expense_Tracker_App.Models;
using Expense_Tracker_App.Service;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Expense_Tracker_App.Controllers
{
    public class NotificationController : Controller
    {
        private readonly INotificationLogService _notificationLogService;
        private readonly UserManager<ApplicationUser> _userManager;
        public NotificationController(INotificationLogService notificationLogService, UserManager<ApplicationUser> userManager) 
        {
            _notificationLogService = notificationLogService;
            _userManager = userManager;
        }
        public async Task<IActionResult> Index()
        {
            var notifications =await _notificationLogService.GetNotificationLogsByUserId(_userManager.GetUserId(User));
            return View(notifications);
        }
    }
}
