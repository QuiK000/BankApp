using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Font;
using iText.IO.Font;
using WebApplication3.Models;

namespace WebApplication3.Services
{
    public interface IPdfService
    {
        byte[] GenerateCreditApplicationPdf(CreditApplication application);
        byte[] GeneratePaymentSchedulePdf(CreditApplication application);
        byte[] GenerateApplicationsReportPdf(IEnumerable<CreditApplication> applications);
    }

    public class PdfService : IPdfService
    {
        public byte[] GenerateCreditApplicationPdf(CreditApplication application)
        {
            using var memoryStream = new MemoryStream();
            var writer = new PdfWriter(memoryStream);
            var pdf = new PdfDocument(writer);
            var document = new Document(pdf);

            // Заголовок
            document.Add(new Paragraph("ЗАЯВКА НА КРЕДИТ")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontSize(18)
                .SetBold());

            document.Add(new Paragraph($"Номер заявки: #{application.Id}")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontSize(12));

            document.Add(new Paragraph($"Дата подання: {application.ApplicationDate:dd.MM.yyyy HH:mm}")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontSize(10)
                .SetMarginBottom(20));

            // Інформація про кредит
            document.Add(new Paragraph("Інформація про кредит")
                .SetFontSize(14)
                .SetBold()
                .SetMarginTop(20));

            var creditTable = new Table(2);
            creditTable.SetWidth(UnitValue.CreatePercentValue(100));

            creditTable.AddCell("Тип кредиту:");
            creditTable.AddCell(application.Credit?.Name ?? "");
            
            creditTable.AddCell("Сума кредиту:");
            creditTable.AddCell($"{application.Amount:N2} грн");
            
            creditTable.AddCell("Термін кредитування:");
            creditTable.AddCell($"{application.TermMonths} місяців");
            
            creditTable.AddCell("Процентна ставка:");
            creditTable.AddCell($"{application.Credit?.InterestRate}%");
            
            creditTable.AddCell("Щомісячний платіж:");
            creditTable.AddCell($"{application.MonthlyPayment:N2} грн");
            
            creditTable.AddCell("Загальна сума виплат:");
            creditTable.AddCell($"{application.TotalAmount:N2} грн");
            
            creditTable.AddCell("Переплата:");
            creditTable.AddCell($"{application.Overpayment:N2} грн");

            document.Add(creditTable);

            // Контактна інформація
            document.Add(new Paragraph("Контактна інформація")
                .SetFontSize(14)
                .SetBold()
                .SetMarginTop(20));

            var contactTable = new Table(2);
            contactTable.SetWidth(UnitValue.CreatePercentValue(100));

            contactTable.AddCell("Ім'я:");
            contactTable.AddCell(application.CustomerName);
            
            contactTable.AddCell("Телефон:");
            contactTable.AddCell(application.Phone);
            
            contactTable.AddCell("Email:");
            contactTable.AddCell(application.Email);
            
            contactTable.AddCell("Статус:");
            contactTable.AddCell(application.Status.ToString());

            document.Add(contactTable);

            document.Close();
            return memoryStream.ToArray();
        }

        public byte[] GeneratePaymentSchedulePdf(CreditApplication application)
        {
            using var memoryStream = new MemoryStream();
            var writer = new PdfWriter(memoryStream);
            var pdf = new PdfDocument(writer);
            var document = new Document(pdf);

            // Заголовок
            document.Add(new Paragraph("ГРАФІК ПЛАТЕЖІВ")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontSize(18)
                .SetBold());

            document.Add(new Paragraph($"Заявка #{application.Id} - {application.Credit?.Name}")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontSize(12)
                .SetMarginBottom(20));

            // Інформація про кредит
            document.Add(new Paragraph($"Сума кредиту: {application.Amount:N2} грн")
                .SetFontSize(10));
            document.Add(new Paragraph($"Процентна ставка: {application.Credit?.InterestRate}%")
                .SetFontSize(10));
            document.Add(new Paragraph($"Термін: {application.TermMonths} місяців")
                .SetFontSize(10)
                .SetMarginBottom(10));

            // Таблиця графіку платежів
            var table = new Table(5);
            table.SetWidth(UnitValue.CreatePercentValue(100));

            // Заголовки
            table.AddHeaderCell("№");
            table.AddHeaderCell("Дата платежу");
            table.AddHeaderCell("Платіж");
            table.AddHeaderCell("Основний борг");
            table.AddHeaderCell("Проценти");

            var monthlyRate = (application.Credit?.InterestRate ?? 0) / 100 / 12;
            var remainingDebt = application.Amount;
            var paymentDate = application.ApplicationDate.AddMonths(1);

            for (int i = 1; i <= application.TermMonths; i++)
            {
                var interest = remainingDebt * monthlyRate;
                var principal = application.MonthlyPayment - interest;
                remainingDebt -= principal;

                table.AddCell(i.ToString());
                table.AddCell(paymentDate.ToString("dd.MM.yyyy"));
                table.AddCell($"{application.MonthlyPayment:N2}");
                table.AddCell($"{principal:N2}");
                table.AddCell($"{interest:N2}");

                paymentDate = paymentDate.AddMonths(1);
            }

            document.Add(table);

            // Підсумок
            document.Add(new Paragraph($"\nЗагальна сума виплат: {application.TotalAmount:N2} грн")
                .SetBold()
                .SetMarginTop(20));
            document.Add(new Paragraph($"Переплата: {application.Overpayment:N2} грн")
                .SetBold());

            document.Close();
            return memoryStream.ToArray();
        }

        public byte[] GenerateApplicationsReportPdf(IEnumerable<CreditApplication> applications)
        {
            using var memoryStream = new MemoryStream();
            var writer = new PdfWriter(memoryStream);
            var pdf = new PdfDocument(writer);
            var document = new Document(pdf);

            // Заголовок
            document.Add(new Paragraph("ЗВІТ ПО ЗАЯВКАХ НА КРЕДИТ")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontSize(18)
                .SetBold());

            document.Add(new Paragraph($"Дата формування: {DateTime.Now:dd.MM.yyyy HH:mm}")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontSize(10)
                .SetMarginBottom(20));

            // Статистика
            var total = applications.Count();
            var totalAmount = applications.Sum(a => a.Amount);
            var avgAmount = total > 0 ? applications.Average(a => a.Amount) : 0;

            document.Add(new Paragraph($"Загальна кількість заявок: {total}")
                .SetFontSize(12));
            document.Add(new Paragraph($"Загальна сума заявок: {totalAmount:N2} грн")
                .SetFontSize(12));
            document.Add(new Paragraph($"Середня сума заявки: {avgAmount:N2} грн")
                .SetFontSize(12)
                .SetMarginBottom(20));

            // Таблиця заявок
            var table = new Table(6);
            table.SetWidth(UnitValue.CreatePercentValue(100));
            table.SetFontSize(8);

            // Заголовки
            table.AddHeaderCell("№");
            table.AddHeaderCell("Дата");
            table.AddHeaderCell("Клієнт");
            table.AddHeaderCell("Кредит");
            table.AddHeaderCell("Сума");
            table.AddHeaderCell("Статус");

            foreach (var app in applications)
            {
                table.AddCell(app.Id.ToString());
                table.AddCell(app.ApplicationDate.ToString("dd.MM.yyyy"));
                table.AddCell(app.CustomerName);
                table.AddCell(app.Credit?.Name ?? "");
                table.AddCell($"{app.Amount:N0}");
                table.AddCell(app.Status.ToString());
            }

            document.Add(table);

            document.Close();
            return memoryStream.ToArray();
        }
    }
}