using Expense_Tracker_App.Data;
using Expense_Tracker_App.Enum;
using Expense_Tracker_App.Models;
using Microsoft.EntityFrameworkCore;
using X.PagedList;
using X.PagedList.Extensions;

namespace Expense_Tracker_App.Service.Impl
{
    public class NotificationService : INotificationLogService
    {
        private readonly ApplicationDbContext _context;

        public NotificationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddLogAsync(string userId, NotificationType type, string message)
        {
            var log = new NotificationLog
            {
                UserId = userId,
                Log_Type = type,
                Message = message
            };
            _context.NotificationLogs.Add(log);
            await _context.SaveChangesAsync();
        }

        public  IPagedList<NotificationLog> GetNotificationLogsByUserId(string userId)
        {
            return _context.NotificationLogs
                .Where(log => log.UserId == userId)
                .OrderByDescending(log => log.CreatedAt)
                .ToPagedList();
        }
    }
}
