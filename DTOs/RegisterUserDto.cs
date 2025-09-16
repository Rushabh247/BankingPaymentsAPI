namespace BankingPaymentsAPI.DTOs
{
    public class RegisterUserDto
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }  // plain, hash before saving
        public string Role { get; set; }      // "Admin", "ClientUser", etc.
    }
}
