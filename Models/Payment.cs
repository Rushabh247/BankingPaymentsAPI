using System.Text.Json.Serialization;
using BankingPaymentsAPI.Enums;

namespace BankingPaymentsAPI.Models
{
    public class Payment
    {
        public int Id { get; set; }

        public int ClientId { get; set; }
        [JsonIgnore] 
        public Client Client { get; set; }

        public int? BeneficiaryId { get; set; }
        [JsonIgnore] 
        public Beneficiary Beneficiary { get; set; }

        public decimal Amount { get; set; }
        public string Currency { get; set; } = "INR";
        public PaymentStatus Status { get; set; }

        public int CreatedBy { get; set; }
        public int? ApprovedBy { get; set; }
        public DateTimeOffset? ApprovedAt { get; set; }
        public string? ApprovalRemarks { get; set; }
        public string? BankTransactionRef { get; set; }

        [JsonIgnore] 
        public ICollection<Transaction> Transactions { get; set; }

        public PaymentMethod Method { get; set; } = PaymentMethod.Internal;
        public string? StripePaymentIntentId { get; set; }
    }
}
