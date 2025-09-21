using BankingPaymentsAPI.Services.Storage;
using BankingPaymentsAPI.Enums;
using BankingPaymentsAPI.Repository;
using System.Text;
using System.Text.Json;
using System.IO;
using System.Threading.Tasks;
using System.Linq;

namespace BankingPaymentsAPI.Services.ReportGeneration
{
    public class CloudinaryReportGeneratorService : IReportGeneratorService
    {
        private readonly IFileStorageService _fileStorage;
        private readonly ITransactionRepository _transactionRepo;
        private readonly ISalaryRepository _salaryRepo;
        private readonly IAuditLogRepository _auditRepo;

        public CloudinaryReportGeneratorService(
            IFileStorageService fileStorage,
            ITransactionRepository transactionRepo,
            ISalaryRepository salaryRepo,
            IAuditLogRepository auditRepo)
        {
            _fileStorage = fileStorage;
            _transactionRepo = transactionRepo;
            _salaryRepo = salaryRepo;
            _auditRepo = auditRepo;
        }

        public async Task<string> GenerateReportAsync(string parametersJson, ReportType reportType)
        {
            var parameters = JsonSerializer.Deserialize<Dictionary<string, string>>(parametersJson);

            int.TryParse(parameters.GetValueOrDefault("clientId"), out var clientId);
            DateTimeOffset.TryParse(parameters.GetValueOrDefault("fromDate"), out var fromDate);
            DateTimeOffset.TryParse(parameters.GetValueOrDefault("toDate"), out var toDate);

            var csv = new StringBuilder();

            switch (reportType)
            {
                case ReportType.TransactionReport:
                    csv.AppendLine("Id,ClientId,Amount,DebitAccount,CreditAccount,Status,TransactionDate");

                    var transactions = _transactionRepo.GetAll()
                        .Where(t =>
                            (!clientId.Equals(0) ? t.Payment.ClientId == clientId : true) &&
                            t.TransactionDate >= fromDate &&
                            t.TransactionDate <= toDate
                        ).ToList();

                    foreach (var t in transactions)
                        csv.AppendLine($"{t.Id},{t.Payment.ClientId},{t.Amount},{t.DebitAccountMasked},{t.CreditAccountMasked},{t.Status},{t.TransactionDate:yyyy-MM-dd HH:mm:ss}");
                    break;

                case ReportType.SalaryDisbursementReport:
                    csv.AppendLine("BatchId,EmployeeId,EmployeeName,Amount,Status,DisbursedAt");

                    var batches = _salaryRepo.GetBatchesByClient(clientId)
                        .Where(b => b.CreatedAt >= fromDate && b.CreatedAt <= toDate)
                        .ToList();

                    foreach (var batch in batches)
                    {
                        foreach (var item in batch.Items)
                        {
                            csv.AppendLine($"{batch.Id},{item.EmployeeId},{item.Employee.FullName},{item.Amount},{item.Status}");
                        }
                    }
                    break;

                case ReportType.AuditLogReport:
                    csv.AppendLine("Id,UserId,Action,EntityName,EntityId,Timestamp");

                    var logs = _auditRepo.GetAll()
                        .Where(a =>
                            (!clientId.Equals(0) ? a.UserId == clientId : true) &&
                            a.Timestamp >= fromDate &&
                            a.Timestamp <= toDate
                        ).ToList();

                    foreach (var log in logs)
                        csv.AppendLine($"{log.Id},{log.UserId},{log.Action},{log.EntityName},{log.EntityId},{log.Timestamp:yyyy-MM-dd HH:mm:ss}");
                    break;
            }

            var fileBytes = Encoding.UTF8.GetBytes(csv.ToString());
            using var ms = new MemoryStream(fileBytes);
            string fileName = $"{reportType}_{DateTimeOffset.UtcNow:yyyyMMddHHmmss}.csv";

            var url = await _fileStorage.UploadFileAsync(ms, fileName, "reports", "csv");

            return url.Url;
        }
    }
}
