namespace BankingPaymentsAPI.Models
{
    public class Employee
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public Client Client { get; set; }

        public string EmployeeCode { get; set; }
        public string FullName { get; set; }
        public string AccountNumberEncrypted { get; set; }
        public decimal Salary { get; set; }
        public string PAN { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
