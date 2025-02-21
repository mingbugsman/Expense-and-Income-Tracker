using Expense_Tracker_App.Enum;
using Expense_Tracker_App.Models;
using Microsoft.EntityFrameworkCore;

namespace Expense_Tracker_App.Service
{
    public interface INotificationLogService
    {
        Task AddLogAsync(string userId, NotificationType type, string message);
        Task<List<NotificationLog>> GetNotificationLogsByUserId(string userId);
    }
}
