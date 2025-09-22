using BankingPaymentsAPI.Data;
using BankingPaymentsAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace BankingPaymentsAPI.Repository
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly AppDbContext _context;

        public PaymentRepository(AppDbContext context)
        {
            _context = context;
        }

        // ✅ Add payment safely and return the entity with minimal navigation
        public Payment Add(Payment payment)
        {
            _context.Payments.Add(payment);
            _context.SaveChanges();

            // Reload the payment from database to ensure EF is tracking only one instance
            return _context.Payments
                .AsNoTracking() // Prevent tracking issues
                .Include(p => p.Client) // Only include needed navigation
                .Include(p => p.Beneficiary)
                .Include(p => p.Transactions)
                .First(p => p.Id == payment.Id);
        }

     
        public Payment? GetById(int id)
        {
            return _context.Payments
                .AsNoTracking() // Prevent tracking conflicts
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


        public Payment Update(Payment payment)
        {
            // Step 1: Load the existing tracked payment with navigation properties
            var existingPayment = _context.Payments
                .Include(p => p.Client)
                .Include(p => p.Beneficiary)
                .Include(p => p.Transactions)
                .FirstOrDefault(p => p.Id == payment.Id);

            if (existingPayment == null)
                throw new System.Exception($"Payment with ID {payment.Id} not found.");

            // Step 2: Update only scalar properties
            existingPayment.Amount = payment.Amount;
            existingPayment.Currency = payment.Currency;
            existingPayment.Status = payment.Status;
            existingPayment.Method = payment.Method;
            existingPayment.StripePaymentIntentId = payment.StripePaymentIntentId;
            existingPayment.BankTransactionRef = payment.BankTransactionRef;
            existingPayment.ApprovedAt = payment.ApprovedAt;
            existingPayment.ApprovedBy = payment.ApprovedBy;
            existingPayment.ApprovalRemarks = payment.ApprovalRemarks;

            // Step 3: Save changes
            _context.SaveChanges();

            // Step 4: Return updated entity
            return existingPayment;
        }

        public void Delete(Payment payment)
        {
            var tracked = _context.Payments.Local.FirstOrDefault(p => p.Id == payment.Id);
            if (tracked != null)
            {
                _context.Entry(tracked).State = EntityState.Detached;
            }

            _context.Payments.Remove(payment);
            _context.SaveChanges();
        }

        
        public Payment? GetByStripeId(string paymentIntentId)
        {
            return _context.Payments
                .AsNoTracking()
                .Include(p => p.Client)
                .Include(p => p.Beneficiary)
                .Include(p => p.Transactions)
                .FirstOrDefault(p => p.StripePaymentIntentId == paymentIntentId);
        }
    }
}
