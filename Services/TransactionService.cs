using BankingPaymentsAPI.DTOs;
using BankingPaymentsAPI.Models;
using BankingPaymentsAPI.Repository;
using BankingPaymentsAPI.Services.Notification;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace BankingPaymentsAPI.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly ITransactionRepository _repo;
        private readonly IAuditService _audit;
        private readonly IHttpContextAccessor _httpContext;
        private readonly IEmailNotificationService _email;
        private readonly IConfiguration _config;
        private readonly ISalaryRepository _salaryRepo;
        private readonly IPaymentRepository _paymentRepo;

        public TransactionService(
            ITransactionRepository repo,
            IAuditService audit,
            IHttpContextAccessor httpContext,
            IEmailNotificationService email,
            IConfiguration config,
            ISalaryRepository salaryRepo,
            IPaymentRepository paymentRepo)
        {
            _repo = repo;
            _audit = audit;
            _httpContext = httpContext;
            _email = email;
            _config = config;
            _salaryRepo = salaryRepo;
            _paymentRepo = paymentRepo;
        }

        public TransactionDto RecordTransaction(TransactionDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            string debitAccount = dto.DebitAccountMasked ?? "****";
            string creditAccount = dto.CreditAccountMasked ?? "****";

            Transaction transaction;

            // Salary Payment transaction
            if (dto.SalaryPaymentId.HasValue)
            {
                var salaryPayment = _salaryRepo.GetPaymentById(dto.SalaryPaymentId.Value);
                if (salaryPayment == null)
                    throw new Exception($"SalaryPayment with Id {dto.SalaryPaymentId} not found.");

                transaction = new Transaction
                {
                    PaymentId = null,
                    SalaryPaymentId = salaryPayment.Id,
                    Amount = salaryPayment.Amount,
                    DebitAccountMasked = debitAccount,
                    CreditAccountMasked = creditAccount,
                    TransactionDate = dto.TransactionDate,
                    Status = dto.Status,
                    ExternalTxnRef = salaryPayment.TxnRef
                };

                _repo.Add(transaction);

                // Send email to employee
                SendTransactionEmail(employeeEmail: salaryPayment.Employee?.Email, clientEmail: null, txn: transaction);
            }
            // Regular Payment transaction
            else if (dto.PaymentId.HasValue)
            {
                var payment = _paymentRepo.GetById(dto.PaymentId.Value);
                if (payment == null)
                    throw new Exception($"Payment with Id {dto.PaymentId} not found.");

                transaction = new Transaction
                {
                    PaymentId = payment.Id,
                    SalaryPaymentId = null,
                    Amount = payment.Amount,
                    DebitAccountMasked = debitAccount,
                    CreditAccountMasked = creditAccount,
                    TransactionDate = dto.TransactionDate,
                    Status = dto.Status,
                    ExternalTxnRef = payment.BankTransactionRef ?? payment.StripePaymentIntentId
                };

                _repo.Add(transaction);

                // Send email to client
                SendTransactionEmail(employeeEmail: null, clientEmail: payment.Client?.ContactEmail, txn: transaction);
            }
            else
            {
                throw new Exception("Either PaymentId or SalaryPaymentId must be provided.");
            }

            // Log audit
            LogAudit("RECORD_TRANSACTION", null, transaction);

            return MapToDto(transaction);
        }

        public TransactionDto? GetById(int id)
        {
            var t = _repo.GetById(id);
            return t == null ? null : MapToDto(t);
        }

        public IEnumerable<TransactionDto> GetByPaymentId(int paymentId)
            => _repo.GetByPaymentId(paymentId).Select(MapToDto);

        public IEnumerable<TransactionDto> GetAll()
            => _repo.GetAll().Select(MapToDto);

        #region Helpers

        private TransactionDto MapToDto(Transaction t) =>
            new TransactionDto
            {
                Id = t.Id,
                PaymentId = t.PaymentId,
                SalaryPaymentId = t.SalaryPaymentId,
                Amount = t.Amount,
                DebitAccountMasked = t.DebitAccountMasked,
                CreditAccountMasked = t.CreditAccountMasked,
                TransactionDate = t.TransactionDate,
                Status = t.Status,
                ExternalTxnRef = t.ExternalTxnRef
            };

        private void SendTransactionEmail(string? employeeEmail, string? clientEmail, Transaction txn)
        {
            if (!string.IsNullOrEmpty(clientEmail))
            {
                _email.SendEmailAsync(clientEmail, "Transaction Recorded",
                    $"Transaction of {txn.Amount} recorded. Ref: {txn.ExternalTxnRef}");
            }

            if (!string.IsNullOrEmpty(employeeEmail))
            {
                _email.SendEmailAsync(employeeEmail, "Transaction Notification",
                    $"You have received a transaction of {txn.Amount}. Ref: {txn.ExternalTxnRef}");
            }
        }

        private void LogAudit(string action, string? oldValueJson, Transaction newValue)
        {
            _audit.Log(new CreateAuditLogDto
            {
                UserId = GetCurrentUserId(),
                Action = action,
                EntityName = nameof(Transaction),
                EntityId = newValue.Id,
                OldValueJson = oldValueJson,
                NewValueJson = JsonSerializer.Serialize(newValue),
                IpAddress = GetClientIp()
            });
        }

        private int GetCurrentUserId()
            => int.TryParse(_httpContext.HttpContext?.User?.FindFirst("userId")?.Value, out var id) ? id : 0;

        private string GetClientIp()
            => _httpContext.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "unknown";

        #endregion
    }
}
