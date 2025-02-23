using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Expense_Tracker_App.Migrations
{
    /// <inheritdoc />
    public partial class UpdateFieldImgForAccountAndTransactions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "BillImage",
                table: "Transactions",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateOfBirth",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<byte[]>(
                name: "ProfileImg",
                table: "AspNetUsers",
                type: "varbinary(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BillImage",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "ProfileImg",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateOfBirth",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);
        }
    }
}
