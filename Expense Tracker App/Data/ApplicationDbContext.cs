using Expense_Tracker_App.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Expense_Tracker_App.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Category> Categories { get; set; }

        public DbSet<Budget> Budgets { get; set; }

        public DbSet<RecurringTransaction> RecurringTransactions { get; set; }

        public DbSet<NotificationLog> NotificationLogs { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // cấu trúc 
            modelBuilder.Entity<NotificationLog>()
             .Property(n => n.Log_Type)
             .HasConversion<string>(); // Lưu dưới dạng string thay vì int


            base.OnModelCreating(modelBuilder);
            // Cấu hình quan hệ 1-n giữa User và Category
            modelBuilder.Entity<NotificationLog>()
                .HasOne(n => n.User)
                .WithMany(u => u.NotificationLogs)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Cấu hình quan hệ 1-n giữa User và Category
            modelBuilder.Entity<Category>()
            .HasOne(c => c.User)
            .WithMany(u => u.Categories)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            ;
            // Cấu hình quan hệ 1-n giữa User và Transaction
            modelBuilder.Entity<Transaction>()
            .HasOne(t => t.User)
            .WithMany(u => u.Transactions)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);

            // Cấu hình quan 1-N giữa user và Budget
            modelBuilder.Entity<Budget>()
                .HasOne(b => b.User)
                .WithMany(u => u.Budgets)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RecurringTransaction>()
                .HasOne(rt => rt.User)
                .WithMany(u => u.RecurringTransactions)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<Transaction>()
            .HasOne(t => t.Category)
            .WithMany()
            .HasForeignKey(t => t.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Budget>()
                .HasOne(b => b.Category)
                .WithMany()
                .HasForeignKey(b => b.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RecurringTransaction>()
            .HasOne(rt => rt.Category)
            .WithMany()
            .HasForeignKey(rt => rt.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<Transaction>()
            .HasIndex(t => t.UserId);

            modelBuilder.Entity<Category>()
           .HasIndex(c => c.UserId);

            modelBuilder.Entity<Budget>()
           .HasIndex(b => b.UserId);



        }
    }   
    
}
