using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication3.Migrations
{
    /// <inheritdoc />
    public partial class fjkshdfjksdh : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Customers",
                keyColumn: "Id",
                keyValue: 1,
                column: "RegistrationDate",
                value: new DateTime(2025, 5, 22, 16, 32, 53, 537, DateTimeKind.Local).AddTicks(5963));

            migrationBuilder.UpdateData(
                table: "Customers",
                keyColumn: "Id",
                keyValue: 2,
                column: "RegistrationDate",
                value: new DateTime(2024, 11, 22, 16, 32, 53, 537, DateTimeKind.Local).AddTicks(5969));

            migrationBuilder.UpdateData(
                table: "Customers",
                keyColumn: "Id",
                keyValue: 3,
                column: "RegistrationDate",
                value: new DateTime(2024, 5, 22, 16, 32, 53, 537, DateTimeKind.Local).AddTicks(5974));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Customers",
                keyColumn: "Id",
                keyValue: 1,
                column: "RegistrationDate",
                value: new DateTime(2025, 5, 21, 23, 9, 25, 714, DateTimeKind.Local).AddTicks(9827));

            migrationBuilder.UpdateData(
                table: "Customers",
                keyColumn: "Id",
                keyValue: 2,
                column: "RegistrationDate",
                value: new DateTime(2024, 11, 21, 23, 9, 25, 714, DateTimeKind.Local).AddTicks(9834));

            migrationBuilder.UpdateData(
                table: "Customers",
                keyColumn: "Id",
                keyValue: 3,
                column: "RegistrationDate",
                value: new DateTime(2024, 5, 21, 23, 9, 25, 714, DateTimeKind.Local).AddTicks(9840));
        }
    }
}
