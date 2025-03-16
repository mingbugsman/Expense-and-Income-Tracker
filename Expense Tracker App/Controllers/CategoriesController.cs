using Expense_Tracker_App.Models;
using Expense_Tracker_App.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Expense_Tracker_App.Controllers
{
    [Authorize]
    public class CategoriesController : Controller
    {
        private readonly ICategoryService _categoryService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly INotificationLogService _notificationLogService;

        public CategoriesController(ICategoryService categoryService, UserManager<ApplicationUser> userManager, INotificationLogService notificationLogService)
        {
            _categoryService = categoryService;
            _userManager = userManager;
            _notificationLogService = notificationLogService;
        }

        private string GetUserId() => _userManager.GetUserId(User);

        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();
            var categories = await _categoryService.GetCategoriesByUserIdAsync(userId);
            return View(categories);
        }

        public async Task<IActionResult> CreateOrEdit(int id = 0)
        {
            if (id == 0)
                return View(new Category());

            var category = await _categoryService.GetCategoryByIdAsync(id, GetUserId());
            if (category == null)
                return NotFound();

            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrEdit([Bind("CategoryId,Title,Type")] Category category)
        {
            category.UserId = GetUserId();

            if (!ModelState.IsValid)
                return View(category);

            if (category.CategoryId == 0)

                await _categoryService.AddCategoryAsync(category);
            else
                await _categoryService.UpdateCategoryAsync(category);

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var success = await _categoryService.DeleteCategoryAsync(id, GetUserId());
            if (!success)
                return NotFound();

            return RedirectToAction(nameof(Index));
        }
    }
}
