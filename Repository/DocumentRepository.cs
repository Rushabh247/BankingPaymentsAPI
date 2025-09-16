using BankingPaymentsAPI.Models;
using BankingPaymentsAPI.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BankingPaymentsAPI.Repository
{
    public class DocumentRepository : IDocumentRepository
    {
        private readonly AppDbContext _context;
        public DocumentRepository(AppDbContext context) => _context = context;

        public async Task<Document> AddAsync(Document document)
        {
            await _context.Documents.AddAsync(document);
            await _context.SaveChangesAsync();
            return document;
        }

        public async Task<Document?> GetByIdAsync(int id)
        {
            return await _context.Documents
                .AsNoTracking()
                .Include(d => d.Client)
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<IEnumerable<Document>> GetByClientIdAsync(int clientId)
        {
            return await _context.Documents
                .AsNoTracking()
                .Where(d => d.ClientId == clientId)
                .ToListAsync();
        }

        public async Task DeleteAsync(Document document)
        {
            _context.Documents.Remove(document);
            await _context.SaveChangesAsync();
        }
    }
}
