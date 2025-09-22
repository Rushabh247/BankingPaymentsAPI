namespace BankingPaymentsAPI.Models
{
    public class AuditLog
    {
        public int Id { get; set; }
        public int UserId { get; set; }

        public string Action { get; set; } // CREATE, UPDATE, DELETE, LOGIN, APPROVE
        public string EntityName { get; set; }
        public int EntityId { get; set; }

        public string? OldValueJson { get; set; }
        public string NewValueJson { get; set; }

        public DateTimeOffset Timestamp { get; set; }
        public string IpAddress { get; set; }
        // test
    }
}
