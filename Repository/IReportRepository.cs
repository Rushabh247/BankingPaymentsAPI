using BankingPaymentsAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BankingPaymentsAPI.Repository
{
    public interface IReportRepository
    {
        Task<ReportRequest> AddAsync(ReportRequest request);
        Task<ReportRequest?> GetByIdAsync(int id);
        Task<IEnumerable<ReportRequest>> GetAllAsync();
        Task UpdateAsync(ReportRequest request);
    }
}
