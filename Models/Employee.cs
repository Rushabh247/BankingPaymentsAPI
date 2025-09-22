using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BankingPaymentsAPI.Models
{
    public class Employee
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        [JsonIgnore]
        public Client Client { get; set; }

        public string FullName { get; set; }
        public string Email { get; set; }
        public string AccountNumber { get; set; }
        public decimal Salary { get; set; } = 0m;
        public decimal Balance { get; set; } = 0m;

        [Timestamp]
        public byte[] RowVersion { get; set; }
    }
}
