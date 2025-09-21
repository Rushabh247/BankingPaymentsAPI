using BankingPaymentsAPI.Enums;

namespace BankingPaymentsAPI.DTOs
{
    public class PaymentDto
    {
        public int Id { get; set; }
        public int? ClientId { get; set; }
        public string ClientName { get; set; }
        public int? BeneficiaryId { get; set; }
        public string BeneficiaryName { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }

        
        public PaymentStatus Status { get; set; }

        public int? ApprovedBy { get; set; }
        public DateTimeOffset? ApprovedAt { get; set; }
        public string? ApprovalRemarks { get; set; }

       
        public PaymentMethod Method { get; set; }
        public string? StripePaymentIntentId { get; set; }
    }
}
