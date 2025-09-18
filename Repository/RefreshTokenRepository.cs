using BankingPaymentsAPI.Data;
using BankingPaymentsAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BankingPaymentsAPI.Repository
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly AppDbContext _context;
        public RefreshTokenRepository(AppDbContext context) => _context = context;

        public async Task<RefreshToken> AddAsync(RefreshToken token)
        {
            _context.RefreshTokens.Add(token);
            await _context.SaveChangesAsync();
            return token;
        }

        public Task<RefreshToken?> GetByTokenAsync(string token) =>
            _context.RefreshTokens
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Token == token);

        public Task<List<RefreshToken>> GetByUserIdAsync(int userId) =>
            _context.RefreshTokens
                .Where(t => t.UserId == userId)
                .ToListAsync();

        public async Task UpdateAsync(RefreshToken token)
        {
            _context.RefreshTokens.Update(token);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(RefreshToken token)
        {
            _context.RefreshTokens.Remove(token);
            await _context.SaveChangesAsync();
        }
    }
}
