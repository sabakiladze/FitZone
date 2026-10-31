using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitZone.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedAdmin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "DeletedAt", "Email", "FirstName", "IsActive", "IsEmailVerified", "LastLoginAt", "LastName", "PasswordHash", "RefreshTokenExpiresAt", "RefreshTokenHash", "RoleId", "UserName", "VerificationCode", "VerificationCodeExpiresAt" },
                values: new object[] { new Guid("7d3a5c2e-4b1f-4e8a-9c6d-2f1a8b9e0c34"), new DateTime(2026, 10, 25, 0, 0, 0, 0, DateTimeKind.Utc), null, "admin@fitzone.local", "System", true, true, null, "Admin", "$2a$11$QpULRn1Nj7pcQsRi.J2je.w5V23VIxVqLIDANfOzvzSO6b2P01pG.", null, null, new Guid("33b55207-ab7c-474e-82e5-fd89bf9bab59"), "admin", null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("7d3a5c2e-4b1f-4e8a-9c6d-2f1a8b9e0c34"));
        }
    }
}
