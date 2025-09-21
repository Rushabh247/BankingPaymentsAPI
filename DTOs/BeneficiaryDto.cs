namespace BankingPaymentsAPI.DTOs
{
    public class BeneficiaryDto
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public string Name { get; set; }
        public string AccountNumberMasked { get; set; } // masked for UI
        public string IFSC { get; set; }
        public string BankName { get; set; }

        public decimal Balance { get; set; }
        public bool IsActive { get; set; }
    }
}
