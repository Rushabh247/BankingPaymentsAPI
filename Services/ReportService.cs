using BankingPaymentsAPI.DTOs;
using BankingPaymentsAPI.Enums;
using BankingPaymentsAPI.Models;
using BankingPaymentsAPI.Repository;

using System;
using System.Collections.Generic;
using System.Linq;

namespace BankingPaymentsAPI.Services
{
    public class ReportService : IReportService
    {
        private readonly IReportRepository _repo;
        public ReportService(IReportRepository repo) => _repo = repo;

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
            r.Status = ReportStatus.Completed;
            r.ResultUrl = resultUrl;
            _repo.Update(r);
        }

        public void MarkFailed(int id, string reason)
        {
            var r = _repo.GetById(id);
            if (r == null) return;
            r.Status = ReportStatus.Failed;
            r.ResultUrl = reason;
            _repo.Update(r);
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
    }
}
