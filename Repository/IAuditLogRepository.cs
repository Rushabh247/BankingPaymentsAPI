using BankingPaymentsAPI.Models;

using System.Collections.Generic;

namespace BankingPaymentsAPI.Repository
{
    public interface IAuditLogRepository
    {
        AuditLog Add(AuditLog log);
        IEnumerable<AuditLog> GetByEntity(string entityName, int entityId);
        IEnumerable<AuditLog> GetByUser(int userId);
        IEnumerable<AuditLog> GetAll();
    }
}
