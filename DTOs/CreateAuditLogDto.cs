namespace BankingPaymentsAPI.DTOs
{
    public class CreateAuditLogDto
    {
        public int UserId { get; set; }
        public string Action { get; set; }
        public string EntityName { get; set; }
        public int EntityId { get; set; }
        public string OldValueJson { get; set; }
        public string NewValueJson { get; set; }
        public string IpAddress { get; set; }
    }
}
