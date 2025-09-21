namespace BankingPaymentsAPI.DTOs
{
    public class PaymentAuditDto
    {
        public int Id { get; set; }
        public int? ClientId { get; set; }         // nullable
        public int? BeneficiaryId { get; set; }    // nullable
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string Status { get; set; }
    }
}
