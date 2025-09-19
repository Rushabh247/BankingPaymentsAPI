using BankingPaymentsAPI.DTOs;
using BankingPaymentsAPI.Enums;
using BankingPaymentsAPI.Models;
using BankingPaymentsAPI.Repository;
using BankingPaymentsAPI.Services.Notification;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace BankingPaymentsAPI.Services
{
    public class SalaryService : ISalaryService
    {
        private readonly ISalaryRepository _repo;
        private readonly IEmployeeRepository _employeeRepo;
        private readonly IAuditService _audit;
        private readonly IHttpContextAccessor _httpContext;
        private readonly IEmailNotificationService _email;
        private readonly IConfiguration _config;

        public SalaryService(
            ISalaryRepository repo,
            IEmployeeRepository employeeRepo,
            IAuditService audit,
            IHttpContextAccessor httpContext,
            IEmailNotificationService email,
            IConfiguration config)
        {
            _repo = repo;
            _employeeRepo = employeeRepo;
            _audit = audit;
            _httpContext = httpContext;
            _email = email;
            _config = config;
        }

        // ✅ Create a new salary batch
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
                    Status = PaymentStatus.Draft,
                    TxnRef = Guid.NewGuid().ToString()
                }).ToList()
            };

            _repo.AddBatch(batch);

            // Notify client
            var fromEmail = _config["Smtp:From"];
            _email.SendEmailAsync(
                batch.Client?.ContactEmail ?? fromEmail,
                "Salary Batch Created",
                $"A new salary batch ({batch.BatchCode}) has been created with {batch.Items.Count} employees."
            );

            // Log CREATE
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

        // ✅ Submit salary batch (process all payments)
        public SalaryBatchDto? SubmitBatch(int id, int submittedBy)
        {
            var b = _repo.GetBatchById(id);
            if (b == null) return null;

            var oldValue = JsonSerializer.Serialize(b);

            b.Status = BatchStatus.Submitted;
            foreach (var p in b.Items)
            {
                // simulate processing
                if (p.Amount > 100000) // example failure condition
                {
                    p.Status = PaymentStatus.Failed;
                    p.FailureReason = "Amount exceeds transfer limit";
                }
                else
                {
                    p.Status = PaymentStatus.Processed;
                    p.FailureReason = null;
                }
            }

            _repo.UpdateBatch(b);

            // Send notification
            var fromEmail = _config["Smtp:From"];
            _email.SendEmailAsync(
                b.Client?.ContactEmail ?? fromEmail,
                "Salary Batch Submitted",
                $"Salary batch ({b.BatchCode}) submitted. " +
                $"Processed {b.Items.Count(i => i.Status == PaymentStatus.Processed)} " +
                $"and Failed {b.Items.Count(i => i.Status == PaymentStatus.Failed)} payments."
            );

            // Log SUBMIT
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

        // Retry or process a specific payment
        public SalaryPaymentDto? ProcessPayment(int paymentId)
        {
            var batch = _repo.GetBatchById(paymentId);
            var payment = batch?.Items.FirstOrDefault(p => p.Id == paymentId);
            if (payment == null) return null;

            var oldValue = JsonSerializer.Serialize(payment);

            if (payment.Amount > 100000)
            {
                payment.Status = PaymentStatus.Failed;
                payment.FailureReason = "Amount exceeds transfer limit (retry failed)";
            }
            else
            {
                payment.Status = PaymentStatus.Processed;
                payment.FailureReason = null;
            }

            _repo.UpdatePayment(payment);

            // Notify employee
            if (!string.IsNullOrEmpty(payment.Employee?.Email))
            {
                _email.SendEmailAsync(
                    payment.Employee.Email,
                    "Salary Payment Update",
                    $"Salary payment for {payment.Employee.FullName} is {payment.Status}. " +
                    $"{(payment.Status == PaymentStatus.Failed ? "Reason: " + payment.FailureReason : "")}"
                );
            }

            // Log
            _audit.Log(new CreateAuditLogDto
            {
                UserId = GetCurrentUserId(),
                Action = "PROCESS_PAYMENT",
                EntityName = nameof(SalaryPayment),
                EntityId = payment.Id,
                OldValueJson = oldValue,
                NewValueJson = JsonSerializer.Serialize(payment),
                IpAddress = GetClientIp()
            });

            return new SalaryPaymentDto
            {
                Id = payment.Id,
                EmployeeId = payment.EmployeeId,
                EmployeeName = payment.Employee?.FullName ?? "Unknown",
                Amount = payment.Amount,
                Status = payment.Status.ToString(),
                TxnRef = payment.TxnRef,
                FailureReason = payment.FailureReason
            };
        }

        //  Get batch by Id
        public SalaryBatchDto? GetBatchById(int id)
        {
            var b = _repo.GetBatchById(id);
            return b == null ? null : MapToDto(b);
        }

        //  Get batches by client
        public IEnumerable<SalaryBatchDto> GetBatchesByClient(int clientId)
        {
            return _repo.GetBatchesByClient(clientId).Select(MapToDto);
        }

        // Delete batch
        public bool DeleteBatch(int id)
        {
            var b = _repo.GetBatchById(id);
            if (b == null) return false;

            _repo.DeleteBatch(b);

            // log + notify
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

            var fromEmail = _config["Smtp:From"];
            _email.SendEmailAsync(
                b.Client?.ContactEmail ?? fromEmail,
                "Salary Batch Deleted",
                $"Salary batch ({b.BatchCode}) has been deleted."
            );

            return true;
        }

        //  Helper: map model to DTO
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
                    TxnRef = i.TxnRef,
                    FailureReason = i.FailureReason
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
