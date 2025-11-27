using Microsoft.EntityFrameworkCore;
using WebApplication3.db;
using WebApplication3.Models;

namespace WebApplication3.Services
{
    public interface ICreditScoringService
    {
        Task<CreditScore> CalculateCreditScore(string userId);
        Task<bool> IsInBlacklist(string taxNumber, string? email = null, string? phone = null);
        Task<decimal> GetRecommendedAmount(int creditScore);
        Task<bool> CanApplyForCredit(string userId, decimal requestedAmount);
    }

    public class CreditScoringService : ICreditScoringService
    {
        private readonly BankContext _context;

        public CreditScoringService(BankContext context)
        {
            _context = context;
        }

        public async Task<CreditScore> CalculateCreditScore(string userId)
        {
            var user = await _context.Users
                .Include(u => u.CreditApplications)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                throw new Exception("Користувача не знайдено");

            // Перевірка чорного списку
            var isBlacklisted = await IsInBlacklist(user.TaxNumber ?? "", user.Email, user.PhoneNumber);
            
            var score = new CreditScore
            {
                UserId = userId,
                User = user
            };

            // Якщо в чорному списку - мінімальні бали
            if (isBlacklisted)
            {
                score.AgeScore = 0;
                score.CreditHistoryScore = 0;
                score.IncomeScore = 0;
                score.EmploymentScore = 0;
                score.ExistingDebtsScore = 0;
                score.TotalScore = 0;
                score.Rating = "E";
                score.DefaultProbability = 100m;
                score.RecommendedMaxAmount = 0;
                score.Notes = "Особа знаходиться в чорному списку банку";
                return score;
            }

            // Розрахунок балів
            score.AgeScore = CalculateAgeScore(user.DateOfBirth);
            score.CreditHistoryScore = await CalculateCreditHistoryScore(userId);
            score.IncomeScore = CalculateIncomeScore(user);
            score.EmploymentScore = CalculateEmploymentScore(user);
            score.ExistingDebtsScore = await CalculateExistingDebtsScore(userId);
            
            // Загальний бал
            score.TotalScore = score.AgeScore + score.CreditHistoryScore + 
                              score.IncomeScore + score.EmploymentScore + 
                              score.ExistingDebtsScore;
            
            // Рейтинг та ймовірність дефолту
            (score.Rating, score.DefaultProbability) = CalculateRating(score.TotalScore);
            
            // Рекомендована сума
            score.RecommendedMaxAmount = await GetRecommendedAmount(score.TotalScore);

            score.CalculationDate = DateTime.Now;

            // Видаляємо старий скоринг якщо є
            var existingScore = await _context.CreditScores
                .FirstOrDefaultAsync(cs => cs.UserId == userId);

            if (existingScore != null)
            {
                _context.CreditScores.Remove(existingScore);
            }

            // Зберігаємо новий
            _context.CreditScores.Add(score);
            await _context.SaveChangesAsync();

            return score;
        }

        private int CalculateAgeScore(DateTime? dateOfBirth)
        {
            if (!dateOfBirth.HasValue)
                return 50; // Мінімальний бал без дати народження

            var age = DateTime.Now.Year - dateOfBirth.Value.Year;
            
            // Коригування якщо день народження ще не настав цього року
            if (DateTime.Now < dateOfBirth.Value.AddYears(age))
                age--;
            
            if (age < 21) return 50;
            if (age >= 21 && age <= 25) return 80;
            if (age >= 26 && age <= 35) return 150;  // Найкращий вік
            if (age >= 36 && age <= 50) return 130;
            if (age >= 51 && age <= 65) return 100;
            
            return 60; // Старше 65
        }

        private async Task<int> CalculateCreditHistoryScore(string userId)
        {
            var applications = await _context.CreditApplications
                .Where(a => a.UserId == userId)
                .ToListAsync();

            if (!applications.Any())
                return 100; // Нова історія - середній бал

            var totalApps = applications.Count;
            var approvedApps = applications.Count(a => a.Status == ApplicationStatus.Approved || 
                                                      a.Status == ApplicationStatus.Issued);
            var rejectedApps = applications.Count(a => a.Status == ApplicationStatus.Rejected);

            // Відсоток схвалених
            var approvalRate = (double)approvedApps / totalApps;
            var baseScore = (int)(approvalRate * 200);

            // Бонус за досвід
            var experienceBonus = Math.Min(approvedApps * 20, 100);
            
            // Штраф за відхилення
            var rejectionPenalty = rejectedApps * 15;

            var finalScore = baseScore + experienceBonus - rejectionPenalty;
            return Math.Clamp(finalScore, 0, 350);
        }

