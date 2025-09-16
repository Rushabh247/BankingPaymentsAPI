using BankingPaymentsAPI.DTOs;
using BankingPaymentsAPI.Models;
using BankingPaymentsAPI.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace BankingPaymentsAPI.Services
{
    public class BankService : IBankService
    {
        private readonly IBankRepository _repo;
        private readonly IAuditService _audit;
        private readonly IHttpContextAccessor _httpContext;

        public BankService(IBankRepository repo, IAuditService audit, IHttpContextAccessor httpContext)
        {
            _repo = repo;
            _audit = audit;
            _httpContext = httpContext;
        }

        public BankDto CreateBank(BankRequestDto dto)
        {
            var bank = new Bank
            {
                BankCode = dto.BankCode,
                BankName = dto.BankName,
                Address = dto.Address,
                ContactNumber = dto.ContactNumber,
                IsActive = true
            };

            _repo.Add(bank);

            //  Log CREATE
            _audit.Log(new CreateAuditLogDto
            {
                UserId = GetUserId(),
                Action = "CREATE",
                EntityName = "Bank",
                EntityId = bank.Id,
                OldValueJson = null,
                NewValueJson = JsonSerializer.Serialize(bank),
                IpAddress = GetIp()
            });

            return MapToDto(bank);
        }

        public BankDto? GetBankById(int id)
        {
            var b = _repo.GetById(id);
            return b == null ? null : MapToDto(b);
        }

        public IEnumerable<BankDto> GetAllBanks()
        {
            return _repo.GetAll().Select(MapToDto);
        }

        public BankDto? UpdateBank(int id, BankUpdateDto dto)
        {
            var bank = _repo.GetById(id);
            if (bank == null) return null;

            var oldValue = JsonSerializer.Serialize(bank);

            bank.BankName = dto.BankName;
            bank.Address = dto.Address;
            bank.ContactNumber = dto.ContactNumber;
            bank.IsActive = dto.IsActive;

            _repo.Update(bank);

            //  Log UPDATE
            _audit.Log(new CreateAuditLogDto
            {
                UserId = GetUserId(),
                Action = "UPDATE",
                EntityName = "Bank",
                EntityId = bank.Id,
                OldValueJson = oldValue,
                NewValueJson = JsonSerializer.Serialize(bank),
                IpAddress = GetIp()
            });

            return MapToDto(bank);
        }

        public bool DeleteBank(int id)
        {
            var bank = _repo.GetById(id);
            if (bank == null) return false;

            var oldValue = JsonSerializer.Serialize(bank);

            _repo.Delete(bank);

            //  Log DELETE
            _audit.Log(new CreateAuditLogDto
            {
                UserId = GetUserId(),
                Action = "DELETE",
                EntityName = "Bank",
                EntityId = bank.Id,
                OldValueJson = oldValue,
                NewValueJson = null,
                IpAddress = GetIp()
            });

            return true;
        }

        private BankDto MapToDto(Bank bank) =>
            new BankDto
            {
                Id = bank.Id,
                BankCode = bank.BankCode,
                BankName = bank.BankName,
                Address = bank.Address,
                ContactNumber = bank.ContactNumber,
                IsActive = bank.IsActive
            };

        //  Helpers to get user and IP
        private int GetUserId()
        {
            var userIdClaim = _httpContext.HttpContext?.User?.FindFirst("userId")?.Value;
            return int.TryParse(userIdClaim, out var id) ? id : 0;
        }

        private string GetIp()
        {
            return _httpContext.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "unknown";
        }
    }
}
