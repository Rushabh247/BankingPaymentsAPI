using BankingPaymentsAPI.Models;


namespace BankingPaymentsAPI.Repository
{
    public interface IRefreshTokenRepository
    {
        RefreshToken Add(RefreshToken token);
        RefreshToken? GetByToken(string token);
        IEnumerable<RefreshToken> GetByUserId(int userId);
        void Update(RefreshToken token);
        void Delete(RefreshToken token);
    }
}
