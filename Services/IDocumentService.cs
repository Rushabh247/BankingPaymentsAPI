using BankingPaymentsAPI.DTOs;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace BankingPaymentsAPI.Services
{
    public interface IDocumentService
    {
        Task<DocumentUploadResultDto> UploadDocumentAsync(int clientId, IFormFile file, string documentType, int uploadedBy);
        DocumentDto? GetById(int id);
        IEnumerable<DocumentDto> GetByClient(int clientId);
        Task<bool> DeleteAsync(int id);
    }

}
