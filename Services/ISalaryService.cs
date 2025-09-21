using BankingPaymentsAPI.DTOs;
using System.Collections.Generic;

namespace BankingPaymentsAPI.Services
{
    public interface ISalaryService
    {
        SalaryBatchDto CreateBatch(SalaryBatchRequestDto request, int createdBy);
        SalaryBatchDto? GetBatchById(int id);
        IEnumerable<SalaryBatchDto> GetBatchesByClient(int clientId);
        SalaryBatchDto? SubmitBatch(int id, int submittedBy);
        bool DeleteBatch(int id);

        SalaryPaymentDto? ProcessPayment(int paymentId);

        
        SalaryPaymentDto? ProcessStripePayment(int paymentId);
        SalaryPaymentDto? ConfirmStripeSalaryPayment(string paymentIntentId, int approverId);
    }
}
