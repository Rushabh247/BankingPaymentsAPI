using BankingPaymentsAPI.DTOs;
using BankingPaymentsAPI.Enums;
using BankingPaymentsAPI.Models;
using BankingPaymentsAPI.Repository;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace BankingPaymentsAPI.Services
{
    public class ReportService : IReportService
    {
        private readonly IReportRepository _repo;
        private readonly IAuditService _audit;
        private readonly IHttpContextAccessor _httpContext;

        public ReportService(IReportRepository repo, IAuditService audit, IHttpContextAccessor httpContext)
        {
            _repo = repo;
            _audit = audit;
            _httpContext = httpContext;
        }

        public ReportRequestDto RequestReport(ReportRequestCreateDto dto, int requestedBy)
        {
            var r = new ReportRequest
            {
                RequestedBy = requestedBy,
                ReportType = dto.ReportType,
                ParametersJson = dto.ParametersJson,
                Status = ReportStatus.Pending,
                RequestedAt = DateTimeOffset.UtcNow
            };

            _repo.Add(r);

            // Log CREATE
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

        public ReportRequestDto? GetById(int id)
        {
            var r = _repo.GetById(id);
            return r == null ? null : MapToDto(r);
        }

        public IEnumerable<ReportRequestDto> GetAll() => _repo.GetAll().Select(MapToDto);

        public void MarkCompleted(int id, string resultUrl)
        {
            var r = _repo.GetById(id);
            if (r == null) return;

            var oldValue = JsonSerializer.Serialize(r);

            r.Status = ReportStatus.Completed;
            r.ResultUrl = resultUrl;
            _repo.Update(r);

            // Log UPDATE
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

        public void MarkFailed(int id, string reason)
        {
            var r = _repo.GetById(id);
            if (r == null) return;

            var oldValue = JsonSerializer.Serialize(r);

            r.Status = ReportStatus.Failed;
            r.ResultUrl = reason;
            _repo.Update(r);

            //  Log UPDATE
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

        //  Helpers
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
