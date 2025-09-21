using BankingPaymentsAPI.Data;
using BankingPaymentsAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BankingPaymentsAPI.Repository
{
    public class BeneficiaryRepository : IBeneficiaryRepository
    {
        private readonly AppDbContext _context;

        public BeneficiaryRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Beneficiary> AddAsync(Beneficiary beneficiary)
        {
            await _context.Beneficiaries.AddAsync(beneficiary);
            await _context.SaveChangesAsync();
            return beneficiary;
        }

        public async Task<Beneficiary?> GetByIdAsync(int id)
        {
            return await _context.Beneficiaries
                .AsNoTracking()
                .Include(b => b.Client)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<IEnumerable<Beneficiary>> GetByClientIdAsync(int clientId)
        {
            return await _context.Beneficiaries
                .AsNoTracking()
                .Where(b => b.ClientId == clientId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Beneficiary>> GetAllAsync()
        {
            return await _context.Beneficiaries
                .AsNoTracking()
                .Include(b => b.Client)
                .ToListAsync();
        }

        public async Task UpdateAsync(Beneficiary beneficiary)
        {
            if (!_context.Beneficiaries.Local.Any(b => b.Id == beneficiary.Id))
            {
                _context.Beneficiaries.Update(beneficiary);
            }
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Beneficiary beneficiary)
        {
            _context.Beneficiaries.Remove(beneficiary);
            await _context.SaveChangesAsync();
        }
    }
}
