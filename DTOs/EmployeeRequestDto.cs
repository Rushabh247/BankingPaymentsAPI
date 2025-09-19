namespace BankingPaymentsAPI.DTOs
{
    public class EmployeeRequestDto
    {
        public int ClientId { get; set; }
        public string EmployeeCode { get; set; }
        public string FullName { get; set; }
        public string AccountNumber { get; set; }// plain; service encrypts

        public string Email { get; set; }
        public decimal Salary { get; set; }
        public string PAN { get; set; }
    }
}
