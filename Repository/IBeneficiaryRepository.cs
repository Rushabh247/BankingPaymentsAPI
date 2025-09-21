using BankingPaymentsAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BankingPaymentsAPI.Repository
{
    public interface IBeneficiaryRepository
    {
        Task<Beneficiary> AddAsync(Beneficiary beneficiary);
        Task<Beneficiary?> GetByIdAsync(int id);
        Task<IEnumerable<Beneficiary>> GetByClientIdAsync(int clientId);
        Task<IEnumerable<Beneficiary>> GetAllAsync();
        Task UpdateAsync(Beneficiary beneficiary);
        Task DeleteAsync(Beneficiary beneficiary);
    }
}
