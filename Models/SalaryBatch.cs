using BankingPaymentsAPI.Enums;
using System.Text.Json.Serialization;

namespace BankingPaymentsAPI.Models
{
    public class SalaryBatch
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        [JsonIgnore] 
        public Client Client { get; set; }

        public string BatchCode { get; set; }
        public decimal TotalAmount { get; set; }
        public BatchStatus Status { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now;

        [JsonIgnore] 
        public ICollection<SalaryPayment> Items { get; set; }
    }
}
