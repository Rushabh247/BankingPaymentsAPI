using BankingPaymentsAPI.DTOs;
using BankingPaymentsAPI.Models;
using BankingPaymentsAPI.Repository;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace BankingPaymentsAPI.Services
{
    public class BeneficiaryService : IBeneficiaryService
    {
        private readonly IBeneficiaryRepository _repo;
        private readonly IAuditService _audit;
        private readonly IHttpContextAccessor _httpContext;

        public BeneficiaryService(IBeneficiaryRepository repo, IAuditService audit, IHttpContextAccessor httpContext)
        {
            _repo = repo;
            _audit = audit;
            _httpContext = httpContext;
        }

        private string Mask(string accountNumber)
        {
            if (string.IsNullOrEmpty(accountNumber)) return "****";
            if (accountNumber.Length <= 4) return "****";
            return new string('*', accountNumber.Length - 4) + accountNumber[^4..];
        }

        public async Task<BeneficiaryDto> CreateBeneficiaryAsync(BeneficiaryRequestDto dto, int createdBy)
        {
            var entity = new Beneficiary
            {
                ClientId = dto.ClientId,
                Name = dto.Name,
                AccountNumber = dto.AccountNumber,
                IFSC = dto.IFSC,
                Email = dto.Email, // take from DTO
                Balance = 0m
            };

            await _repo.AddAsync(entity);

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

        public async Task<BeneficiaryDto?> GetByIdAsync(int id)
        {
            var b = await _repo.GetByIdAsync(id);
            return b == null ? null : MapToDto(b);
        }

        public async Task<IEnumerable<BeneficiaryDto>> GetByClientAsync(int clientId)
        {
            var list = await _repo.GetByClientIdAsync(clientId);
            return list.Select(MapToDto);
        }

        public async Task<BeneficiaryDto?> UpdateAsync(int id, BeneficiaryRequestDto dto)
        {
            var b = await _repo.GetByIdAsync(id);
            if (b == null) return null;

            var oldValue = JsonSerializer.Serialize(b);

            b.Name = dto.Name;
            b.AccountNumber = dto.AccountNumber;
            b.IFSC = dto.IFSC;
            b.Email = dto.Email; // take from DTO

            await _repo.UpdateAsync(b);

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

        public async Task<bool> DeleteAsync(int id)
        {
            var b = await _repo.GetByIdAsync(id);
            if (b == null) return false;

            var oldValue = JsonSerializer.Serialize(b);
            await _repo.DeleteAsync(b);

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
                AccountNumberMasked = Mask(b.AccountNumber),
                IFSC = b.IFSC,
                Balance = b.Balance
            };

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
