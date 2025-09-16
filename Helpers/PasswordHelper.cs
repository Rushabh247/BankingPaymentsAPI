namespace BankingPaymentsAPI.Helpers
{
    using System.Security.Cryptography;
    using System.Text;

    namespace BankingPaymentsAPI.Helpers
    {
        public static class PasswordHelper
        {
            // Hash password with SHA256
            public static string Hash(string password)
            {
                using var sha256 = SHA256.Create();
                var bytes = Encoding.UTF8.GetBytes(password);
                var hashBytes = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hashBytes);
            }

            // Verify password by comparing hashes
            public static bool Verify(string password, string storedHash)
            {
                var hashedInput = Hash(password);
                return hashedInput == storedHash;
            }
        }
    }

}
