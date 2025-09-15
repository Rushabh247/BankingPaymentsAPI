using BankingPaymentsAPI.Models;

using System.Collections.Generic;

namespace BankingPaymentsAPI.Repository
{
    public interface IBankRepository
    {
        Bank Add(Bank bank);
        Bank? GetById(int id);
        IEnumerable<Bank> GetAll();
        void Update(Bank bank);
        void Delete(Bank bank);
    }
}
