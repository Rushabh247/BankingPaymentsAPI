using BankingPaymentsAPI.DTOs;
using BankingPaymentsAPI.Repository;
using BankingPaymentsAPI.Models;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace BankingPaymentsAPI.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly ITransactionRepository _repo;
        private readonly IAuditService _audit;
        private readonly IHttpContextAccessor _httpContext;

        public TransactionService(ITransactionRepository repo, IAuditService audit, IHttpContextAccessor httpContext)
        {
            _repo = repo;
            _audit = audit;
            _httpContext = httpContext;
        }

        public TransactionDto RecordTransaction(TransactionDto dto)
        {
            var txn = new Transaction
            {
                PaymentId = dto.PaymentId,
                Amount = dto.Amount,
                DebitAccountMasked = dto.DebitAccountMasked,
                CreditAccountMasked = dto.CreditAccountMasked,
                TransactionDate = dto.TransactionDate == default ? DateTimeOffset.UtcNow : dto.TransactionDate,
                Status = dto.Status,
                ExternalTxnRef = dto.ExternalTxnRef
            };

            _repo.Add(txn);

            //  Log CREATE
            _audit.Log(new CreateAuditLogDto
            {
                UserId = GetCurrentUserId(),
                Action = "CREATE_TRANSACTION",
                EntityName = nameof(Transaction),
                EntityId = txn.Id,
                OldValueJson = null,
                NewValueJson = JsonSerializer.Serialize(txn),
                IpAddress = GetClientIp()
            });

            dto.Id = txn.Id;
            dto.TransactionDate = txn.TransactionDate;
            return dto;
        }

        public TransactionDto? GetById(int id)
        {
            var t = _repo.GetById(id);
            return t == null ? null : MapToDto(t);
        }

        public IEnumerable<TransactionDto> GetByPaymentId(int paymentId)
        {
            return _repo.GetByPaymentId(paymentId).Select(MapToDto);
        }

        public IEnumerable<TransactionDto> GetAll()
        {
            return _repo.GetAll().Select(MapToDto);
        }

        private TransactionDto MapToDto(Transaction t) =>
            new TransactionDto
            {
                Id = t.Id,
                PaymentId = t.PaymentId,
                Amount = t.Amount,
                DebitAccountMasked = t.DebitAccountMasked,
                CreditAccountMasked = t.CreditAccountMasked,
                TransactionDate = t.TransactionDate,
                Status = t.Status,
                ExternalTxnRef = t.ExternalTxnRef
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
