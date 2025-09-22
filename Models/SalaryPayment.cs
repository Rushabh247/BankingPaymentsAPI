using BankingPaymentsAPI.Enums;
using System.Text.Json.Serialization;

namespace BankingPaymentsAPI.Models
{
    public class SalaryPayment
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public decimal Amount { get; set; }

        [JsonIgnore] 
        public PaymentStatus Status { get; set; }

        public string TxnRef { get; set; }

        public PaymentMethod Method { get; set; }
        public string? StripePaymentIntentId { get; set; }
        public string? FailureReason { get; set; }

        [JsonIgnore] 
        public Employee? Employee { get; set; }

        public int SalaryBatchId { get; set; }
        [JsonIgnore] 
        public SalaryBatch? SalaryBatch { get; set; }
    }
}
