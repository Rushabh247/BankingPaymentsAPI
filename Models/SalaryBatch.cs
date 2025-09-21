
using BankingPaymentsAPI.Enums;
namespace BankingPaymentsAPI.Models
{
    public class SalaryBatch
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public Client Client { get; set; }

        public string BatchCode { get; set; }
        public decimal TotalAmount { get; set; }
        public BatchStatus Status { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now;

        public ICollection<SalaryPayment> Items { get; set; }
    }
}
