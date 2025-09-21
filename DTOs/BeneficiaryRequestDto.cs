namespace BankingPaymentsAPI.DTOs
{
    public class BeneficiaryRequestDto
    {
        public int ClientId { get; set; }
        public string Name { get; set; }
        public string AccountNumber { get; set; } // plain; service should encrypt
        public string IFSC { get; set; }
        public string BankName { get; set; }
        public string Email { get; set; } 
    }
}
