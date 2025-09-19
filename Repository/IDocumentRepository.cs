using BankingPaymentsAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BankingPaymentsAPI.Repository
{
    public interface IDocumentRepository
    {
        Task<Document> AddAsync(Document document);
        Task<Document?> GetByIdAsync(int id);
        Task<IEnumerable<Document>> GetByClientIdAsync(int clientId);
        Task DeleteAsync(Document document);
        Task UpdateAsync(Document document);
    }
}
