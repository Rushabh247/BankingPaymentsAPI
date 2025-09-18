using BankingPaymentsAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BankingPaymentsAPI.Repository
{
    public interface IRefreshTokenRepository
    {
        Task<RefreshToken> AddAsync(RefreshToken token);
        Task<RefreshToken?> GetByTokenAsync(string token);
        Task<List<RefreshToken>> GetByUserIdAsync(int userId);
        Task UpdateAsync(RefreshToken token);
        Task DeleteAsync(RefreshToken token);
    }
}
