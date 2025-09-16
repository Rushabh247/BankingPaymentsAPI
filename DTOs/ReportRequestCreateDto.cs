using BankingPaymentsAPI.Enums;

namespace BankingPaymentsAPI.DTOs
{
    public class ReportRequestCreateDto
    {
        public ReportType ReportType { get; set; }
        public string ParametersJson { get; set; }
    }
}
