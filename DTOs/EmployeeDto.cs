namespace BankingPaymentsAPI.DTOs
{
    public class EmployeeDto
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string AccountNumberMasked { get; set; } = string.Empty;
        public decimal Salary { get; set; }        
        public decimal Balance { get; set; }
    }
}
