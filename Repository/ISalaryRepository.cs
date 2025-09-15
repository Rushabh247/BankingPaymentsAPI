using BankingPaymentsAPI.Models;

using System.Collections.Generic;

namespace BankingPaymentsAPI.Repository
{
    public interface ISalaryRepository
    {
        SalaryBatch AddBatch(SalaryBatch batch);
        SalaryBatch? GetBatchById(int id);
        IEnumerable<SalaryBatch> GetBatchesByClient(int clientId);
        void UpdateBatch(SalaryBatch batch);
        void DeleteBatch(SalaryBatch batch);
    }
}
