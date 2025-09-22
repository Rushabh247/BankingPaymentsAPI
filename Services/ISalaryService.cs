using BankingPaymentsAPI.DTOs;
using System.Collections.Generic;

namespace BankingPaymentsAPI.Services
{
    public interface ISalaryService
    {
        // Batch
        SalaryBatchDto CreateBatch(SalaryBatchRequestDto request, int createdByUserId);
        SalaryBatchDto? ProcessBatch(int batchId, int approverId, string remarks);

        // Single retry
        SalaryPaymentDto? RetryFailedPayment(int paymentId, int approverId);

        // Stripe
        SalaryPaymentDto CreateStripePayment(int employeeId, decimal amount, int batchId, int createdByUserId);
        SalaryPaymentDto? ConfirmStripeSalaryPayment(string paymentIntentId, int approverId);

        // Queries
        SalaryBatchDto? GetBatchById(int id);
        IEnumerable<SalaryBatchDto> GetBatchesByClient(int clientId);
        SalaryPaymentDto? GetPaymentById(int id);

        // Delete
        bool DeleteBatch(int id);
    }
}
