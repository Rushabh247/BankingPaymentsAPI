namespace BankingPaymentsAPI.DTOs
{
    public class BankDto
    {
        public int Id { get; set; }
        public string BankCode { get; set; }
        public string BankName { get; set; }
        public string Address { get; set; }
        public string ContactNumber { get; set; }
        public bool IsActive { get; set; }
    }
}
