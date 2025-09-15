using BankingPaymentsAPI.Data;
using BankingPaymentsAPI.Models;

using Microsoft.EntityFrameworkCore;

namespace BankingPaymentsAPI.Repository
{
    public class ClientRepository : IClientRepository
    {
        private readonly AppDbContext _context;

        public ClientRepository(AppDbContext context)
        {
            _context = context;
        }

        public Client Add(Client client)
        {
            _context.Clients.Add(client);
            _context.SaveChanges();
            return client;
        }

        public Client? GetById(int id)
        {
            return _context.Clients
                .Include(c => c.Bank)
                .Include(c => c.Beneficiaries)
                .Include(c => c.Employees)
                .Include(c => c.Documents)
                .FirstOrDefault(c => c.Id == id);
        }

        public IEnumerable<Client> GetAll()
        {
            return _context.Clients
                .Include(c => c.Bank)
                .ToList();
        }

        public void Update(Client client)
        {
            _context.Clients.Update(client);
            _context.SaveChanges();
        }

        public void Delete(Client client)
        {
            _context.Clients.Remove(client);
            _context.SaveChanges();
        }
    }
}
