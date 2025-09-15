namespace BankingPaymentsAPI.DTOs
{
    public class PaymentDto
    {
        public int Id { get; set; }
        public string ClientName { get; set; }
        public string BeneficiaryName { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }
        public int? ApprovedBy { get; set; }
        public DateTimeOffset? ApprovedAt { get; set; }
    }
}
