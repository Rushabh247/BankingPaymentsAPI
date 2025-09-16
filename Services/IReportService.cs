using BankingPaymentsAPI.DTOs;
using System.Collections.Generic;

namespace BankingPaymentsAPI.Services
{
    public interface IReportService
    {
        ReportRequestDto RequestReport(ReportRequestCreateDto dto, int requestedBy);
        ReportRequestDto? GetById(int id);
        IEnumerable<ReportRequestDto> GetAll();
        void MarkCompleted(int id, string resultUrl);
        void MarkFailed(int id, string reason);
    }
}
