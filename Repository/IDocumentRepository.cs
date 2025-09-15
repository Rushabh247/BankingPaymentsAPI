using BankingPaymentsAPI.Models;
using System.Collections.Generic;

namespace BankingPaymentsAPI.Repository
{
    public interface IDocumentRepository
    {
        Document Add(Document document);
        Document? GetById(int id);
        IEnumerable<Document> GetByClientId(int clientId);
        void Delete(Document document);
    }
}
