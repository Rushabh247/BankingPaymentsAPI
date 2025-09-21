using BankingPaymentsAPI.DTOs;
using BankingPaymentsAPI.Enums;
using BankingPaymentsAPI.Models;
using BankingPaymentsAPI.Repository;
using BankingPaymentsAPI.Services.Notification;
using BankingPaymentsAPI.Services.Storage;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BankingPaymentsAPI.Services
{
    public class DocumentService : IDocumentService
    {
        private readonly IDocumentRepository _repo;
        private readonly IFileStorageService _fileStorage;
        private readonly IAuditService _audit;
        private readonly IHttpContextAccessor _httpContext;
        private readonly IEmailNotificationService _emailNotification;
        private readonly IClientRepository _clientRepo;

        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            ReferenceHandler = ReferenceHandler.Preserve,
            WriteIndented = true
        };

        public DocumentService(
            IDocumentRepository repo,
            IFileStorageService fileStorage,
            IAuditService audit,
            IHttpContextAccessor httpContext,
            IEmailNotificationService emailNotification,
            IClientRepository clientRepo)
        {
            _repo = repo;
            _fileStorage = fileStorage;
            _audit = audit;
            _httpContext = httpContext;
            _emailNotification = emailNotification;
            _clientRepo = clientRepo;
        }

        public async Task<DocumentUploadResultDto> UploadDocumentAsync(int clientId, IFormFile file, string documentType, int uploadedBy)
        {
            using var stream = file.OpenReadStream();

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
                UploadedBy = uploadedBy,
                Status = DocumentStatus.Pending
            };

            await _repo.AddAsync(doc);

            _audit.Log(new CreateAuditLogDto
            {
                UserId = GetCurrentUserId(),
                Action = "CREATE",
                EntityName = nameof(Document),
                EntityId = doc.Id,
                OldValueJson = null,
                NewValueJson = JsonSerializer.Serialize(doc, _jsonOptions),
                IpAddress = GetClientIp()
            });

            var client = await _clientRepo.GetByIdAsync(clientId);
            if (client != null && !string.IsNullOrEmpty(client.ContactEmail))
            {
                await _emailNotification.SendEmailAsync(
                    client.ContactEmail,
                    "Document Uploaded Successfully",
                    $"<p>Dear {client.Name},</p>" +
                    $"<p>Your document <strong>{doc.FileName}</strong> has been uploaded successfully and is pending verification.</p>" +
                    "<p>Thank you,<br/>BankingPayments Team</p>"
                );
            }

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
                UploadedBy = doc.UploadedBy,
                Status = doc.Status.ToString()
            };
        }

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
                UploadedAt = d.UploadedAt,
                Status = d.Status.ToString()
            };
        }

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
                UploadedAt = d.UploadedAt,
                Status = d.Status.ToString()
            });
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var d = await _repo.GetByIdAsync(id);
            if (d == null) return false;

            var oldValue = JsonSerializer.Serialize(d, _jsonOptions);

            await _fileStorage.DeleteFileAsync(d.CloudinaryPublicId);
            await _repo.DeleteAsync(d);

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

            var client = await _clientRepo.GetByIdAsync(d.ClientId);
            if (client != null && !string.IsNullOrEmpty(client.ContactEmail))
            {
                await _emailNotification.SendEmailAsync(
                    client.ContactEmail,
                    "Document Deleted",
                    $"<p>Dear {client.Name},</p>" +
                    $"<p>Your document <strong>{d.FileName}</strong> has been deleted from the system.</p>" +
                    "<p>Thank you,<br/>BankingPayments Team</p>"
                );
            }

            return true;
        }

        public async Task<DocumentDto?> UpdateStatusAsync(int documentId, string status)
        {
            var doc = await _repo.GetByIdAsync(documentId);
            if (doc == null) return null;

            var oldValue = JsonSerializer.Serialize(doc, _jsonOptions);

            if (!Enum.TryParse<DocumentStatus>(status, true, out var newStatus)) return null;

            doc.Status = newStatus;
            await _repo.UpdateAsync(doc);

            _audit.Log(new CreateAuditLogDto
            {
                UserId = GetCurrentUserId(),
                Action = "UPDATE_DOCUMENT_STATUS",
                EntityName = nameof(Document),
                EntityId = doc.Id,
                OldValueJson = oldValue,
                NewValueJson = JsonSerializer.Serialize(doc, _jsonOptions),
                IpAddress = GetClientIp()
            });

            var client = await _clientRepo.GetByIdAsync(doc.ClientId);
            if (client != null)
            {
                var oldClientValue = JsonSerializer.Serialize(client, _jsonOptions);

                if (doc.Status == DocumentStatus.Verified)
                {
                    client.IsVerified = true;
                    client.OnboardingStatus = OnboardingStatus.Approved;
                    client.VerifiedAt = DateTimeOffset.UtcNow;
                    client.VerifiedBy = GetCurrentUserId();

                    await _clientRepo.UpdateAsync(client);

                    _audit.Log(new CreateAuditLogDto
                    {
                        UserId = GetCurrentUserId(),
                        Action = "VERIFY_CLIENT",
                        EntityName = nameof(Client),
                        EntityId = client.Id,
                        OldValueJson = oldClientValue,
                        NewValueJson = JsonSerializer.Serialize(client, _jsonOptions),
                        IpAddress = GetClientIp()
                    });
                }
                else if (doc.Status == DocumentStatus.Rejected)
                {
                    client.IsVerified = false;
                    client.OnboardingStatus = OnboardingStatus.Rejected;
                    client.VerifiedAt = null;
                    client.VerifiedBy = null;

                    await _clientRepo.UpdateAsync(client);

                    _audit.Log(new CreateAuditLogDto
                    {
                        UserId = GetCurrentUserId(),
                        Action = "REJECT_CLIENT",
                        EntityName = nameof(Client),
                        EntityId = client.Id,
                        OldValueJson = oldClientValue,
                        NewValueJson = JsonSerializer.Serialize(client, _jsonOptions),
                        IpAddress = GetClientIp()
                    });
                }

                if (!string.IsNullOrEmpty(client.ContactEmail))
                {
                    string subject = $"Document Status Update - {doc.FileName}";
                    string body = $"<p>Dear {client.Name},</p>";

                    if (doc.Status == DocumentStatus.Verified)
                    {
                        body += $"<p>Your document <strong>{doc.FileName}</strong> has been <span style='color:green;font-weight:bold;'>VERIFIED</span>.</p>";
                        body += $"<p>Your account is now <b>APPROVED</b> and you can start using our services.</p>";
                    }
                    else if (doc.Status == DocumentStatus.Rejected)
                    {
                        body += $"<p>Your document <strong>{doc.FileName}</strong> has been <span style='color:red;font-weight:bold;'>REJECTED</span>.</p>";
                        body += $"<p>Your onboarding request is <b>REJECTED</b>. Please re-upload valid documents.</p>";
                    }

                    body += "<p>Thank you,<br/>BankingPayments Team</p>";

                    await _emailNotification.SendEmailAsync(client.ContactEmail, subject, body);
                }
            }

            return new DocumentDto
            {
                Id = doc.Id,
                ClientId = doc.ClientId,
                FileName = doc.FileName,
                Url = doc.Url,
                Type = doc.Type.ToString(),
                UploadedAt = doc.UploadedAt,
                Status = doc.Status.ToString()
            };
        }

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
