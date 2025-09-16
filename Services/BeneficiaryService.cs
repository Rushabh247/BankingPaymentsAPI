using BankingPaymentsAPI.DTOs;
using BankingPaymentsAPI.Models;
using BankingPaymentsAPI.Repository;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace BankingPaymentsAPI.Services
{
    public class BeneficiaryService : IBeneficiaryService
    {
        private readonly IBeneficiaryRepository _repo;
        private readonly IAuditService _audit;
        private readonly IHttpContextAccessor _httpContext;

        public BeneficiaryService(
            IBeneficiaryRepository repo,
            IAuditService audit,
            IHttpContextAccessor httpContext)
        {
            _repo = repo;
            _audit = audit;
            _httpContext = httpContext;
        }

        //  Encryption helpers
        private string Encrypt(string plain) =>
            Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(plain));

        private string Mask(string encrypted)
        {
            try
            {
                var bytes = Convert.FromBase64String(encrypted);
                var plain = System.Text.Encoding.UTF8.GetString(bytes);
                if (plain.Length <= 4) return "****";
                return new string('*', plain.Length - 4) + plain[^4..];
            }
            catch { return "****"; }
        }

        public BeneficiaryDto CreateBeneficiary(BeneficiaryRequestDto dto, int createdBy)
        {
            var entity = new Beneficiary
            {
                ClientId = dto.ClientId,
                Name = dto.Name,
                AccountNumberEncrypted = Encrypt(dto.AccountNumber),
                IFSC = dto.IFSC,
                BankName = dto.BankName,
                IsActive = true
            };

            _repo.Add(entity);

            //  Log CREATE
            _audit.Log(new CreateAuditLogDto
            {
                UserId = GetCurrentUserId(),
                Action = "CREATE",
                EntityName = nameof(Beneficiary),
                EntityId = entity.Id,
                OldValueJson = null,
                NewValueJson = JsonSerializer.Serialize(entity),
                IpAddress = GetClientIp()
            });

            return MapToDto(entity);
        }

        public BeneficiaryDto? GetById(int id)
        {
            var b = _repo.GetById(id);
            return b == null ? null : MapToDto(b);
        }

        public IEnumerable<BeneficiaryDto> GetByClient(int clientId)
        {
            return _repo.GetByClientId(clientId).Select(MapToDto);
        }

        public BeneficiaryDto? Update(int id, BeneficiaryRequestDto dto)
        {
            var b = _repo.GetById(id);
            if (b == null) return null;

            var oldValue = JsonSerializer.Serialize(b);

            b.Name = dto.Name;
            b.AccountNumberEncrypted = Encrypt(dto.AccountNumber);
            b.IFSC = dto.IFSC;
            b.BankName = dto.BankName;

            _repo.Update(b);

            //  Log UPDATE
            _audit.Log(new CreateAuditLogDto
            {
                UserId = GetCurrentUserId(),
                Action = "UPDATE",
                EntityName = nameof(Beneficiary),
                EntityId = b.Id,
                OldValueJson = oldValue,
                NewValueJson = JsonSerializer.Serialize(b),
                IpAddress = GetClientIp()
            });

            return MapToDto(b);
        }

        public bool Delete(int id)
        {
            var b = _repo.GetById(id);
            if (b == null) return false;

            var oldValue = JsonSerializer.Serialize(b);

            _repo.Delete(b);

            //  Log DELETE
            _audit.Log(new CreateAuditLogDto
            {
                UserId = GetCurrentUserId(),
                Action = "DELETE",
                EntityName = nameof(Beneficiary),
                EntityId = b.Id,
                OldValueJson = oldValue,
                NewValueJson = null,
                IpAddress = GetClientIp()
            });

            return true;
        }

        private BeneficiaryDto MapToDto(Beneficiary b) =>
            new BeneficiaryDto
            {
                Id = b.Id,
                ClientId = b.ClientId,
                Name = b.Name,
                AccountNumberMasked = Mask(b.AccountNumberEncrypted),
                IFSC = b.IFSC,
                BankName = b.BankName,
                IsActive = b.IsActive
            };

        // Helpers
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
