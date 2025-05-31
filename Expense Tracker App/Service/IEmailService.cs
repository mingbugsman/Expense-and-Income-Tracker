using System.Threading.Tasks;

namespace Expense_Tracker_App.Service
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body, bool isHtml = false);
    }
} 