using System.Xml.Linq;
using BankingPaymentsAPI.Enums;

namespace BankingPaymentsAPI.Models
{
    public class Document
    {
        public int Id { get; set; }

        public int ClientId { get; set; }
        public Client Client { get; set; }

        public string FileName { get; set; }
        public string MimeType { get; set; }
        public long SizeBytes { get; set; }

        public string CloudinaryPublicId { get; set; }
        public string Url { get; set; }

        public DocumentType Type { get; set; }
        public DateTimeOffset UploadedAt { get; set; }
        public int UploadedBy { get; set; }
    }
}
