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
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _repo;
        private readonly IAuditService _audit;
        private readonly IHttpContextAccessor _httpContext;

        public EmployeeService(IEmployeeRepository repo, IAuditService audit, IHttpContextAccessor httpContext)
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

        public EmployeeDto CreateEmployee(EmployeeRequestDto dto, int createdBy)
        {
            var e = new Employee
            {
                ClientId = dto.ClientId,
                FullName = dto.FullName,
                AccountNumber = dto.AccountNumber,
                Email = dto.Email,
                Balance = 0m
            };

            _repo.Add(e);

            _audit.Log(new CreateAuditLogDto
            {
                UserId = GetCurrentUserId(),
                Action = "CREATE",
                EntityName = nameof(Employee),
                EntityId = e.Id,
                OldValueJson = null,
                NewValueJson = JsonSerializer.Serialize(e),
                IpAddress = GetClientIp()
            });

            return MapToDto(e);
        }

        public EmployeeDto? GetById(int id)
        {
            var e = _repo.GetById(id);
            return e == null ? null : MapToDto(e);
        }

        public IEnumerable<EmployeeDto> GetByClient(int clientId)
        {
            return _repo.GetByClientId(clientId).Select(MapToDto);
        }

        public EmployeeDto? Update(int id, EmployeeRequestDto dto)
        {
            var e = _repo.GetById(id);
            if (e == null) return null;

            var oldValue = JsonSerializer.Serialize(e);

            e.FullName = dto.FullName;
            e.AccountNumber = dto.AccountNumber;
            e.Email = dto.Email;

            _repo.Update(e);

            _audit.Log(new CreateAuditLogDto
            {
                UserId = GetCurrentUserId(),
                Action = "UPDATE",
                EntityName = nameof(Employee),
                EntityId = e.Id,
                OldValueJson = oldValue,
                NewValueJson = JsonSerializer.Serialize(e),
                IpAddress = GetClientIp()
            });

            return MapToDto(e);
        }

        public bool Delete(int id)
        {
            var e = _repo.GetById(id);
            if (e == null) return false;

            var oldValue = JsonSerializer.Serialize(e);
            _repo.Delete(e);

            _audit.Log(new CreateAuditLogDto
            {
                UserId = GetCurrentUserId(),
                Action = "DELETE",
                EntityName = nameof(Employee),
                EntityId = e.Id,
                OldValueJson = oldValue,
                NewValueJson = null,
                IpAddress = GetClientIp()
            });

            return true;
        }

        private EmployeeDto MapToDto(Employee e) =>
            new EmployeeDto
            {
                Id = e.Id,
                ClientId = e.ClientId,
                FullName = e.FullName,
                AccountNumberMasked = Mask(e.AccountNumber),
                Email = e.Email,
                Balance = e.Balance
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
