using BankingPaymentsAPI.DTOs;
using System.Collections.Generic;

namespace BankingPaymentsAPI.Services
{
    public interface IBankService
    {
        BankDto CreateBank(BankRequestDto dto);
        BankDto? GetBankById(int id);
        IEnumerable<BankDto> GetAllBanks();
        BankDto? UpdateBank(int id, BankUpdateDto dto);
        bool DeleteBank(int id);
    }
}
