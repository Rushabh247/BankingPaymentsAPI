using  BankingPaymentsAPI.Enums;


namespace BankingPaymentsAPI.Models
{
    public class User
    {
        public int Id { get; set; }

        public string Username { get; set; }        // unique
        public string Email { get; set; }           // unique
        public string PasswordHash { get; set; }    // hashed securely

        public UserRole Role { get; set; }          // SuperAdmin, BankUser, ClientUser
        public bool IsActive { get; set; } = true;
        public DateTimeOffset LastLogin { get; set; }

  
        public int? ClientId { get; set; }
        public Client Client { get; set; }

        
        public int? BankId { get; set; }
        public Bank Bank { get; set; }

        // Audit
        public DateTimeOffset CreatedAt { get; set; }
        public int CreatedBy { get; set; }
    }
}
