using BankingPaymentsAPI.Models;

namespace BankingPaymentsAPI.Repository
{
    public interface IClientRepository
    {
        Client Add(Client client);
        Client? GetById(int id);
        IEnumerable<Client> GetAll();
        void Update(Client client);
        void Delete(Client client);
    }
}
