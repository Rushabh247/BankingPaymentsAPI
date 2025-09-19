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

        public EmployeeService(
            IEmployeeRepository repo,
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

        public EmployeeDto CreateEmployee(EmployeeRequestDto dto, int createdBy)
        {
            var e = new Employee
            {
                ClientId = dto.ClientId,
                EmployeeCode = dto.EmployeeCode,
                FullName = dto.FullName,
                AccountNumberEncrypted = Encrypt(dto.AccountNumber),
                Salary = dto.Salary,
                PAN = dto.PAN,
                Email = dto.Email,
                IsActive = true
            };

            _repo.Add(e);

            //  Log CREATE
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

            e.EmployeeCode = dto.EmployeeCode;
            e.FullName = dto.FullName;
            e.AccountNumberEncrypted = Encrypt(dto.AccountNumber);
            e.Salary = dto.Salary;
            e.Email = dto.Email;
            e.PAN = dto.PAN;

            _repo.Update(e);

            //  Log UPDATE
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

            //  Log DELETE
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
                EmployeeCode = e.EmployeeCode,
                FullName = e.FullName,
                AccountNumberMasked = Mask(e.AccountNumberEncrypted),
                Salary = e.Salary,
                PAN = e.PAN,
                Email = e.Email,
                IsActive = e.IsActive
            };

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
