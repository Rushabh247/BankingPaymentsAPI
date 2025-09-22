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

        SalaryPayment? GetPaymentByStripeId(string stripeIntentId);
        SalaryPayment? GetPaymentById(int id);
        void UpdatePayment(SalaryPayment payment);

        // Add this to save new payments individually
        SalaryPayment AddPayment(SalaryPayment payment);
    }
}
