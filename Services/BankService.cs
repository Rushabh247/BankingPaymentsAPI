using BankingPaymentsAPI.DTOs;
using BankingPaymentsAPI.Models;
using BankingPaymentsAPI.Repository;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BankingPaymentsAPI.Services
{
    public class BankService : IBankService
    {
        private readonly IBankRepository _repo;
        public BankService(IBankRepository repo) => _repo = repo;

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
            bank.BankName = dto.BankName;
            bank.Address = dto.Address;
            bank.ContactNumber = dto.ContactNumber;
            bank.IsActive = dto.IsActive;
            _repo.Update(bank);
            return MapToDto(bank);
        }

        public bool DeleteBank(int id)
        {
            var bank = _repo.GetById(id);
            if (bank == null) return false;
            _repo.Delete(bank);
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
    }
}
