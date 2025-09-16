using BankingPaymentsAPI.Models;

using System.Collections.Generic;

namespace BankingPaymentsAPI.Repository
{
    public interface IReportRepository
    {
        ReportRequest Add(ReportRequest request);
        ReportRequest? GetById(int id);
        IEnumerable<ReportRequest> GetAll();
        void Update(ReportRequest request);
    }
}
