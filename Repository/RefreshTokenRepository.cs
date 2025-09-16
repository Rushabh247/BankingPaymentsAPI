using BankingPaymentsAPI.Data;
using BankingPaymentsAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace BankingPaymentsAPI.Repository
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly AppDbContext _context;
        public RefreshTokenRepository(AppDbContext context) => _context = context;

        public RefreshToken Add(RefreshToken token)
        {
            _context.RefreshTokens.Add(token);
            _context.SaveChanges();
            return token;
        }

        public RefreshToken? GetByToken(string token) =>
            _context.RefreshTokens
            .AsNoTracking()
            .FirstOrDefault(t => t.Token == token);

        public IEnumerable<RefreshToken> GetByUserId(int userId) =>
            _context.RefreshTokens.Where(t => t.UserId == userId).ToList();

        public void Update(RefreshToken token)
        {
            _context.RefreshTokens.Update(token);
            _context.SaveChanges();
        }

        public void Delete(RefreshToken token)
        {
            _context.RefreshTokens.Remove(token);
            _context.SaveChanges();
        }
    }
}
