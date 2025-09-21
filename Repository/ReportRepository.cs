using BankingPaymentsAPI.Data;
using BankingPaymentsAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BankingPaymentsAPI.Repository
{
    public class ReportRepository : IReportRepository
    {
        private readonly AppDbContext _context;
        public ReportRepository(AppDbContext context) => _context = context;

        public async Task<ReportRequest> AddAsync(ReportRequest request)
        {
            await _context.ReportRequests.AddAsync(request);
            await _context.SaveChangesAsync();
            return request;
        }

        public async Task<ReportRequest?> GetByIdAsync(int id) =>
            await _context.ReportRequests.FirstOrDefaultAsync(r => r.Id == id);

        public async Task<IEnumerable<ReportRequest>> GetAllAsync() =>
            await _context.ReportRequests.ToListAsync();

        public async Task UpdateAsync(ReportRequest request)
        {
            _context.ReportRequests.Update(request);
            await _context.SaveChangesAsync();
        }
    }
}
