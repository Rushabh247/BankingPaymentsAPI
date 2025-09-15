using BankingPaymentsAPI.DTOs;
using System.Collections.Generic;

namespace BankingPaymentsAPI.Services
{
    public interface ISalaryService
    {
        SalaryBatchDto CreateBatch(SalaryBatchRequestDto request, int createdBy);
        SalaryBatchDto? GetBatchById(int id);
        IEnumerable<SalaryBatchDto> GetBatchesByClient(int clientId);
        SalaryBatchDto? SubmitBatch(int id, int submittedBy); // sets status Submitted
        bool DeleteBatch(int id);
    }
}
