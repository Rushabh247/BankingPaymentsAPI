using BankingPaymentsAPI.DTOs;
using BankingPaymentsAPI.Repository;
using BankingPaymentsAPI.Models;
using System;
using System.Linq;
using System.Collections.Generic;

namespace BankingPaymentsAPI.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly ITransactionRepository _repo;
        public TransactionService(ITransactionRepository repo) => _repo = repo;

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
            dto.Id = txn.Id;
            dto.TransactionDate = txn.TransactionDate;
            return dto;
        }

        public TransactionDto? GetById(int id)
        {
            var t = _repo.GetById(id);
            if (t == null) return null;
            return MapToDto(t);
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
    }
}
