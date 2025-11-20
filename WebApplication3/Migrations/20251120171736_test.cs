using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WebApplication3.Migrations
{
    /// <inheritdoc />
    public partial class test : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Credits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InterestRate = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    MinAmount = table.Column<int>(type: "int", nullable: false),
                    MaxAmount = table.Column<int>(type: "int", nullable: false),
                    MinTermMonths = table.Column<int>(type: "int", nullable: false),
                    MaxTermMonths = table.Column<int>(type: "int", nullable: false),
                    Requirements = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IconClass = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Credits", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CreditApplications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TermMonths = table.Column<int>(type: "int", nullable: false),
                    ApplicationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreditId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CreditApplications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CreditApplications_Credits_CreditId",
                        column: x => x.CreditId,
                        principalTable: "Credits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Credits",
                columns: new[] { "Id", "Description", "IconClass", "InterestRate", "MaxAmount", "MaxTermMonths", "MinAmount", "MinTermMonths", "Name", "Requirements" },
                values: new object[,]
                {
                    { 1, "Кредит на будь-які потреби без застави", "fa-shopping-cart", 18.5m, 300000, 60, 5000, 6, "Споживчий кредит", "Паспорт, ІПН, довідка про доходи" },
                    { 2, "Кредит на придбання нерухомості під заставу", "fa-home", 12.9m, 5000000, 300, 100000, 12, "Іпотека", "Паспорт, ІПН, довідка про доходи, документи на нерухомість" },
                    { 3, "Кредит на придбання автомобіля під заставу авто", "fa-car", 15.9m, 1500000, 84, 50000, 12, "Автокредит", "Паспорт, ІПН, довідка про доходи, водійське посвідчення" },
                    { 4, "Кредит для розвитку малого та середнього бізнесу", "fa-briefcase", 16.5m, 3000000, 120, 50000, 12, "Бізнес кредит", "Реєстрація ФОП/ТОВ, бізнес-план, фінансова звітність" },
                    { 5, "Перекредитування існуючих кредитів за нижчою ставкою", "fa-sync-alt", 14.9m, 1000000, 120, 20000, 12, "Рефінансування", "Паспорт, ІПН, кредитний договір, довідка про заборгованість" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_CreditApplications_CreditId",
                table: "CreditApplications",
                column: "CreditId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CreditApplications");

            migrationBuilder.DropTable(
                name: "Credits");
        }
    }
}
