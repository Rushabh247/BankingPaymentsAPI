using BankingPaymentsAPI.DTOs;
using BankingPaymentsAPI.Enums;
using BankingPaymentsAPI.Models;
using BankingPaymentsAPI.Repository;
using BankingPaymentsAPI.Services.ReportGeneration;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace BankingPaymentsAPI.Services
{
    public class ReportService : IReportService
    {
        private readonly IReportRepository _repo;
        private readonly IAuditService _audit;
        private readonly IHttpContextAccessor _httpContext;
        private readonly IReportGeneratorService _reportGenerator;
        private readonly ITransactionService _transactionService;
        private readonly ISalaryRepository _salaryRepo;

        public ReportService(
            IReportRepository repo,
            IAuditService audit,
            IHttpContextAccessor httpContext,
            IReportGeneratorService reportGenerator,
            ITransactionService transactionService,
            ISalaryRepository salaryRepo)
        {
            _repo = repo;
            _audit = audit;
            _httpContext = httpContext;
            _reportGenerator = reportGenerator;
            _transactionService = transactionService;
            _salaryRepo = salaryRepo;
        }

        public async Task<ReportRequestDto> RequestReportAsync(ReportRequestCreateDto dto, int requestedBy)
        {
            var r = new ReportRequest
            {
                RequestedBy = requestedBy,
                ReportType = (ReportType)dto.ReportType,
                ParametersJson = dto.ParametersJson,
                Status = ReportStatus.Pending,
                RequestedAt = DateTimeOffset.UtcNow
            };

            await _repo.AddAsync(r);

            _audit.Log(new CreateAuditLogDto
            {
                UserId = GetCurrentUserId(),
                Action = "REQUEST_REPORT",
                EntityName = nameof(ReportRequest),
                EntityId = r.Id,
                OldValueJson = null,
                NewValueJson = JsonSerializer.Serialize(r),
                IpAddress = GetClientIp()
            });

            return MapToDto(r);
        }

        public async Task<IEnumerable<ReportRequestDto>> GetReportsAsync(int? clientId = null, string fromDateStr = null, string toDateStr = null)
        {
            var reports = await _repo.GetAllAsync();

            DateTimeOffset? fromDate = null;
            DateTimeOffset? toDate = null;
            if (DateTimeOffset.TryParse(fromDateStr, out var f)) fromDate = f;
            if (DateTimeOffset.TryParse(toDateStr, out var t)) toDate = t;

            return reports.Where(r =>
            {
                var parameters = JsonSerializer.Deserialize<Dictionary<string, object>>(r.ParametersJson);
                bool clientMatch = !clientId.HasValue ||
                    (parameters.ContainsKey("clientId") && parameters["clientId"]?.ToString() == clientId.Value.ToString());

                bool dateMatch = true;
                if (fromDate.HasValue && toDate.HasValue)
                {
                    if (parameters.ContainsKey("fromDate") && parameters.ContainsKey("toDate"))
                    {
                        var reportFrom = DateTimeOffset.Parse(parameters["fromDate"].ToString());
                        var reportTo = DateTimeOffset.Parse(parameters["toDate"].ToString());
                        dateMatch = reportFrom >= fromDate && reportTo <= toDate;
                    }
                }

                return clientMatch && dateMatch;
            }).Select(MapToDto);
        }

        public async Task<object> GetReportDataByTypeAsync(string reportType, int? clientId = null)
        {
            if (Enum.TryParse<ReportType>(reportType, true, out var type))
            {
                switch (type)
                {
                    case ReportType.TransactionReport:
                        return _transactionService.GetAll();

                    case ReportType.SalaryDisbursementReport:
                        if (clientId.HasValue)
                            return _salaryRepo.GetBatchesByClient(clientId.Value);
                        return _salaryRepo.GetBatchesByClient(0);

                    case ReportType.AuditLogReport:
                        return _audit.GetAll();

                    default:
                        throw new ArgumentException("Unsupported report type");
                }
            }
            throw new ArgumentException("Invalid report type");
        }

        public async Task<ReportRequestDto?> GetByIdAsync(int id)
        {
            var r = await _repo.GetByIdAsync(id);
            return r == null ? null : MapToDto(r);
        }

        public async Task GenerateAndCompleteReportAsync(int id)
        {
            var r = await _repo.GetByIdAsync(id);
            if (r == null) return;

            var oldValue = JsonSerializer.Serialize(r);

            var reportTypeEnum = (ReportType)r.ReportType;
            var url = await _reportGenerator.GenerateReportAsync(r.ParametersJson, reportTypeEnum);

            r.Status = ReportStatus.Completed;
            r.ResultUrl = url;
            await _repo.UpdateAsync(r);

            _audit.Log(new CreateAuditLogDto
            {
                UserId = GetCurrentUserId(),
                Action = "MARK_COMPLETED",
                EntityName = nameof(ReportRequest),
                EntityId = r.Id,
                OldValueJson = oldValue,
                NewValueJson = JsonSerializer.Serialize(r),
                IpAddress = GetClientIp()
            });
        }

        public async Task MarkFailedAsync(int id, string reason)
        {
            var r = await _repo.GetByIdAsync(id);
            if (r == null) return;

            var oldValue = JsonSerializer.Serialize(r);

            r.Status = ReportStatus.Failed;
            r.ResultUrl = reason;
            await _repo.UpdateAsync(r);

            _audit.Log(new CreateAuditLogDto
            {
                UserId = GetCurrentUserId(),
                Action = "MARK_FAILED",
                EntityName = nameof(ReportRequest),
                EntityId = r.Id,
                OldValueJson = oldValue,
                NewValueJson = JsonSerializer.Serialize(r),
                IpAddress = GetClientIp()
            });
        }

        private ReportRequestDto MapToDto(ReportRequest r) =>
            new ReportRequestDto
            {
                Id = r.Id,
                RequestedBy = r.RequestedBy,
                ReportType = r.ReportType.ToString(),
                ParametersJson = r.ParametersJson,
                Status = r.Status.ToString(),
                ResultUrl = r.ResultUrl,
                RequestedAt = r.RequestedAt
            };

        private int GetCurrentUserId()
        {
            var userIdClaim = _httpContext.HttpContext?.User?.FindFirst("userId")?.Value;
            return int.TryParse(userIdClaim, out var id) ? id : 0;
        }

        private string GetClientIp()
        {
            return _httpContext.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "unknown";
        }
    }
}
