using BankingPaymentsAPI.Enums;

namespace BankingPaymentsAPI.DTOs
{
    public class SalaryPaymentDto
    {
        public int Id { get; set; }
        public int? EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public decimal Amount { get; set; }

       
        public PaymentStatus Status { get; set; }

        public string TxnRef { get; set; }
        public string? FailureReason { get; set; }
        public int? SalaryBatchId { get; set; }

      
        public PaymentMethod Method { get; set; }
        public string? StripePaymentIntentId { get; set; }
    }
}
