using BankingPaymentsAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BankingPaymentsAPI.Repository
{
    public interface IClientRepository
    {
        Task<Client> AddAsync(Client client);
        Task<Client?> GetByIdAsync(int id);
        Task<IEnumerable<Client>> GetAllAsync();
        Task UpdateAsync(Client client);
        Task DeleteAsync(Client client);
        Task<Client?> GetByStripePaymentIntentIdAsync(string paymentIntentId);
        Task<Client?> GetByStripeIdAsync(string paymentIntentId);
    }
}
