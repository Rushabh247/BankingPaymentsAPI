namespace BankingPaymentsAPI.DTOs
{
    public class ClientRequestDto
    {
        public int BankId { get; set; }
        public string ClientCode { get; set; }
        public string Name { get; set; }
        public string ContactEmail { get; set; }
        public string ContactPhone { get; set; }
    }
}
