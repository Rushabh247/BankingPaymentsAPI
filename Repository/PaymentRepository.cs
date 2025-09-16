using BankingPaymentsAPI.Data;
using BankingPaymentsAPI.Models;

using Microsoft.EntityFrameworkCore;

namespace BankingPaymentsAPI.Repository
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly AppDbContext _context;

        public PaymentRepository(AppDbContext context)
        {
            _context = context;
        }

        public Payment Add(Payment payment)
        {
            _context.Payments.Add(payment);
            _context.SaveChanges();
            return payment;
        }

        public Payment? GetById(int id)
        {
            return _context.Payments
                .AsNoTracking()
                .Include(p => p.Client)
                .Include(p => p.Beneficiary)
                .Include(p => p.Transactions)
                .FirstOrDefault(p => p.Id == id);
        }

        public IEnumerable<Payment> GetAll()
        {
            return _context.Payments
                .AsNoTracking()
                .Include(p => p.Client)
                .Include(p => p.Beneficiary)
                .ToList();
        }

        public void Update(Payment payment)
        {
            _context.Payments.Update(payment);
            _context.SaveChanges();
        }

        public void Delete(Payment payment)
        {
            _context.Payments.Remove(payment);
            _context.SaveChanges();
        }
    }
}
