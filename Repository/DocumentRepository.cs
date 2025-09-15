using BankingPaymentsAPI.Models;
using BankingPaymentsAPI.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace BankingPaymentsAPI.Repository
{
    public class DocumentRepository : IDocumentRepository
    {
        private readonly AppDbContext _context;
        public DocumentRepository(AppDbContext context) => _context = context;

        public Document Add(Document document)
        {
            _context.Documents.Add(document);
            _context.SaveChanges();
            return document;
        }

        public Document? GetById(int id)
        {
            return _context.Documents
                .Include(d => d.Client)
                .FirstOrDefault(d => d.Id == id);
        }

        public IEnumerable<Document> GetByClientId(int clientId)
        {
            return _context.Documents.Where(d => d.ClientId == clientId).ToList();
        }

        public void Delete(Document document)
        {
            _context.Documents.Remove(document);
            _context.SaveChanges();
        }
    }
}
