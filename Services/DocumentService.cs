using BankingPaymentsAPI.DTOs;
using BankingPaymentsAPI.Services.Storage;
using BankingPaymentsAPI.Enums;
using BankingPaymentsAPI.Repository;
using BankingPaymentsAPI.Models;
using Microsoft.AspNetCore.Http;

namespace BankingPaymentsAPI.Services
{
    public class DocumentService : IDocumentService
    {
        private readonly IDocumentRepository _repo;
        private readonly IFileStorageService _fileStorage;

        public DocumentService(IDocumentRepository repo, IFileStorageService fileStorage)
        {
            _repo = repo;
            _fileStorage = fileStorage;
        }

      
        public async Task<DocumentUploadResultDto> UploadDocumentAsync(int clientId, IFormFile file, string documentType, int uploadedBy)
        {
            using var stream = file.OpenReadStream();

            // Upload file to Cloudinary (async)
            var storageResult = await _fileStorage.UploadFileAsync(
                stream,
                file.FileName,
                file.ContentType,
                $"clients/{clientId}"
            );

            var doc = new Document
            {
                ClientId = clientId,
                FileName = file.FileName,
                MimeType = file.ContentType,
                SizeBytes = file.Length,
                CloudinaryPublicId = storageResult.PublicId,
                Url = storageResult.Url,
                Type = Enum.TryParse<DocumentType>(documentType, true, out var dt) ? dt : DocumentType.Other,
                UploadedAt = DateTimeOffset.UtcNow,
                UploadedBy = uploadedBy
            };

            _repo.Add(doc);

            return new DocumentUploadResultDto
            {
                Id = doc.Id,
                ClientId = doc.ClientId,
                FileName = doc.FileName,
                MimeType = doc.MimeType,
                SizeBytes = doc.SizeBytes,
                Url = doc.Url,
                CloudinaryPublicId = doc.CloudinaryPublicId,
                Type = doc.Type.ToString(),
                UploadedAt = doc.UploadedAt,
                UploadedBy = doc.UploadedBy
            };
        }

        public DocumentDto? GetById(int id)
        {
            var d = _repo.GetById(id);
            if (d == null) return null;

            return new DocumentDto
            {
                Id = d.Id,
                ClientId = d.ClientId,
                FileName = d.FileName,
                Url = d.Url,
                Type = d.Type.ToString(),
                UploadedAt = d.UploadedAt
            };
        }

        public IEnumerable<DocumentDto> GetByClient(int clientId)
        {
            return _repo.GetByClientId(clientId).Select(d => new DocumentDto
            {
                Id = d.Id,
                ClientId = d.ClientId,
                FileName = d.FileName,
                Url = d.Url,
                Type = d.Type.ToString(),
                UploadedAt = d.UploadedAt
            });
        }

       
        public async Task<bool> DeleteAsync(int id)
        {
            var d = _repo.GetById(id);
            if (d == null) return false;

            // Delete from Cloudinary
            await _fileStorage.DeleteFileAsync(d.CloudinaryPublicId);

            _repo.Delete(d);
            return true;
        }
    }
}
