namespace WebApplication3.Models;

public class Credit
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal InterestRate { get; set; }

    public int MinAmount { get; set; }

    public int MaxAmount { get; set; }

    public int MinTermMonths { get; set; }

    public int MaxTermMonths { get; set; }

    public string Requirements { get; set; } = string.Empty;

    public string IconClass { get; set; } = "fa-money-bill-wave";
    
    public virtual ICollection<CustomerCredit> CustomerCredits { get; set; } = new List<CustomerCredit>();
}