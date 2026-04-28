using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RetailSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RefactorJwtAccount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PasswordSalt",
                table: "AdminAccounts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RefreshToken",
                table: "AdminAccounts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "TokenCreated",
                table: "AdminAccounts",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "TokenExpires",
                table: "AdminAccounts",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "AdminAccounts",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "PasswordSalt", "RefreshToken", "TokenCreated", "TokenExpires" },
                values: new object[] { "", "", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PasswordSalt",
                table: "AdminAccounts");

            migrationBuilder.DropColumn(
                name: "RefreshToken",
                table: "AdminAccounts");

            migrationBuilder.DropColumn(
                name: "TokenCreated",
                table: "AdminAccounts");

            migrationBuilder.DropColumn(
                name: "TokenExpires",
                table: "AdminAccounts");
        }
    }
}
