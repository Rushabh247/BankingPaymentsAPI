namespace BankingPaymentsAPI.Models
{
    public class Bank
    {
        public int Id { get; set; }
        public string BankCode { get; set; } // unique
        public string BankName { get; set; }
        public string Address { get; set; }
        public string ContactNumber { get; set; }
        public bool IsActive { get; set; } = true;

        public decimal AccountBalance { get; set; } = 0;

        public ICollection<Client> Clients { get; set; }
        public ICollection<User> Users { get; set; } // BankUsers
    }
}