        private int CalculateIncomeScore(ApplicationUser user)
        {
            var score = 0;

            // Бали за заповненість профілю (індикатор надійності)
            if (!string.IsNullOrEmpty(user.FullName)) score += 50;
            if (!string.IsNullOrEmpty(user.PhoneNumber)) score += 50;
            if (!string.IsNullOrEmpty(user.Address)) score += 50;
            if (!string.IsNullOrEmpty(user.TaxNumber)) score += 100;

            return Math.Min(score, 250);
        }

        private int CalculateEmploymentScore(ApplicationUser user)
        {
            // Використовуємо тривалість реєстрації як індикатор стабільності
            var accountAge = (DateTime.Now - user.RegistrationDate).Days;
            
            if (accountAge < 30) return 50;       // Менше місяця
            if (accountAge < 90) return 80;       // 1-3 місяці
            if (accountAge < 180) return 110;     // 3-6 місяців
            if (accountAge < 365) return 130;     // 6-12 місяців
            
            return 150; // Більше року
        }

        private async Task<int> CalculateExistingDebtsScore(string userId)
        {
            var activeApplications = await _context.CreditApplications
                .Where(a => a.UserId == userId && 
                           (a.Status == ApplicationStatus.Approved || 
                            a.Status == ApplicationStatus.Issued))
                .ToListAsync();

            if (!activeApplications.Any())
                return 100; // Немає боргів - добре

            var totalDebt = activeApplications.Sum(a => a.Amount);
            
            // Чим більше боргів, тим менше балів
            if (totalDebt < 50000) return 90;
            if (totalDebt < 100000) return 70;
            if (totalDebt < 200000) return 50;
            if (totalDebt < 500000) return 30;
            
            return 10; // Дуже великий борг
        }

        private (string rating, decimal probability) CalculateRating(int totalScore)
        {
            if (totalScore >= 850) return ("A+", 2.0m);
            if (totalScore >= 750) return ("A", 5.0m);
            if (totalScore >= 650) return ("B+", 10.0m);
            if (totalScore >= 550) return ("B", 15.0m);
            if (totalScore >= 450) return ("C", 25.0m);
            if (totalScore >= 350) return ("D", 40.0m);
            
            return ("E", 60.0m);
        }

        public async Task<decimal> GetRecommendedAmount(int creditScore)
        {
            if (creditScore >= 850) return 1000000;
            if (creditScore >= 750) return 500000;
            if (creditScore >= 650) return 300000;
            if (creditScore >= 550) return 150000;
            if (creditScore >= 450) return 75000;
            if (creditScore >= 350) return 30000;
            
            return 10000;
        }

        public async Task<bool> IsInBlacklist(string taxNumber, string? email = null, string? phone = null)
        {
            // Перевірка по ІПН
            if (!string.IsNullOrEmpty(taxNumber))
            {
                var byTaxNumber = await _context.BlacklistEntries
                    .AnyAsync(b => b.IsActive && b.TaxNumber == taxNumber);
                if (byTaxNumber) return true;
            }
            
            // Перевірка по email
            if (!string.IsNullOrEmpty(email))
            {
                var byEmail = await _context.BlacklistEntries
                    .AnyAsync(b => b.IsActive && b.Email == email);
                if (byEmail) return true;
            }

            // Перевірка по телефону
            if (!string.IsNullOrEmpty(phone))
            {
                var byPhone = await _context.BlacklistEntries
                    .AnyAsync(b => b.IsActive && b.Phone == phone);
                if (byPhone) return true;
            }

            return false;
        }

        public async Task<bool> CanApplyForCredit(string userId, decimal requestedAmount)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return false;
            
            // Перевірка чорного списку - критично важливо!
            if (await IsInBlacklist(user.TaxNumber ?? "", user.Email, user.PhoneNumber))
                return false;

            // Розрахунок або отримання існуючого скорингу
            var creditScore = await _context.CreditScores
                .Where(cs => cs.UserId == userId)
                .OrderByDescending(cs => cs.CalculationDate)
                .FirstOrDefaultAsync();

            // Якщо скоринг старий (більше 30 днів) або немає - перераховуємо
            if (creditScore == null || (DateTime.Now - creditScore.CalculationDate).TotalDays > 30)
            {
                creditScore = await CalculateCreditScore(userId);
            }

            // Перевірка рекомендованої суми
            return requestedAmount <= creditScore.RecommendedMaxAmount;
        }
    }
}