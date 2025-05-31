using Expense_Tracker_App.Models;
using Expense_Tracker_App.Service;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Expense_Tracker_App.Data;

namespace Expense_Tracker_App.Controllers
{
    [Authorize]
    public class NotificationController : Controller
    {
        private readonly INotificationLogService _notificationLogService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public NotificationController(INotificationLogService notificationLogService, UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _notificationLogService = notificationLogService;
            _userManager = userManager;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { count = 0 });
            }

            var count = await _context.NotificationLogs
                .Where(n => n.UserId == userId && !n.IsRead)
                .CountAsync();

            return Json(new { count });
        }

        public IActionResult Index(int? page)
        {
            int pageSize = 10;
            int pageNumber = page ?? 1;
            var userId = _userManager.GetUserId(User);
            var notifications = _notificationLogService.GetNotificationLogsByUserId(userId, pageNumber, pageSize);
            _notificationLogService.MarkNotificationsAsRead(userId);
            return View(notifications);
        }

        public IActionResult Details(int id)
        {
            var logs = _notificationLogService.GetNotificationLogsByUserId(_userManager.GetUserId(User));
            var log = logs.Where(l => l.Log_Id == id).FirstOrDefault();
            if (log == null) 
            {
                return NotFound();
            }
            return Json(new { message = log });
        }
    }
}
