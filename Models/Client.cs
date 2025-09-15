using BankingPaymentsAPI.Enums;
using BankingPaymentsAPI.Models;

public class Client
{
    public int Id { get; set; }

    public int BankId { get; set; }
    public Bank Bank { get; set; }

    public string ClientCode { get; set; }
    public string Name { get; set; }
    public string ContactEmail { get; set; }
    public string ContactPhone { get; set; }

    public OnboardingStatus OnboardingStatus { get; set; } = OnboardingStatus.Pending;
    public bool IsVerified { get; set; }
    public DateTimeOffset? VerifiedAt { get; set; }
    public int? VerifiedBy { get; set; }

    // Relations
    public ICollection<Beneficiary> Beneficiaries { get; set; } = new List<Beneficiary>();
    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    public ICollection<Document> Documents { get; set; } = new List<Document>();

    // **Add this for SalaryBatches**
    public ICollection<SalaryBatch> SalaryBatches { get; set; } = new List<SalaryBatch>();

    public User User { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public int CreatedBy { get; set; }
}
