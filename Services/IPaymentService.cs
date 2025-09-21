using BankingPaymentsAPI.DTOs;
using System.Collections.Generic;

namespace BankingPaymentsAPI.Services
{
    public interface IPaymentService
    {
        PaymentDto CreatePayment(PaymentRequestDto request, int createdByUserId);

        PaymentDto? GetPaymentById(int id);
        IEnumerable<PaymentDto> GetAllPayments();

        PaymentDto? ApprovePayment(int id, int approverId, string remarks);

        bool DeletePayment(int id);

        
        PaymentDto CreateStripePayment(PaymentRequestDto request, int createdByUserId);
        PaymentDto ConfirmStripePayment(string paymentIntentId, int approverId, string remarks);
    }
}
