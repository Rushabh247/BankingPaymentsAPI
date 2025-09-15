using BankingPaymentsAPI.DTOs;
using System.Collections.Generic;

namespace BankingPaymentsAPI.Services
{
    public interface IBeneficiaryService
    {
        BeneficiaryDto CreateBeneficiary(BeneficiaryRequestDto dto, int createdBy);
        BeneficiaryDto? GetById(int id);
        IEnumerable<BeneficiaryDto> GetByClient(int clientId);
        BeneficiaryDto? Update(int id, BeneficiaryRequestDto dto);
        bool Delete(int id);
    }
}
