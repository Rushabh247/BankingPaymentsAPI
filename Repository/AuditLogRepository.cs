using BankingPaymentsAPI.Data;
using BankingPaymentsAPI.Models;

using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace BankingPaymentsAPI.Repository
{
    public class AuditLogRepository : IAuditLogRepository
    {
        private readonly AppDbContext _context;
        public AuditLogRepository(AppDbContext context) => _context = context;


        public AuditLog Add(AuditLog log)
        {
            _context.AuditLogs.Add(log);
            _context.SaveChanges();
            return log;
        }
        

        public IEnumerable<AuditLog> GetByEntity(string entityName, int entityId)
        {
            return _context.AuditLogs.Where(a => a.EntityName == entityName && a.EntityId == entityId).ToList();
        }

        public IEnumerable<AuditLog> GetByUser(int userId)
        {
            return _context.AuditLogs.Where(a => a.UserId == userId).ToList();
        }

        public IEnumerable<AuditLog> GetAll()
        {
            return _context.AuditLogs.ToList();
        }
    }
}
