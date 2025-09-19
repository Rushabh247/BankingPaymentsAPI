namespace BankingPaymentsAPI.DTOs
{
    public class DocumentDto
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public string FileName { get; set; }
        public string Url { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public DateTimeOffset UploadedAt { get; set; }
    }
}
