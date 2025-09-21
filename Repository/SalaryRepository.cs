using BankingPaymentsAPI.Data;
using BankingPaymentsAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace BankingPaymentsAPI.Repository
{
    public class SalaryRepository : ISalaryRepository
    {
        private readonly AppDbContext _context;
        public SalaryRepository(AppDbContext context) => _context = context;

        public SalaryBatch AddBatch(SalaryBatch batch)
        {
            _context.SalaryBatches.Add(batch);
            _context.SaveChanges();
            return batch;
        }

        public SalaryBatch? GetBatchById(int id)
        {
            return _context.SalaryBatches
                .Include(b => b.Items)
                .ThenInclude(i => i.Employee)
                .FirstOrDefault(b => b.Id == id);
        }

        public IEnumerable<SalaryBatch> GetBatchesByClient(int clientId)
        {
            return _context.SalaryBatches
                .Include(b => b.Items)
                .ThenInclude(i => i.Employee)
                .Where(b => b.ClientId == clientId)
                .ToList();
        }

        public void UpdateBatch(SalaryBatch batch)
        {
            _context.SalaryBatches.Update(batch);
            _context.SaveChanges();
        }

        public void DeleteBatch(SalaryBatch batch)
        {
            _context.SalaryBatches.Remove(batch);
            _context.SaveChanges();
        }

        public SalaryPayment? GetPaymentById(int id)
        {
            return _context.SalaryPayments
                .Include(p => p.Employee)
                .Include(p => p.SalaryBatch)   
                .FirstOrDefault(p => p.Id == id);
        }

        public void UpdatePayment(SalaryPayment payment)
        {
            _context.SalaryPayments.Update(payment);
            _context.SaveChanges();
        }
    }
}
