namespace BankingPaymentsAPI.DTOs
{
    public class EmployeeDto
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public string EmployeeCode { get; set; }
        public string FullName { get; set; }
        public string AccountNumberMasked { get; set; }

        public string Email { get; set; }
        public decimal Salary { get; set; }
        public string PAN { get; set; }
        public bool IsActive { get; set; }
    }
}
