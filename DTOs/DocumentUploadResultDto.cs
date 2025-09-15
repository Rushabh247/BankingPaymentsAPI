namespace BankingPaymentsAPI.DTOs
{
    public class DocumentUploadResultDto
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public string FileName { get; set; }
        public string MimeType { get; set; }
        public long SizeBytes { get; set; }
        public string Url { get; set; }
        public string CloudinaryPublicId { get; set; }
        public string Type { get; set; }
        public DateTimeOffset UploadedAt { get; set; }
        public int UploadedBy { get; set; }
    }
}
