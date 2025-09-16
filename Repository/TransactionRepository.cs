using BankingPaymentsAPI.Models;
using BankingPaymentsAPI.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace BankingPaymentsAPI.Repository
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly AppDbContext _context;
        public TransactionRepository(AppDbContext context) => _context = context;

        public Transaction Add(Transaction txn)
        {
            _context.Transactions.Add(txn);
            _context.SaveChanges();
            return txn;
        }

        public Transaction? GetById(int id)
        {
            return _context.Transactions.Include(t => t.Payment).FirstOrDefault(t => t.Id == id);
        }

        public IEnumerable<Transaction> GetByPaymentId(int paymentId)
        {
            return _context.Transactions.Where(t => t.PaymentId == paymentId).ToList();
        }

        public IEnumerable<Transaction> GetAll()
        {
            return _context.Transactions.Include(t => t.Payment).ToList();
        }
    }
}
