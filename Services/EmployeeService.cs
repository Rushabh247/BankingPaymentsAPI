using BankingPaymentsAPI.DTOs;
using BankingPaymentsAPI.Models;
using BankingPaymentsAPI.Repository;

using System;
using System.Collections.Generic;
using System.Linq;

namespace BankingPaymentsAPI.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _repo;
        public EmployeeService(IEmployeeRepository repo) => _repo = repo;

        private string Encrypt(string plain) => Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(plain));
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
                IsActive = true
            };
            _repo.Add(e);
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
            e.EmployeeCode = dto.EmployeeCode;
            e.FullName = dto.FullName;
            e.AccountNumberEncrypted = Encrypt(dto.AccountNumber);
            e.Salary = dto.Salary;
            e.PAN = dto.PAN;
            _repo.Update(e);
            return MapToDto(e);
        }

        public bool Delete(int id)
        {
            var e = _repo.GetById(id);
            if (e == null) return false;
            _repo.Delete(e);
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
                IsActive = e.IsActive
            };
    }
}
