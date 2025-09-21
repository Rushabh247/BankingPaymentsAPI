namespace BankingPaymentsAPI.DTOs
{
    public class SalaryPaymentAuditDto
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }
        public string? TxnRef { get; set; }
        public string? FailureReason { get; set; }
    }
}
