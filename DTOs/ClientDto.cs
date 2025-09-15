namespace BankingPaymentsAPI.DTOs
{
    public class ClientDto
    {
        public int Id { get; set; }
        public string BankName { get; set; }
        public string ClientCode { get; set; }
        public string Name { get; set; }
        public string ContactEmail { get; set; }
        public string ContactPhone { get; set; }
        public string OnboardingStatus { get; set; }
        public bool IsVerified { get; set; }
    }
}
