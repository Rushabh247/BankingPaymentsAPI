using BankingPaymentsAPI.DTOs;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BankingPaymentsAPI.Services
{
    public interface IDocumentService
    {
        Task<DocumentUploadResultDto> UploadDocumentAsync(int clientId, IFormFile file, string documentType, int uploadedBy);
        Task<DocumentDto?> GetByIdAsync(int id);
        Task<IEnumerable<DocumentDto>> GetByClientAsync(int clientId);
     
        Task<bool> DeleteAsync(int id);
        Task<DocumentDto?> UpdateStatusAsync(int documentId, string status);
    }
}
