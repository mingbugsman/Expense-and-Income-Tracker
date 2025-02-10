using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Expense_Tracker_App.Data;
using Expense_Tracker_App.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace Expense_Tracker_App.Controllers
{
    [Authorize]
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CategoriesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;

        }
        private string getUserId()
        {
           return _userManager.GetUserId(User);
        }

        // GET: Categories
        public async Task<IActionResult> Index()
        {
            var userId = getUserId();
            var categories = await _context.Categories.Where(c => c.UserId == userId).ToListAsync();    
            return View(categories);
        }


        // 🛠 Đảm bảo hỗ trợ phương thức GET
        public IActionResult CreateOrEdit(int id = 0)
        {
            if (id == 0)
                return View(new Category());
            else
                return View(_context.Categories.Find(id));
        }



        // POST: Categories/CreateOrEdit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrEdit([Bind("CategoryId,Title,Icon,Type")] Category category)
        {
            category.UserId = getUserId();
            if (ModelState.IsValid)
            {
                category.UserId = getUserId(); // Đảm bảo UserId là của user đang đăng nhập
                Console.WriteLine($"CategoryId: {category.CategoryId}, UserId: {category.UserId}");
                if (category.CategoryId == 0)
                    _context.Add(category);
                else
                {
                    var existingCategory = await _context.Categories
                        .FirstOrDefaultAsync(c => c.CategoryId == category.CategoryId && c.UserId == category.UserId);

                    if (existingCategory == null)
                        return NotFound();

                    _context.Entry(existingCategory).CurrentValues.SetValues(category);
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState)
                {
                    foreach (var subError in error.Value.Errors)
                    {
                        Console.WriteLine($"Field: {error.Key}, Error: {subError.ErrorMessage}");
                    }
                }
            }

            return View(category);
        }




        // POST: Categories/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = getUserId();
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.CategoryId == id && c.UserId == userId);

            if (category == null)
                return NotFound();

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }



        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.CategoryId == id);
        }
    }
}
