using BankingPaymentsAPI.Enums;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BankingPaymentsAPI.Models
{
    public class Client
    {
        public int Id { get; set; }

        public int BankId { get; set; }
        [JsonIgnore] 
        public Bank Bank { get; set; }

        public string ClientCode { get; set; }
        public string Name { get; set; }
        public string ContactEmail { get; set; }
        public string ContactPhone { get; set; }

        public string AccountNumber { get; set; }

        public OnboardingStatus OnboardingStatus { get; set; } = OnboardingStatus.Pending;
        public bool IsVerified { get; set; }
        public DateTimeOffset? VerifiedAt { get; set; }
        public int? VerifiedBy { get; set; }

        public decimal Balance { get; set; } = 0m;
        public string? StripePaymentIntentId { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }

        [JsonIgnore]
        public ICollection<Beneficiary> Beneficiaries { get; set; } = new List<Beneficiary>();

        [JsonIgnore] 
        public ICollection<Employee> Employees { get; set; } = new List<Employee>();

        [JsonIgnore] 
        public ICollection<Document> Documents { get; set; } = new List<Document>();

        [JsonIgnore] 
        public ICollection<SalaryBatch> SalaryBatches { get; set; } = new List<SalaryBatch>();

        [JsonIgnore] 
        public User User { get; set; }

        public DateTimeOffset CreatedAt { get; set; }
        public int CreatedBy { get; set; }
    }
}
