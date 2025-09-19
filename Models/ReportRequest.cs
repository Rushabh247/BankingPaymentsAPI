
using BankingPaymentsAPI.Enums;
namespace BankingPaymentsAPI.Models

{
    public class ReportRequest
    {
        public int Id { get; set; }
        public int RequestedBy { get; set; }

        public ReportType ReportType { get; set; }
        public string ParametersJson { get; set; }

        public ReportStatus Status { get; set; }
        public string? ResultUrl { get; set; }
        public DateTimeOffset RequestedAt { get; set; }
    }
}
