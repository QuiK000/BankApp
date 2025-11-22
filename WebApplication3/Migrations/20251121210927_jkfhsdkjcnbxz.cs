using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WebApplication3.Migrations
{
    /// <inheritdoc />
    public partial class jkfhsdkjcnbxz : Migration
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
                name: "Customers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TaxNumber = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RegistrationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Services",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PercentageRate = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    ServiceType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IconClass = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Services", x => x.Id);
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

            migrationBuilder.CreateTable(
                name: "CustomerCredits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    CreditId = table.Column<int>(type: "int", nullable: false),
                    ApprovedAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TermMonths = table.Column<int>(type: "int", nullable: false),
                    InterestRate = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    IssueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RepaymentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RemainingDebt = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerCredits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerCredits_Credits_CreditId",
                        column: x => x.CreditId,
                        principalTable: "Credits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CustomerCredits_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CustomerServices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    ServiceId = table.Column<int>(type: "int", nullable: false),
                    ActivationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TotalCost = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerServices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerServices_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CustomerServices_Services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Services",
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

            migrationBuilder.InsertData(
                table: "Customers",
                columns: new[] { "Id", "Address", "DateOfBirth", "Email", "FullName", "Phone", "RegistrationDate", "Status", "TaxNumber" },
                values: new object[,]
                {
                    { 1, "м. Київ, вул. Хрещатик, 1", new DateTime(1985, 5, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "ivanov@example.com", "Іванов Іван Іванович", "+380501234567", new DateTime(2025, 5, 21, 23, 9, 25, 714, DateTimeKind.Local).AddTicks(9827), "Активний", "1234567890" },
                    { 2, "м. Львів, пр. Свободи, 15", new DateTime(1990, 8, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), "petrenko@example.com", "Петренко Олена Петрівна", "+380672345678", new DateTime(2024, 11, 21, 23, 9, 25, 714, DateTimeKind.Local).AddTicks(9834), "Активний", "0987654321" },
                    { 3, "м. Одеса, вул. Дерибасівська, 20", new DateTime(1982, 3, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "sydorenko@example.com", "Сидоренко Олег Миколайович", "+380933456789", new DateTime(2024, 5, 21, 23, 9, 25, 714, DateTimeKind.Local).AddTicks(9840), "Активний", "5555666677" }
                });

            migrationBuilder.InsertData(
                table: "Services",
                columns: new[] { "Id", "Description", "IconClass", "IsActive", "Name", "PercentageRate", "Price", "ServiceType" },
                values: new object[,]
                {
                    { 1, "Захист на випадок нещасного випадку", "fa-shield-alt", true, "Страхування життя та здоров'я", 2.5m, 0m, "Insurance" },
                    { 2, "Можливість погасити кредит раніше терміну", "fa-fast-forward", true, "Дострокове погашення без комісії", null, 500m, "EarlyRepayment" },
                    { 3, "Перші 3 місяці без процентів", "fa-calendar-check", true, "Пільговий період", null, 1000m, "GracePeriod" },
                    { 4, "Отримання SMS про стан кредиту", "fa-mobile-alt", true, "SMS-інформування", null, 50m, "Notification" },
                    { 5, "Відстрочка платежів до 6 місяців", "fa-umbrella-beach", true, "Кредитні канікули", null, 2000m, "PaymentHoliday" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_CreditApplications_CreditId",
                table: "CreditApplications",
                column: "CreditId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerCredits_CreditId",
                table: "CustomerCredits",
                column: "CreditId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerCredits_CustomerId",
                table: "CustomerCredits",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerServices_CustomerId",
                table: "CustomerServices",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerServices_ServiceId",
                table: "CustomerServices",
                column: "ServiceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CreditApplications");

            migrationBuilder.DropTable(
                name: "CustomerCredits");

            migrationBuilder.DropTable(
                name: "CustomerServices");

            migrationBuilder.DropTable(
                name: "Credits");

            migrationBuilder.DropTable(
                name: "Customers");

            migrationBuilder.DropTable(
                name: "Services");
        }
    }
}
