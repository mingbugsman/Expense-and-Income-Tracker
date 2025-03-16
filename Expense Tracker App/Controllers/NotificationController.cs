using Expense_Tracker_App.Models;
using Expense_Tracker_App.Service;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using X.PagedList.Extensions;

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
        public IActionResult Index(int? page)
        {
            int pageSize = 10;
            int pageNumber = page ?? 1;
            var notifications =  _notificationLogService.GetNotificationLogsByUserId(_userManager.GetUserId(User));
            var res = notifications.OrderByDescending(n => n.CreatedAt).ToPagedList(pageNumber, pageSize);
            return  View(res);
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
