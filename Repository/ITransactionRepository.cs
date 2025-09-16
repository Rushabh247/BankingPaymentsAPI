using BankingPaymentsAPI.Models;
using System.Collections.Generic;

namespace BankingPaymentsAPI.Repository
{
    public interface ITransactionRepository
    {
        Transaction Add(Transaction txn);
        Transaction? GetById(int id);
        IEnumerable<Transaction> GetByPaymentId(int paymentId);
        IEnumerable<Transaction> GetAll();
    }
}
