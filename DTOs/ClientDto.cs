namespace BankingPaymentsAPI.DTOs
{
    public class ClientDto
    {
        public int Id { get; set; }
        public int? BankId { get; set; }            // nullable
        public string BankName { get; set; }
        public string ClientCode { get; set; }
        public string Name { get; set; }
        public string ContactEmail { get; set; }
        public string ContactPhone { get; set; }
        public string OnboardingStatus { get; set; }
        public bool IsVerified { get; set; }
        public string? AccountNumber { get; set; }
        public string AccountNumberMasked { get; set; }

        public decimal Balance { get; set; }
    }
}
