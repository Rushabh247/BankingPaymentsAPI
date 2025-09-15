namespace BankingPaymentsAPI.DTOs
{
    public class BankUpdateDto
    {
        public string BankName { get; set; }
        public string Address { get; set; }
        public string ContactNumber { get; set; }
        public bool IsActive { get; set; }
    }
}
