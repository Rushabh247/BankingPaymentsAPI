using BankingPaymentsAPI.Data;
using BankingPaymentsAPI.Models;

using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace BankingPaymentsAPI.Repository
{
    public class BankRepository : IBankRepository
    {
        private readonly AppDbContext _context;
        public BankRepository(AppDbContext context) => _context = context;

        public Bank Add(Bank bank)
        {
            _context.Banks.Add(bank);
            _context.SaveChanges();
            return bank;
        }

        public Bank? GetById(int id)
        {
            return _context.Banks
                .Include(b => b.Clients)
                .Include(b => b.Users)
                .FirstOrDefault(b => b.Id == id);
        }

        public IEnumerable<Bank> GetAll()
        {
            return _context.Banks.AsNoTracking().ToList();
        }

        public void Update(Bank bank)
        {
            _context.Banks.Update(bank);
            _context.SaveChanges();
        }

        public void Delete(Bank bank)
        {
            _context.Banks.Remove(bank);
            _context.SaveChanges();
        }
    }
}
