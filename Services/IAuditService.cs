using BankingPaymentsAPI.DTOs;
using System.Collections.Generic;

namespace BankingPaymentsAPI.Services
{
    public interface IAuditService
    {
        AuditLogDto Log(CreateAuditLogDto dto);
        IEnumerable<AuditLogDto> GetByEntity(string entityName, int entityId);
        IEnumerable<AuditLogDto> GetByUser(int userId);
    }
}
