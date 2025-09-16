namespace BankingPaymentsAPI.DTOs
{
    public class ReportRequestDto
    {
        public int Id { get; set; }
        public int RequestedBy { get; set; }
        public string ReportType { get; set; }
        public string ParametersJson { get; set; }
        public string Status { get; set; }
        public string ResultUrl { get; set; }
        public DateTimeOffset RequestedAt { get; set; }
    }
}
