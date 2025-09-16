using BankingPaymentsAPI.DTOs;
using BankingPaymentsAPI.Enums;
using BankingPaymentsAPI.Models;
using BankingPaymentsAPI.Repository;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace BankingPaymentsAPI.Services
{
    public class SalaryService : ISalaryService
    {
        private readonly ISalaryRepository _repo;
        private readonly IEmployeeRepository _employeeRepo;
        private readonly IAuditService _audit;
        private readonly IHttpContextAccessor _httpContext;

        public SalaryService(
            ISalaryRepository repo,
            IEmployeeRepository employeeRepo,
            IAuditService audit,
            IHttpContextAccessor httpContext)
        {
            _repo = repo;
            _employeeRepo = employeeRepo;
            _audit = audit;
            _httpContext = httpContext;
        }

        public SalaryBatchDto CreateBatch(SalaryBatchRequestDto request, int createdBy)
        {
            var batch = new SalaryBatch
            {
                ClientId = request.ClientId,
                BatchCode = request.BatchCode,
                Status = BatchStatus.Created,
                TotalAmount = request.Items.Sum(i => i.Amount),
                Items = request.Items.Select(i => new SalaryPayment
                {
                    EmployeeId = i.EmployeeId,
                    Amount = i.Amount,
                    Status = PaymentStatus.Draft
                }).ToList()
            };

            _repo.AddBatch(batch);

            //  Log CREATE
            _audit.Log(new CreateAuditLogDto
            {
                UserId = GetCurrentUserId(),
                Action = "CREATE_BATCH",
                EntityName = nameof(SalaryBatch),
                EntityId = batch.Id,
                OldValueJson = null,
                NewValueJson = JsonSerializer.Serialize(batch),
                IpAddress = GetClientIp()
            });

            return MapToDto(batch);
        }

        public SalaryBatchDto? GetBatchById(int id)
        {
            var b = _repo.GetBatchById(id);
            return b == null ? null : MapToDto(b);
        }

        public IEnumerable<SalaryBatchDto> GetBatchesByClient(int clientId)
        {
            return _repo.GetBatchesByClient(clientId).Select(MapToDto);
        }

        public SalaryBatchDto? SubmitBatch(int id, int submittedBy)
        {
            var b = _repo.GetBatchById(id);
            if (b == null) return null;

            var oldValue = JsonSerializer.Serialize(b);

            b.Status = BatchStatus.Submitted;
            _repo.UpdateBatch(b);

            // Log UPDATE
            _audit.Log(new CreateAuditLogDto
            {
                UserId = GetCurrentUserId(),
                Action = "SUBMIT_BATCH",
                EntityName = nameof(SalaryBatch),
                EntityId = b.Id,
                OldValueJson = oldValue,
                NewValueJson = JsonSerializer.Serialize(b),
                IpAddress = GetClientIp()
            });

            return MapToDto(b);
        }

        public bool DeleteBatch(int id)
        {
            var b = _repo.GetBatchById(id);
            if (b == null) return false;

            _repo.DeleteBatch(b);

            //  Log DELETE
            _audit.Log(new CreateAuditLogDto
            {
                UserId = GetCurrentUserId(),
                Action = "DELETE_BATCH",
                EntityName = nameof(SalaryBatch),
                EntityId = b.Id,
                OldValueJson = JsonSerializer.Serialize(b),
                NewValueJson = null,
                IpAddress = GetClientIp()
            });

            return true;
        }

        private SalaryBatchDto MapToDto(SalaryBatch b) =>
            new SalaryBatchDto
            {
                Id = b.Id,
                ClientId = b.ClientId,
                BatchCode = b.BatchCode,
                TotalAmount = b.TotalAmount,
                Status = b.Status.ToString(),
                Items = b.Items?.Select(i => new SalaryPaymentDto
                {
                    Id = i.Id,
                    EmployeeId = i.EmployeeId,
                    EmployeeName = i.Employee?.FullName ?? "Unknown",
                    Amount = i.Amount,
                    Status = i.Status.ToString(),
                    TxnRef = i.TxnRef
                }).ToList() ?? new List<SalaryPaymentDto>()
            };

        // Helpers
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
