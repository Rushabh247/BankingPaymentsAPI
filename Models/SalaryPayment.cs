using BankingPaymentsAPI.Enums;

namespace BankingPaymentsAPI.Models
{
    public class SalaryPayment
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public decimal Amount { get; set; }
        public PaymentStatus Status { get; set; }
        public string TxnRef { get; set; }

        public PaymentMethod Method { get; set; }   
        public string? StripePaymentIntentId { get; set; } // for Stripe only
        public string? FailureReason { get; set; }

        public Employee? Employee { get; set; }

        public int SalaryBatchId { get; set; }
        public SalaryBatch? SalaryBatch { get; set; }
    }
}
