namespace BankingPaymentsAPI.DTOs
{
  
    public class SalaryPaymentDto
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }
        public string? TxnRef { get; set; }
    }
}
