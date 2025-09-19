using System.Text.Json.Serialization;

namespace BankingPaymentsAPI.Models
{
    public class Beneficiary
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        [JsonIgnore]
        public Client Client { get; set; }

        public string Name { get; set; }
        public string AccountNumberEncrypted { get; set; } // encrypted at rest
        public string IFSC { get; set; }
        public string BankName { get; set; }
        public bool IsActive { get; set; } = true;
    }

}
