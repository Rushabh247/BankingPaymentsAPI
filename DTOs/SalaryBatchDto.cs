namespace BankingPaymentsAPI.DTOs
{
    public class SalaryBatchDto
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public string BatchCode { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public List<SalaryPaymentDto> Items { get; set; }
    }
}
