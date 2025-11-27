using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Font;
using iText.IO.Font;
using iText.IO.Font.Constants;
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
        private PdfFont GetUkrainianFont()
        {
            // Використовуємо Liberation Sans (безкоштовний шрифт з підтримкою кирилиці)
            // Альтернативно можна використати Arial або інший системний шрифт
            try
            {
                // Спроба завантажити системний шрифт Arial
                var fontPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "arial.ttf");
                if (File.Exists(fontPath))
                {
                    return PdfFontFactory.CreateFont(fontPath, PdfEncodings.IDENTITY_H);
                }
                
                // Якщо Arial не знайдено, використовуємо вбудований шрифт з кирилицею
                return PdfFontFactory.CreateFont(StandardFonts.HELVETICA, PdfEncodings.CP1252);
            }
            catch
            {
                // Fallback до стандартного шрифту
                return PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
            }
        }

        public byte[] GenerateCreditApplicationPdf(CreditApplication application)
        {
            using var memoryStream = new MemoryStream();
            var writer = new PdfWriter(memoryStream);
            var pdf = new PdfDocument(writer);
            var document = new Document(pdf);
            
            var font = GetUkrainianFont();
            document.SetFont(font);

            // Заголовок
            var title = new Paragraph("ЗАЯВКА НА КРЕДИТ")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontSize(20)
                .SetBold()
                .SetFont(font);
            document.Add(title);

            var appNumber = new Paragraph($"Номер заявки: #{application.Id}")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontSize(14)
                .SetFont(font);
            document.Add(appNumber);

            var appDate = new Paragraph($"Дата подання: {application.ApplicationDate:dd.MM.yyyy HH:mm}")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontSize(12)
                .SetFont(font)
                .SetMarginBottom(20);
            document.Add(appDate);

            // Інформація про кредит
            var creditHeader = new Paragraph("Інформація про кредит")
                .SetFontSize(16)
                .SetBold()
                .SetFont(font)
                .SetMarginTop(20);
            document.Add(creditHeader);

            var creditTable = new Table(2);
            creditTable.SetWidth(UnitValue.CreatePercentValue(100));
            creditTable.SetFont(font);

            AddTableRow(creditTable, "Тип кредиту:", application.Credit?.Name ?? "", font);
            AddTableRow(creditTable, "Сума кредиту:", $"{application.Amount:N2} грн", font);
            AddTableRow(creditTable, "Термін кредитування:", $"{application.TermMonths} місяців", font);
            AddTableRow(creditTable, "Процентна ставка:", $"{application.Credit?.InterestRate}%", font);
            AddTableRow(creditTable, "Щомісячний платіж:", $"{application.MonthlyPayment:N2} грн", font);
            AddTableRow(creditTable, "Загальна сума виплат:", $"{application.TotalAmount:N2} грн", font);
            AddTableRow(creditTable, "Переплата:", $"{application.Overpayment:N2} грн", font);

            document.Add(creditTable);

            // Контактна інформація
            var contactHeader = new Paragraph("Контактна інформація")
                .SetFontSize(16)
                .SetBold()
                .SetFont(font)
                .SetMarginTop(20);
            document.Add(contactHeader);

            var contactTable = new Table(2);
            contactTable.SetWidth(UnitValue.CreatePercentValue(100));
            contactTable.SetFont(font);

            AddTableRow(contactTable, "Ім'я:", application.CustomerName, font);
            AddTableRow(contactTable, "Телефон:", application.Phone, font);
            AddTableRow(contactTable, "Email:", application.Email, font);
            AddTableRow(contactTable, "Статус:", GetStatusName(application.Status), font);

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
            
            var font = GetUkrainianFont();
            document.SetFont(font);

            // Заголовок
            var title = new Paragraph("ГРАФІК ПЛАТЕЖІВ")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontSize(20)
                .SetBold()
                .SetFont(font);
            document.Add(title);

            var subtitle = new Paragraph($"Заявка #{application.Id} - {application.Credit?.Name}")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontSize(14)
                .SetFont(font)
                .SetMarginBottom(20);
            document.Add(subtitle);

            // Інформація про кредит
            document.Add(new Paragraph($"Сума кредиту: {application.Amount:N2} грн")
                .SetFontSize(12)
                .SetFont(font));
            document.Add(new Paragraph($"Процентна ставка: {application.Credit?.InterestRate}%")
                .SetFontSize(12)
                .SetFont(font));
            document.Add(new Paragraph($"Термін: {application.TermMonths} місяців")
                .SetFontSize(12)
                .SetFont(font)
                .SetMarginBottom(15));

            // Таблиця графіку платежів
            var table = new Table(6);
            table.SetWidth(UnitValue.CreatePercentValue(100));
            table.SetFont(font);
            table.SetFontSize(10);

            // Заголовки
            table.AddHeaderCell(CreateHeaderCell("№", font));
            table.AddHeaderCell(CreateHeaderCell("Дата платежу", font));
            table.AddHeaderCell(CreateHeaderCell("Платіж", font));
            table.AddHeaderCell(CreateHeaderCell("Основний борг", font));
            table.AddHeaderCell(CreateHeaderCell("Проценти", font));
            table.AddHeaderCell(CreateHeaderCell("Залишок", font));

            var monthlyRate = (application.Credit?.InterestRate ?? 0) / 100 / 12;
            var remainingDebt = application.Amount;
            var paymentDate = application.ApplicationDate.AddMonths(1);

            for (int i = 1; i <= application.TermMonths; i++)
            {
                var interest = remainingDebt * monthlyRate;
                var principal = application.MonthlyPayment - interest;
                remainingDebt -= principal;

                table.AddCell(CreateCell(i.ToString(), font));
                table.AddCell(CreateCell(paymentDate.ToString("dd.MM.yyyy"), font));
                table.AddCell(CreateCell($"{application.MonthlyPayment:N2}", font));
                table.AddCell(CreateCell($"{principal:N2}", font));
                table.AddCell(CreateCell($"{interest:N2}", font));
                table.AddCell(CreateCell($"{Math.Max(0, remainingDebt):N2}", font));

                paymentDate = paymentDate.AddMonths(1);
            }

            document.Add(table);

            // Підсумок
            document.Add(new Paragraph($"\nЗагальна сума виплат: {application.TotalAmount:N2} грн")
                .SetBold()
                .SetFont(font)
                .SetMarginTop(20));
            document.Add(new Paragraph($"Переплата: {application.Overpayment:N2} грн")
                .SetBold()
                .SetFont(font));

            document.Close();
            return memoryStream.ToArray();
        }

        public byte[] GenerateApplicationsReportPdf(IEnumerable<CreditApplication> applications)
        {
            using var memoryStream = new MemoryStream();
            var writer = new PdfWriter(memoryStream);
            var pdf = new PdfDocument(writer);
            var document = new Document(pdf);
            
            var font = GetUkrainianFont();
            document.SetFont(font);

            // Заголовок
            var title = new Paragraph("ЗВІТ ПО ЗАЯВКАХ НА КРЕДИТ")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontSize(20)
                .SetBold()
                .SetFont(font);
            document.Add(title);

            var reportDate = new Paragraph($"Дата формування: {DateTime.Now:dd.MM.yyyy HH:mm}")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontSize(12)
                .SetFont(font)
                .SetMarginBottom(20);
            document.Add(reportDate);

            // Статистика
            var total = applications.Count();
            var totalAmount = applications.Sum(a => a.Amount);
            var avgAmount = total > 0 ? applications.Average(a => a.Amount) : 0;

            document.Add(new Paragraph($"Загальна кількість заявок: {total}")
                .SetFontSize(12)
                .SetFont(font));
            document.Add(new Paragraph($"Загальна сума заявок: {totalAmount:N2} грн")
                .SetFontSize(12)
                .SetFont(font));
            document.Add(new Paragraph($"Середня сума заявки: {avgAmount:N2} грн")
                .SetFontSize(12)
                .SetFont(font)
                .SetMarginBottom(20));

            // Таблиця заявок
            var table = new Table(6);
            table.SetWidth(UnitValue.CreatePercentValue(100));
            table.SetFont(font);
            table.SetFontSize(9);

            // Заголовки
            table.AddHeaderCell(CreateHeaderCell("№", font));
            table.AddHeaderCell(CreateHeaderCell("Дата", font));
            table.AddHeaderCell(CreateHeaderCell("Клієнт", font));
            table.AddHeaderCell(CreateHeaderCell("Кредит", font));
            table.AddHeaderCell(CreateHeaderCell("Сума", font));
            table.AddHeaderCell(CreateHeaderCell("Статус", font));

            foreach (var app in applications)
            {
                table.AddCell(CreateCell(app.Id.ToString(), font));
                table.AddCell(CreateCell(app.ApplicationDate.ToString("dd.MM.yyyy"), font));
                table.AddCell(CreateCell(app.CustomerName, font));
                table.AddCell(CreateCell(app.Credit?.Name ?? "", font));
                table.AddCell(CreateCell($"{app.Amount:N0}", font));
                table.AddCell(CreateCell(GetStatusName(app.Status), font));
            }

            document.Add(table);

            document.Close();
            return memoryStream.ToArray();
        }

        private void AddTableRow(Table table, string label, string value, PdfFont font)
        {
            var labelCell = new Cell().Add(new Paragraph(label).SetFont(font).SetBold());
            var valueCell = new Cell().Add(new Paragraph(value).SetFont(font));
            table.AddCell(labelCell);
            table.AddCell(valueCell);
        }

        private Cell CreateHeaderCell(string text, PdfFont font)
        {
            return new Cell()
                .Add(new Paragraph(text).SetFont(font).SetBold())
                .SetBackgroundColor(iText.Kernel.Colors.ColorConstants.LIGHT_GRAY)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetPadding(5);
        }

        private Cell CreateCell(string text, PdfFont font)
        {
            return new Cell()
                .Add(new Paragraph(text).SetFont(font))
                .SetTextAlignment(TextAlignment.CENTER)
                .SetPadding(3);
        }

        private string GetStatusName(ApplicationStatus status)
        {
            return status switch
            {
                ApplicationStatus.New => "Нова заявка",
                ApplicationStatus.UnderReview => "На розгляді",
                ApplicationStatus.DocumentsRequired => "Потрібні документи",
                ApplicationStatus.DocumentsVerification => "Перевірка документів",
                ApplicationStatus.Approved => "Схвалено",
                ApplicationStatus.Rejected => "Відхилено",
                ApplicationStatus.Issued => "Видано",
                ApplicationStatus.Cancelled => "Скасовано",
                _ => status.ToString()
            };
        }
    }
}