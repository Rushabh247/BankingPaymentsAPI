using BankingPaymentsAPI.Models;

using System.Collections.Generic;

namespace BankingPaymentsAPI.Repository
{
    public interface IBeneficiaryRepository
    {
        Beneficiary Add(Beneficiary beneficiary);
        Beneficiary? GetById(int id);
        IEnumerable<Beneficiary> GetByClientId(int clientId);
        IEnumerable<Beneficiary> GetAll();
        void Update(Beneficiary beneficiary);
        void Delete(Beneficiary beneficiary);
    }
}
