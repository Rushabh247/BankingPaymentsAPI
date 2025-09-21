using BankingPaymentsAPI.Enums;

namespace BankingPaymentsAPI.DTOs
{
    public class PaymentRequestDto
    {
        public int ClientId { get; set; }
        public int? BeneficiaryId { get; set; }
        public decimal Amount { get; set; }
        public string? Currency { get; set; }

       
        public PaymentMethod Method { get; set; } = PaymentMethod.Internal;
    }
}
