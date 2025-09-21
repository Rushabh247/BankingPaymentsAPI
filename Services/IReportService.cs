using BankingPaymentsAPI.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BankingPaymentsAPI.Services
{
    public interface IReportService
    {
        Task<ReportRequestDto> RequestReportAsync(ReportRequestCreateDto dto, int requestedBy);
        Task<ReportRequestDto?> GetByIdAsync(int id);
      

        Task<IEnumerable<ReportRequestDto>> GetReportsAsync(int? clientId = null, string fromDate = null, string toDate = null);

        Task<object> GetReportDataByTypeAsync(string reportType, int? clientId = null);
        Task GenerateAndCompleteReportAsync(int id);
        Task MarkFailedAsync(int id, string reason);
    }
}
