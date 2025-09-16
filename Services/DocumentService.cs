using BankingPaymentsAPI.DTOs;
using BankingPaymentsAPI.Services.Storage;
using BankingPaymentsAPI.Enums;
using BankingPaymentsAPI.Repository;
using BankingPaymentsAPI.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        // Upload new document
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

            await _repo.AddAsync(doc);

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

        //  Get by Id
        public async Task<DocumentDto?> GetByIdAsync(int id)
        {
            var d = await _repo.GetByIdAsync(id);
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

        // Get all by Client
        public async Task<IEnumerable<DocumentDto>> GetByClientAsync(int clientId)
        {
            var docs = await _repo.GetByClientIdAsync(clientId);

            return docs.Select(d => new DocumentDto
            {
                Id = d.Id,
                ClientId = d.ClientId,
                FileName = d.FileName,
                Url = d.Url,
                Type = d.Type.ToString(),
                UploadedAt = d.UploadedAt
            });
        }

      
        //  Delete document (Cloudinary + DB)
        public async Task<bool> DeleteAsync(int id)
        {
            var d = await _repo.GetByIdAsync(id);
            if (d == null) return false;

            // Delete from Cloudinary
            await _fileStorage.DeleteFileAsync(d.CloudinaryPublicId);

            await _repo.DeleteAsync(d);
            return true;
        }
    }
}
