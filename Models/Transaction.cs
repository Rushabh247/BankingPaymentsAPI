using System.Text.Json.Serialization;

namespace BankingPaymentsAPI.Models
{
    public class Transaction
    {
        public int Id { get; set; }

        // For regular payments
        public int? PaymentId { get; set; }
        [JsonIgnore]
        public Payment Payment { get; set; }

        // For salary payments
        public int? SalaryPaymentId { get; set; }
        [JsonIgnore]
        public SalaryPayment SalaryPayment { get; set; }

        public decimal Amount { get; set; }
        public string DebitAccountMasked { get; set; }
        public string CreditAccountMasked { get; set; }
        public DateTimeOffset TransactionDate { get; set; }
        public string Status { get; set; }
        public string ExternalTxnRef { get; set; }
    }
}
