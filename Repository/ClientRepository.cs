using BankingPaymentsAPI.Data;
using BankingPaymentsAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BankingPaymentsAPI.Repository
{
    public class ClientRepository : IClientRepository
    {
        private readonly AppDbContext _context;

        public ClientRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Client> AddAsync(Client client)
        {
            await _context.Clients.AddAsync(client);
            await _context.SaveChangesAsync();
            return client;
        }

        public async Task<Client?> GetByIdAsync(int id)
        {
            return await _context.Clients
                .Include(c => c.Bank)
                .Include(c => c.Beneficiaries)
                .Include(c => c.Employees)
                .Include(c => c.Documents)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<IEnumerable<Client>> GetAllAsync()
        {
            return await _context.Clients
                .Include(c => c.Bank)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task UpdateAsync(Client client)
        {
            // Attach if not tracked
            var tracked = _context.Clients.Local.FirstOrDefault(c => c.Id == client.Id);
            if (tracked == null)
            {
                _context.Clients.Attach(client);
            }

            _context.Entry(client).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Client client)
        {
            _context.Clients.Remove(client);
            await _context.SaveChangesAsync();
        }

        public async Task<Client?> GetByStripePaymentIntentIdAsync(string paymentIntentId)
        {
            return await _context.Clients
                .Include(c => c.Bank)
                .Include(c => c.Beneficiaries)
                .Include(c => c.Employees)
                .FirstOrDefaultAsync(c => c.StripePaymentIntentId == paymentIntentId);
        }

        public async Task<Client?> GetByStripeIdAsync(string paymentIntentId)
        {
            return await _context.Clients.FirstOrDefaultAsync(c => c.StripePaymentIntentId == paymentIntentId);
        }
    }
}
