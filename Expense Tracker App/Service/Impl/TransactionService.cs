using Expense_Tracker_App.Models;

using Expense_Tracker_App.Data;
using Expense_Tracker_App.Service;
using Microsoft.EntityFrameworkCore;

public class TransactionService : ITransactionService
{
    private readonly ApplicationDbContext _context;

    public TransactionService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Transaction>> GetUserTransactionsAsync(string userId)
    {
        return await _context.Transactions
            .Include(t => t.Category)
            .Where(t => t.UserId == userId)
            .ToListAsync();
    }
    
    public async Task<Transaction> GetTransactionByIdAsync(int id, string userId) 
    { 
    
        return await _context.Transactions
            .FirstOrDefaultAsync(t => t.TransactionId == id && t.UserId == userId);
    }

    public async Task CreateOrUpdateTransactionAsync(Transaction transaction, string userId)
    {
        transaction.UserId = userId;

        if (transaction.TransactionId == 0)
        {
            _context.Add(transaction);
        }
        else
        {
            var existingTransaction = await _context.Transactions
                .FirstOrDefaultAsync(t => t.TransactionId == transaction.TransactionId && t.UserId == userId);

            if (existingTransaction == null)
                return;

            _context.Entry(existingTransaction).CurrentValues.SetValues(transaction);
        }

        await _context.SaveChangesAsync();
    }

    public async Task<bool> DeleteTransactionAsync(int id, string userId)
    {
        var transaction = await _context.Transactions
            .FirstOrDefaultAsync(t => t.TransactionId == id && t.UserId == userId);

        if (transaction == null)
            return false;

        _context.Transactions.Remove(transaction);
        await _context.SaveChangesAsync();
        return true;
    }

    public List<Category> GetUserCategories(string userId)
    {
        var categories = _context.Categories
            .Where(c => c.UserId == userId)
            .ToList();

        categories.Insert(0, new Category { CategoryId = 0, Title = "Choose a category" });
        return categories;
    }
}
