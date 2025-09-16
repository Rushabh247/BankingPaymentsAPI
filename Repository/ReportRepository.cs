using BankingPaymentsAPI.Data;
using BankingPaymentsAPI.Models;

using System.Collections.Generic;
using System.Linq;

namespace BankingPaymentsAPI.Repository
{
    public class ReportRepository : IReportRepository
    {
        private readonly AppDbContext _context;
        public ReportRepository(AppDbContext context) => _context = context;

        public ReportRequest Add(ReportRequest request)
        {
            _context.ReportRequests.Add(request);
            _context.SaveChanges();
            return request;
        }

        public ReportRequest? GetById(int id) =>
            _context.ReportRequests.FirstOrDefault(r => r.Id == id);

        public IEnumerable<ReportRequest> GetAll() => _context.ReportRequests.ToList();

        public void Update(ReportRequest request)
        {
            _context.ReportRequests.Update(request);
            _context.SaveChanges();
        }
    }
}
