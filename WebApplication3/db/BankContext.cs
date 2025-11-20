using Microsoft.EntityFrameworkCore;
using WebApplication3.Models;

namespace WebApplication3.db;

public class BankContext(DbContextOptions<BankContext> options) : DbContext(options)
{
    public DbSet<Credit> Credits { get; set; }
    public DbSet<CreditApplication> CreditApplications { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Credit>()
            .Property(c => c.InterestRate)
            .HasPrecision(5, 2);

        modelBuilder.Entity<CreditApplication>()
            .Property(ca => ca.Amount)
            .HasPrecision(18, 2);
        
        modelBuilder.Entity<CreditApplication>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .UseIdentityColumn();
        });

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
    }
}