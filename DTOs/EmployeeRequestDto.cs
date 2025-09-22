namespace BankingPaymentsAPI.DTOs
{
    public class EmployeeRequestDto
    {
        public int ClientId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public decimal Salary { get; set; }       
    }
}
