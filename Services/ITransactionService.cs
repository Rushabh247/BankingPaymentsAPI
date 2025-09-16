using BankingPaymentsAPI.DTOs;
using System.Collections.Generic;

namespace BankingPaymentsAPI.Services
{
    public interface ITransactionService
    {
        TransactionDto RecordTransaction(TransactionDto dto);
        TransactionDto? GetById(int id);
        IEnumerable<TransactionDto> GetByPaymentId(int paymentId);
        IEnumerable<TransactionDto> GetAll();
    }
}
