using System.Reflection.Metadata;
using BankingPaymentsAPI.Enums;


namespace BankingPaymentsAPI.Models
{
    public class Client
    {
        public int Id { get; set; }
        public int BankId { get; set; }
        public Bank Bank { get; set; }

        public string ClientCode { get; set; }   // unique within bank
        public string Name { get; set; }
        public string ContactEmail { get; set; }
        public string ContactPhone { get; set; }

        // Onboarding / Verification
        public OnboardingStatus OnboardingStatus { get; set; } = OnboardingStatus.Pending;
        public bool IsVerified { get; set; }
        public DateTimeOffset? VerifiedAt { get; set; }
        public int? VerifiedBy { get; set; }

        // Relations
        public ICollection<Beneficiary> Beneficiaries { get; set; }
        public ICollection<Employee> Employees { get; set; }
        public ICollection<Document> Documents { get; set; }

        // Linked User (1:1)
        public User User { get; set; }

        // Audit
        public DateTimeOffset CreatedAt { get; set; }
        public int CreatedBy { get; set; }
    }

}
