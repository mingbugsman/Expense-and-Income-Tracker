using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Expense_Tracker_App.Migrations
{
    public partial class AddCustomIntervalDaysToRecurringTransactions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CustomIntervalDays",
                table: "RecurringTransactions",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomIntervalDays",
                table: "RecurringTransactions");
        }
    }
}
