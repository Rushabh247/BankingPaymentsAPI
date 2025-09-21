using System.Threading.Tasks;

namespace BankingPaymentsAPI.Services.ReportGeneration
{
    public interface IReportGeneratorService
    {
        Task<string> GenerateReportAsync(string parametersJson, BankingPaymentsAPI.Enums.ReportType reportType);
    }
}
