using BankingPaymentsAPI.Models;

namespace BankingPaymentsAPI.Repository
{
    public interface IUserRepository
    {
        User? GetById(int id);
        User? GetByUsernameOrEmail(string usernameOrEmail);
        User Add(User user);
        void Update(User user);
        void SoftDelete(User user);
    }
}
