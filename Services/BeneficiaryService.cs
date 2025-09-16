using BankingPaymentsAPI.DTOs;
using BankingPaymentsAPI.Models;
using BankingPaymentsAPI.Repository;

using System;
using System.Collections.Generic;
using System.Linq;

namespace BankingPaymentsAPI.Services
{
    public class BeneficiaryService : IBeneficiaryService
    {
        private readonly IBeneficiaryRepository _repo;
        public BeneficiaryService(IBeneficiaryRepository repo) => _repo = repo;

       
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
            b.Name = dto.Name;
            b.AccountNumberEncrypted = Encrypt(dto.AccountNumber);
            b.IFSC = dto.IFSC;
            b.BankName = dto.BankName;
            _repo.Update(b);
            return MapToDto(b);
        }

        public bool Delete(int id)
        {
            var b = _repo.GetById(id);
            if (b == null) return false;
            _repo.Delete(b);
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
    }
}
