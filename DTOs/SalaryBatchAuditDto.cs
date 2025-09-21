namespace BankingPaymentsAPI.DTOs
{
    public class SalaryBatchAuditDto
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public string BatchCode { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
    }
}
