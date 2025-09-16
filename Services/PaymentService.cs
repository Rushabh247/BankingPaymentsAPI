using BankingPaymentsAPI.DTOs;
using BankingPaymentsAPI.Enums;
using BankingPaymentsAPI.Models;
using BankingPaymentsAPI.Repository;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace BankingPaymentsAPI.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepo;
        private readonly IAuditService _audit;
        private readonly IHttpContextAccessor _httpContext;

        public PaymentService(
            IPaymentRepository paymentRepo,
            IAuditService audit,
            IHttpContextAccessor httpContext)
        {
            _paymentRepo = paymentRepo;
            _audit = audit;
            _httpContext = httpContext;
        }

        public PaymentDto CreatePayment(PaymentRequestDto request, int createdByUserId)
        {
            var payment = new Payment
            {
                ClientId = request.ClientId,
                BeneficiaryId = request.BeneficiaryId,
                Amount = request.Amount,
                Currency = request.Currency ?? "INR",
                Status = PaymentStatus.PendingApproval,
                CreatedBy = createdByUserId
            };

            _paymentRepo.Add(payment);

            //  Log CREATE
            _audit.Log(new CreateAuditLogDto
            {
                UserId = GetCurrentUserId(),
                Action = "CREATE",
                EntityName = nameof(Payment),
                EntityId = payment.Id,
                OldValueJson = null,
                NewValueJson = JsonSerializer.Serialize(payment),
                IpAddress = GetClientIp()
            });

            return MapToDto(payment);
        }

        public PaymentDto? GetPaymentById(int id)
        {
            var payment = _paymentRepo.GetById(id);
            return payment == null ? null : MapToDto(payment);
        }

        public IEnumerable<PaymentDto> GetAllPayments()
        {
            return _paymentRepo.GetAll().Select(MapToDto);
        }

        public PaymentDto? ApprovePayment(int id, int approverId, string remarks)
        {
            var payment = _paymentRepo.GetById(id);
            if (payment == null) return null;

            var oldValue = JsonSerializer.Serialize(payment);

            payment.Status = PaymentStatus.Approved;
            payment.ApprovedBy = approverId;
            payment.ApprovedAt = DateTimeOffset.UtcNow;
            payment.ApprovalRemarks = remarks;

            _paymentRepo.Update(payment);

            //  Log UPDATE (Approval)
            _audit.Log(new CreateAuditLogDto
            {
                UserId = GetCurrentUserId(),
                Action = "APPROVE",
                EntityName = nameof(Payment),
                EntityId = payment.Id,
                OldValueJson = oldValue,
                NewValueJson = JsonSerializer.Serialize(payment),
                IpAddress = GetClientIp()
            });

            return MapToDto(payment);
        }

        public bool DeletePayment(int id)
        {
            var payment = _paymentRepo.GetById(id);
            if (payment == null) return false;

            var oldValue = JsonSerializer.Serialize(payment);

            _paymentRepo.Delete(payment);

            //  Log DELETE
            _audit.Log(new CreateAuditLogDto
            {
                UserId = GetCurrentUserId(),
                Action = "DELETE",
                EntityName = nameof(Payment),
                EntityId = payment.Id,
                OldValueJson = oldValue,
                NewValueJson = null,
                IpAddress = GetClientIp()
            });

            return true;
        }

        private PaymentDto MapToDto(Payment payment)
        {
            return new PaymentDto
            {
                Id = payment.Id,
                ClientName = payment.Client?.Name ?? "Unknown",
                BeneficiaryName = payment.Beneficiary?.Name ?? "Unknown",
                Amount = payment.Amount,
                Status = payment.Status.ToString(),
                ApprovedBy = payment.ApprovedBy,
                ApprovedAt = payment.ApprovedAt
            };
        }

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
