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

            var score = new CreditScore
            {
                UserId = userId,
                User = user
            };

            score.AgeScore = CalculateAgeScore(user.DateOfBirth);
            score.CreditHistoryScore = await CalculateCreditHistoryScore(userId);
            score.IncomeScore = CalculateIncomeScore(user);
            score.EmploymentScore = CalculateEmploymentScore(user);
            score.ExistingDebtsScore = await CalculateExistingDebtsScore(userId);
            
            score.TotalScore = score.AgeScore + score.CreditHistoryScore + 
                              score.IncomeScore + score.EmploymentScore + 
                              score.ExistingDebtsScore;
            
            (score.Rating, score.DefaultProbability) = CalculateRating(score.TotalScore);
            
            score.RecommendedMaxAmount = await GetRecommendedAmount(score.TotalScore);

            score.CalculationDate = DateTime.Now;

            return score;
        }

        private int CalculateAgeScore(DateTime? dateOfBirth)
        {
            if (!dateOfBirth.HasValue)
                return 50;

            var age = DateTime.Now.Year - dateOfBirth.Value.Year;
            
            if (age < 21) return 50;
            if (age >= 21 && age <= 25) return 80;
            if (age >= 26 && age <= 35) return 150;
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
                return 100;

            var totalApps = applications.Count;
            var approvedApps = applications.Count(a => a.Status == ApplicationStatus.Approved || 
                                                      a.Status == ApplicationStatus.Issued);
            var rejectedApps = applications.Count(a => a.Status == ApplicationStatus.Rejected);

            var approvalRate = (double)approvedApps / totalApps;
            var baseScore = (int)(approvalRate * 200);

            var experienceBonus = Math.Min(approvedApps * 20, 100);
            
            var rejectionPenalty = rejectedApps * 15;

            var finalScore = baseScore + experienceBonus - rejectionPenalty;
            return Math.Clamp(finalScore, 0, 350);
        }

        private int CalculateIncomeScore(ApplicationUser user)
        {
            var score = 0;

            if (!string.IsNullOrEmpty(user.FullName)) score += 50;
            if (!string.IsNullOrEmpty(user.PhoneNumber)) score += 50;
            if (!string.IsNullOrEmpty(user.Address)) score += 50;
            if (!string.IsNullOrEmpty(user.TaxNumber)) score += 100;

            return Math.Min(score, 250);
        }

        private int CalculateEmploymentScore(ApplicationUser user)
        {
            var accountAge = (DateTime.Now - user.RegistrationDate).Days;
            
            if (accountAge < 30) return 50;
            if (accountAge >= 30 && accountAge < 90) return 80;
            if (accountAge >= 90 && accountAge < 180) return 110;
            if (accountAge >= 180 && accountAge < 365) return 130;
            
            return 150;
        }

        private async Task<int> CalculateExistingDebtsScore(string userId)
        {
            var activeApplications = await _context.CreditApplications
                .Where(a => a.UserId == userId && 
                           (a.Status == ApplicationStatus.Approved || 
                            a.Status == ApplicationStatus.Issued))
                .ToListAsync();

            if (!activeApplications.Any())
                return 100;

            var totalDebt = activeApplications.Sum(a => a.Amount);
            
            if (totalDebt < 50000) return 90;
            if (totalDebt < 100000) return 70;
            if (totalDebt < 200000) return 50;
            if (totalDebt < 500000) return 30;
            
            return 10;
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
            var query = _context.BlacklistEntries
                .Where(b => b.IsActive);

            if (!string.IsNullOrEmpty(taxNumber))
            {
                if (await query.AnyAsync(b => b.TaxNumber == taxNumber))
                    return true;
            }
            
            if (!string.IsNullOrEmpty(email))
            {
                if (await query.AnyAsync(b => b.Email == email))
                    return true;
            }

            if (!string.IsNullOrEmpty(phone))
            {
                if (await query.AnyAsync(b => b.Phone == phone))
                    return true;
            }

            return false;
        }

        public async Task<bool> CanApplyForCredit(string userId, decimal requestedAmount)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return false;
            
            if (await IsInBlacklist(user.TaxNumber ?? "", user.Email, user.PhoneNumber))
                return false;

            var creditScore = await CalculateCreditScore(userId);
            
            var existingScore = await _context.CreditScores
                .FirstOrDefaultAsync(cs => cs.UserId == userId);

            if (existingScore != null)
            {
                _context.CreditScores.Remove(existingScore);
            }

            _context.CreditScores.Add(creditScore);
            await _context.SaveChangesAsync();

            // Перевірка рекомендованої суми
            return requestedAmount <= creditScore.RecommendedMaxAmount;
        }
    }
}