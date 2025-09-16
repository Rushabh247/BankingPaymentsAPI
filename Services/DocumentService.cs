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
using System.Text.Json;

namespace BankingPaymentsAPI.Services
{
    public class DocumentService : IDocumentService
    {
        private readonly IDocumentRepository _repo;
        private readonly IFileStorageService _fileStorage;
        private readonly IAuditService _audit;
        private readonly IHttpContextAccessor _httpContext;

        public DocumentService(
            IDocumentRepository repo,
            IFileStorageService fileStorage,
            IAuditService audit,
            IHttpContextAccessor httpContext)
        {
            _repo = repo;
            _fileStorage = fileStorage;
            _audit = audit;
            _httpContext = httpContext;
        }

        // Upload new document
        public async Task<DocumentUploadResultDto> UploadDocumentAsync(int clientId, IFormFile file, string documentType, int uploadedBy)
        {
            using var stream = file.OpenReadStream();

            // Upload file to Cloudinary 
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

            // Log CREATE
            _audit.Log(new CreateAuditLogDto
            {
                UserId = GetCurrentUserId(),
                Action = "CREATE",
                EntityName = nameof(Document),
                EntityId = doc.Id,
                OldValueJson = null,
                NewValueJson = JsonSerializer.Serialize(doc),
                IpAddress = GetClientIp()
            });

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

        // Get by Id
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

        // Delete document (Cloudinary + DB)
        public async Task<bool> DeleteAsync(int id)
        {
            var d = await _repo.GetByIdAsync(id);
            if (d == null) return false;

            var oldValue = JsonSerializer.Serialize(d);

            // Delete from Cloudinary
            await _fileStorage.DeleteFileAsync(d.CloudinaryPublicId);

            await _repo.DeleteAsync(d);

            // Log DELETE
            _audit.Log(new CreateAuditLogDto
            {
                UserId = GetCurrentUserId(),
                Action = "DELETE",
                EntityName = nameof(Document),
                EntityId = d.Id,
                OldValueJson = oldValue,
                NewValueJson = null,
                IpAddress = GetClientIp()
            });

            return true;
        }

        //  Helpers
        private int GetCurrentUserId()
        {
            var userIdClaim = _httpContext.HttpContext?.User?.FindFirst("userId")?.Value;
            return int.TryParse(userIdClaim, out var id) ? id : 0;
        }

        private string GetClientIp()
        {
            return _httpContext.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "unknown";
        }
    }
}
