using BankingPaymentsAPI.Data;
using BankingPaymentsAPI.Models;

using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace BankingPaymentsAPI.Repository
{
    public class BeneficiaryRepository : IBeneficiaryRepository
    {
        private readonly AppDbContext _context;
        public BeneficiaryRepository(AppDbContext context) => _context = context;

        public Beneficiary Add(Beneficiary beneficiary)
        {
            _context.Beneficiaries.Add(beneficiary);
            _context.SaveChanges();
            return beneficiary;
        }

        public Beneficiary? GetById(int id)
        {
            return _context.Beneficiaries
                .AsNoTracking()
                .Include(b => b.Client)
                .FirstOrDefault(b => b.Id == id);
        }

        public IEnumerable<Beneficiary> GetByClientId(int clientId)
        {
            return _context.Beneficiaries
                .AsNoTracking()
                .Where(b => b.ClientId == clientId)
                .ToList();
        }

        public IEnumerable<Beneficiary> GetAll()
        {
            return _context.Beneficiaries
                .AsNoTracking()
                .Include(b => b.Client)
                .ToList();
        }

        public void Update(Beneficiary beneficiary)
        {
            _context.Beneficiaries.Update(beneficiary);
            _context.SaveChanges();
        }

        public void Delete(Beneficiary beneficiary)
        {
            _context.Beneficiaries.Remove(beneficiary);
            _context.SaveChanges();
        }
    }
}
