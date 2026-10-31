using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitZone.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SoftDeleteTrainer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Trainers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Trainers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "Trainers",
                keyColumn: "Id",
                keyValue: new Guid("05966c03-f1ee-4285-a055-518a29c368ea"),
                columns: new[] { "DeletedAt", "IsActive" },
                values: new object[] { null, true });

            migrationBuilder.UpdateData(
                table: "Trainers",
                keyColumn: "Id",
                keyValue: new Guid("063e3914-691e-4663-bbd4-5fb2acac5139"),
                columns: new[] { "DeletedAt", "IsActive" },
                values: new object[] { null, true });

            migrationBuilder.UpdateData(
                table: "Trainers",
                keyColumn: "Id",
                keyValue: new Guid("17f8c66d-6ec2-4b69-b7f5-1ee9c544e7f5"),
                columns: new[] { "DeletedAt", "IsActive" },
                values: new object[] { null, true });

            migrationBuilder.UpdateData(
                table: "Trainers",
                keyColumn: "Id",
                keyValue: new Guid("39a2d8bc-8c91-4a6a-965e-862c90c757ae"),
                columns: new[] { "DeletedAt", "IsActive" },
                values: new object[] { null, true });

            migrationBuilder.UpdateData(
                table: "Trainers",
                keyColumn: "Id",
                keyValue: new Guid("4d64029d-2c85-47e7-9d37-8a5417dd6872"),
                columns: new[] { "DeletedAt", "IsActive" },
                values: new object[] { null, true });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Trainers");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Trainers");
        }
    }
}
