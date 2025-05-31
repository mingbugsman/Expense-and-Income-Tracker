using Expense_Tracker_App.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Expense_Tracker_App.Service
{
    public interface ICategoryService
    {
        Task<List<Category>> GetCategoriesByUserIdAsync(string userId);
        Task<Category?> GetCategoryByIdAsync(int id, string userId);
        Task AddCategoryAsync(Category category);
        Task UpdateCategoryAsync(Category category);
        Task<bool> DeleteCategoryAsync(int id, string userId);
        Task<bool> CategoryExistsAsync(int id);
        Task<List<Category>> SearchCategoriesAsync(string userId, string? searchTerm, string? type);
    }
}
