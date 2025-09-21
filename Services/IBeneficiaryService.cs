using BankingPaymentsAPI.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BankingPaymentsAPI.Services
{
    public interface IBeneficiaryService
    {
        Task<BeneficiaryDto> CreateBeneficiaryAsync(BeneficiaryRequestDto dto, int createdBy);
        Task<BeneficiaryDto?> GetByIdAsync(int id);
        Task<IEnumerable<BeneficiaryDto>> GetByClientAsync(int clientId);
        Task<BeneficiaryDto?> UpdateAsync(int id, BeneficiaryRequestDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
