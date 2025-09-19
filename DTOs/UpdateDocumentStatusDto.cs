namespace BankingPaymentsAPI.DTOs
{
    public class UpdateDocumentStatusDto
    {
        public int DocumentId { get; set; }
        public string Status { get; set; } // Pending, Verified, Rejected
    }
}
