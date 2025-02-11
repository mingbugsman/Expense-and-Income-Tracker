using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Expense_Tracker_App.Migrations
{
    /// <inheritdoc />
    public partial class updateDbset_pt2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RecurringTransaction_AspNetUsers_UserId",
                table: "RecurringTransaction");

            migrationBuilder.DropForeignKey(
                name: "FK_RecurringTransaction_Categories_CategoryId",
                table: "RecurringTransaction");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RecurringTransaction",
                table: "RecurringTransaction");

            migrationBuilder.RenameTable(
                name: "RecurringTransaction",
                newName: "RecurringTransactions");

            migrationBuilder.RenameIndex(
                name: "IX_RecurringTransaction_UserId",
                table: "RecurringTransactions",
                newName: "IX_RecurringTransactions_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_RecurringTransaction_CategoryId",
                table: "RecurringTransactions",
                newName: "IX_RecurringTransactions_CategoryId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RecurringTransactions",
                table: "RecurringTransactions",
                column: "RecurringTransactionId");

            migrationBuilder.AddForeignKey(
                name: "FK_RecurringTransactions_AspNetUsers_UserId",
                table: "RecurringTransactions",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RecurringTransactions_Categories_CategoryId",
                table: "RecurringTransactions",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "CategoryId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RecurringTransactions_AspNetUsers_UserId",
                table: "RecurringTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_RecurringTransactions_Categories_CategoryId",
                table: "RecurringTransactions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RecurringTransactions",
                table: "RecurringTransactions");

            migrationBuilder.RenameTable(
                name: "RecurringTransactions",
                newName: "RecurringTransaction");

            migrationBuilder.RenameIndex(
                name: "IX_RecurringTransactions_UserId",
                table: "RecurringTransaction",
                newName: "IX_RecurringTransaction_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_RecurringTransactions_CategoryId",
                table: "RecurringTransaction",
                newName: "IX_RecurringTransaction_CategoryId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RecurringTransaction",
                table: "RecurringTransaction",
                column: "RecurringTransactionId");

            migrationBuilder.AddForeignKey(
                name: "FK_RecurringTransaction_AspNetUsers_UserId",
                table: "RecurringTransaction",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RecurringTransaction_Categories_CategoryId",
                table: "RecurringTransaction",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "CategoryId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
