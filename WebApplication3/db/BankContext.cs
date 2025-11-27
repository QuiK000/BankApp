using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebApplication3.Models;

namespace WebApplication3.db
{
    public class BankContext : IdentityDbContext<ApplicationUser>
    {
        public BankContext(DbContextOptions<BankContext> options) : base(options)
        {
        }

        public DbSet<Credit> Credits { get; set; }
        public DbSet<CreditApplication> CreditApplications { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<CustomerCredit> CustomerCredits { get; set; }
        public DbSet<CustomerService> CustomerServices { get; set; }
        
        public DbSet<CreditScore> CreditScores { get; set; }
        public DbSet<BlacklistEntry> BlacklistEntries { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Credit>()
                .Property(c => c.InterestRate)
                .HasPrecision(5, 2);
            
            modelBuilder.Entity<CreditApplication>()
                .Property(ca => ca.Amount)
                .HasPrecision(18, 2);
            
            modelBuilder.Entity<CustomerCredit>()
                .Property(cc => cc.ApprovedAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<CustomerCredit>()
                .Property(cc => cc.InterestRate)
                .HasPrecision(5, 2);

            modelBuilder.Entity<CustomerCredit>()
                .Property(cc => cc.RemainingDebt)
                .HasPrecision(18, 2);
            
            modelBuilder.Entity<Service>()
                .Property(s => s.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Service>()
                .Property(s => s.PercentageRate)
                .HasPrecision(5, 2);

            modelBuilder.Entity<CustomerService>()
                .Property(cs => cs.TotalCost)
                .HasPrecision(18, 2);
            
            modelBuilder.Entity<CreditScore>()
                .Property(cs => cs.DefaultProbability)
                .HasPrecision(5, 2);

            modelBuilder.Entity<CreditScore>()
                .Property(cs => cs.RecommendedMaxAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<BlacklistEntry>()
                .Property(b => b.DebtAmount)
                .HasPrecision(18, 2);
            
            modelBuilder.Entity<CustomerCredit>()
                .HasOne(cc => cc.Customer)
                .WithMany(c => c.CustomerCredits)
                .HasForeignKey(cc => cc.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CustomerCredit>()
                .HasOne(cc => cc.Credit)
                .WithMany(cr => cr.CustomerCredits)
                .HasForeignKey(cc => cc.CreditId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CustomerService>()
                .HasOne(cs => cs.Customer)
                .WithMany(c => c.CustomerServices)
                .HasForeignKey(cs => cs.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CustomerService>()
                .HasOne(cs => cs.Service)
                .WithMany(s => s.CustomerServices)
                .HasForeignKey(cs => cs.ServiceId)
                .OnDelete(DeleteBehavior.Cascade);
            
            modelBuilder.Entity<CreditApplication>()
                .HasOne(ca => ca.User)
                .WithMany(u => u.CreditApplications)
                .HasForeignKey(ca => ca.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<CreditScore>()
                .HasOne(cs => cs.User)
                .WithMany()
                .HasForeignKey(cs => cs.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Credit>().HasData(
                new Credit
                {
                    Id = 1,
                    Name = "Споживчий кредит",
                    Description = "Кредит на будь-які потреби без застави",
                    InterestRate = 18.5m,
                    MinAmount = 5000,
                    MaxAmount = 300000,
                    MinTermMonths = 6,
                    MaxTermMonths = 60,
                    Requirements = "Паспорт, ІПН, довідка про доходи",
                    IconClass = "fa-shopping-cart"
                },
                new Credit
                {
                    Id = 2,
                    Name = "Іпотека",
                    Description = "Кредит на придбання нерухомості під заставу",
                    InterestRate = 12.9m,
                    MinAmount = 100000,
                    MaxAmount = 5000000,
                    MinTermMonths = 12,
                    MaxTermMonths = 300,
                    Requirements = "Паспорт, ІПН, довідка про доходи, документи на нерухомість",
                    IconClass = "fa-home"
                },
                new Credit
                {
                    Id = 3,
                    Name = "Автокредит",
                    Description = "Кредит на придбання автомобіля під заставу авто",
                    InterestRate = 15.9m,
                    MinAmount = 50000,
                    MaxAmount = 1500000,
                    MinTermMonths = 12,
                    MaxTermMonths = 84,
                    Requirements = "Паспорт, ІПН, довідка про доходи, водійське посвідчення",
                    IconClass = "fa-car"
                },
                new Credit
                {
                    Id = 4,
                    Name = "Бізнес кредит",
                    Description = "Кредит для розвитку малого та середнього бізнесу",
                    InterestRate = 16.5m,
                    MinAmount = 50000,
                    MaxAmount = 3000000,
                    MinTermMonths = 12,
                    MaxTermMonths = 120,
                    Requirements = "Реєстрація ФОП/ТОВ, бізнес-план, фінансова звітність",
                    IconClass = "fa-briefcase"
                },
                new Credit
                {
                    Id = 5,
                    Name = "Рефінансування",
                    Description = "Перекредитування існуючих кредитів за нижчою ставкою",
                    InterestRate = 14.9m,
                    MinAmount = 20000,
                    MaxAmount = 1000000,
                    MinTermMonths = 12,
                    MaxTermMonths = 120,
                    Requirements = "Паспорт, ІПН, кредитний договір, довідка про заборгованість",
                    IconClass = "fa-sync-alt"
                }
            );
            
            modelBuilder.Entity<Service>().HasData(
                new Service
                {
                    Id = 1,
                    Name = "Страхування життя та здоров'я",
                    Description = "Захист на випадок нещасного випадку",
                    Price = 0,
                    PercentageRate = 2.5m,
                    ServiceType = "Insurance",
                    IconClass = "fa-shield-alt",
                    IsActive = true
                },
                new Service
                {
                    Id = 2,
                    Name = "Дострокове погашення без комісії",
                    Description = "Можливість погасити кредит раніше терміну",
                    Price = 500,
                    PercentageRate = null,
                    ServiceType = "EarlyRepayment",
                    IconClass = "fa-fast-forward",
                    IsActive = true
                },
                new Service
                {
                    Id = 3,
                    Name = "Пільговий період",
                    Description = "Перші 3 місяці без процентів",
                    Price = 1000,
                    PercentageRate = null,
                    ServiceType = "GracePeriod",
                    IconClass = "fa-calendar-check",
                    IsActive = true
                },
                new Service
                {
                    Id = 4,
                    Name = "SMS-інформування",
                    Description = "Отримання SMS про стан кредиту",
                    Price = 50,
                    PercentageRate = null,
                    ServiceType = "Notification",
                    IconClass = "fa-mobile-alt",
                    IsActive = true
                },
                new Service
                {
                    Id = 5,
                    Name = "Кредитні канікули",
                    Description = "Відстрочка платежів до 6 місяців",
                    Price = 2000,
                    PercentageRate = null,
                    ServiceType = "PaymentHoliday",
                    IconClass = "fa-umbrella-beach",
                    IsActive = true
                }
            );
        }
    }
}