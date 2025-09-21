using BankingPaymentsAPI.DTOs;
using BankingPaymentsAPI.Enums;
using BankingPaymentsAPI.Models;
using BankingPaymentsAPI.Repository;
using BankingPaymentsAPI.Services.Notification;
using BankingPaymentsAPI.Services.PaymentProcessing;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace BankingPaymentsAPI.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepo;
        private readonly IAuditService _audit;
        private readonly IHttpContextAccessor _httpContext;
        private readonly IEmailNotificationService _email;
        private readonly IConfiguration _config;
        private readonly IFundTransferService _fundTransferService;
        private readonly IStripePaymentService _stripe;

        public PaymentService(
            IPaymentRepository paymentRepo,
            IAuditService audit,
            IHttpContextAccessor httpContext,
            IEmailNotificationService email,
            IConfiguration config,
            IFundTransferService fundTransferService,
            IStripePaymentService stripe)
        {
            _paymentRepo = paymentRepo;
            _audit = audit;
            _httpContext = httpContext;
            _email = email;
            _config = config;
            _fundTransferService = fundTransferService;
            _stripe = stripe;
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
                CreatedBy = createdByUserId,
                Method = PaymentMethod.Internal
            };

            payment = _paymentRepo.Add(payment);
            SendPaymentEmail(payment, "Payment Created", $"Payment of {payment.Amount} {payment.Currency} created and pending approval.");

            LogAudit("CREATE", null, payment);

            return MapToDto(payment);
        }

        public PaymentDto CreateStripePayment(PaymentRequestDto request, int createdByUserId)
        {
            var payment = new Payment
            {
                ClientId = request.ClientId,
                BeneficiaryId = request.BeneficiaryId,
                Amount = request.Amount,
                Currency = request.Currency ?? "INR",
                Status = PaymentStatus.PendingApproval,
                CreatedBy = createdByUserId,
                Method = PaymentMethod.Stripe
            };

            // Create Stripe PaymentIntent
            var intent = _stripe.CreatePaymentIntent(payment.Amount, payment.Currency, $"payment_{payment.Id}");
            payment.StripePaymentIntentId = intent.Id;

            payment = _paymentRepo.Add(payment);

            SendPaymentEmail(payment, "Stripe Payment Initiated", $"Stripe payment of {payment.Amount} {payment.Currency} initiated. Use Stripe to complete payment.");
            LogAudit("CREATE_STRIPE_PAYMENT", null, payment);

            return MapToDto(payment);
        }

        public PaymentDto ConfirmStripePayment(string paymentIntentId, int approverId, string remarks)
        {
            var payment = _paymentRepo.GetByStripeId(paymentIntentId);
            if (payment == null) return null;

            var oldValue = JsonSerializer.Serialize(payment);

            // Check Stripe payment status
            var intent = _stripe.GetPaymentIntent(paymentIntentId);
            payment.Status = intent.Status == "succeeded" ? PaymentStatus.Approved : PaymentStatus.Failed;
            payment.ApprovedBy = approverId;
            payment.ApprovedAt = DateTimeOffset.UtcNow;
            payment.ApprovalRemarks = remarks;

            if (payment.Status == PaymentStatus.Approved)
            {
                _fundTransferService.TransferFunds(payment.ClientId, AccountHolderType.Client, payment.BeneficiaryId ?? 0, AccountHolderType.Beneficiary, payment.Amount);
            }

            _paymentRepo.Update(payment);
            SendPaymentEmail(payment, "Stripe Payment Update", $"Payment {payment.Status}. Remarks: {remarks}");
            LogAudit("CONFIRM_STRIPE_PAYMENT", oldValue, payment);

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

            _fundTransferService.TransferFunds(payment.ClientId, AccountHolderType.Client, payment.BeneficiaryId ?? 0, AccountHolderType.Beneficiary, payment.Amount);

            _paymentRepo.Update(payment);
            SendPaymentEmail(payment, "Payment Approved", $"Payment approved. Remarks: {remarks}");
            LogAudit("APPROVE", oldValue, payment);

            return MapToDto(payment);
        }

        public bool DeletePayment(int id)
        {
            var payment = _paymentRepo.GetById(id);
            if (payment == null) return false;

            var oldValue = JsonSerializer.Serialize(payment);
            _paymentRepo.Delete(payment);
            SendPaymentEmail(payment, "Payment Deleted", $"Payment deleted.");
            LogAudit("DELETE", oldValue, null);

            return true;
        }

        #region Helpers
        private PaymentDto MapToDto(Payment p)
        {
            return new PaymentDto
            {
                Id = p.Id,
                ClientId = p.ClientId,
                ClientName = p.Client?.Name ?? "Unknown",
                BeneficiaryId = p.BeneficiaryId,
                BeneficiaryName = p.Beneficiary?.Name ?? "Unknown",
                Amount = p.Amount,
                Currency = p.Currency,
                Status = p.Status,
                ApprovedBy = p.ApprovedBy,
                ApprovedAt = p.ApprovedAt,
                ApprovalRemarks = p.ApprovalRemarks
            };
        }

        private void SendPaymentEmail(Payment p, string subject, string body)
        {
            var fromEmail = _config["Smtp:From"];
            _email.SendEmailAsync(p.Client?.ContactEmail ?? fromEmail, subject, body);
        }

        private void LogAudit(string action, string? oldValueJson, Payment newValue)
        {
            _audit.Log(new CreateAuditLogDto
            {
                UserId = GetCurrentUserId(),
                Action = action,
                EntityName = nameof(Payment),
                EntityId = newValue.Id,
                OldValueJson = oldValueJson,
                NewValueJson = JsonSerializer.Serialize(newValue),
                IpAddress = GetClientIp()
            });
        }

        private int GetCurrentUserId() => int.TryParse(_httpContext.HttpContext?.User?.FindFirst("userId")?.Value, out var id) ? id : 0;
        private string GetClientIp() => _httpContext.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "unknown";
        #endregion
    }
}
