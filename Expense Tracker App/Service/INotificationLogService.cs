using Expense_Tracker_App.Enum;
using Expense_Tracker_App.Models;
using Microsoft.EntityFrameworkCore;
using X.PagedList;

namespace Expense_Tracker_App.Service
{
    public interface INotificationLogService
    {
        Task AddLogAsync(string userId, NotificationType type, string message);
        IPagedList<NotificationLog> GetNotificationLogsByUserId(string userId, int pageNumber = 1, int pageSize = 10);
        int GetUnreadNotificationCount(string userId);
        void MarkNotificationsAsRead(string userId);
        Task<int> GetUnreadCountAsync(string userId);
        NotificationLog? GetNotificationLogById(int id, string userId);
    }
}
