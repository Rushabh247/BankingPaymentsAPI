namespace BankingPaymentsAPI.Models
{
    public class Transaction
    {
        public int Id { get; set; }
        public int? PaymentId { get; set; }
        public Payment Payment { get; set; }

        public decimal Amount { get; set; }
        public string DebitAccountMasked { get; set; }
        public string CreditAccountMasked { get; set; }

        public DateTimeOffset TransactionDate { get; set; }
        public string Status { get; set; }
        public string ExternalTxnRef { get; set; }
    }
}
