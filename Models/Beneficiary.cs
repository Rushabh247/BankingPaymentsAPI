using System.ComponentModel.DataAnnotations;

namespace BankingPaymentsAPI.Models
{
    public class Beneficiary
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public Client Client { get; set; }

        public string Name { get; set; }
        public string AccountNumber { get; set; }
        public string IFSC { get; set; }
        public string Email { get; set; }

       
        public decimal Balance { get; set; } = 0m;

        [Timestamp]
        public byte[] RowVersion { get; set; }
    }
}
