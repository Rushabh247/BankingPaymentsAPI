using BankingPaymentsAPI.DTOs;
using BankingPaymentsAPI.Enums;
using BankingPaymentsAPI.Models;
using BankingPaymentsAPI.Repository;

namespace BankingPaymentsAPI.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepo;

        public PaymentService(IPaymentRepository paymentRepo)
        {
            _paymentRepo = paymentRepo;
        }

        public PaymentDto CreatePayment(PaymentRequestDto request, int createdByUserId)
        {
            var payment = new Payment
            {
                ClientId = request.ClientId,
                BeneficiaryId = request.BeneficiaryId,
                Amount = request.Amount,
                Currency = request.Currency ?? "INR",
                Status = PaymentStatus.PendingApproval,
                CreatedBy = createdByUserId
            };

            _paymentRepo.Add(payment);
            return MapToDto(payment);
        }

        public PaymentDto? GetPaymentById(int id)
        {
            var payment = _paymentRepo.GetById(id);
            return payment == null ? null : MapToDto(payment);
        }

        public IEnumerable<PaymentDto> GetAllPayments()
        {
            return _paymentRepo.GetAll().Select(MapToDto);
        }

        public PaymentDto? ApprovePayment(int id, int approverId, string remarks)
        {
            var payment = _paymentRepo.GetById(id);
            if (payment == null) return null;

            payment.Status = PaymentStatus.Approved;
            payment.ApprovedBy = approverId;
            payment.ApprovedAt = DateTimeOffset.UtcNow;
            payment.ApprovalRemarks = remarks;

            _paymentRepo.Update(payment);
            return MapToDto(payment);
        }

        public bool DeletePayment(int id)
        {
            var payment = _paymentRepo.GetById(id);
            if (payment == null) return false;

            _paymentRepo.Delete(payment);
            return true;
        }

        private PaymentDto MapToDto(Payment payment)
        {
            return new PaymentDto
            {
                Id = payment.Id,
                ClientName = payment.Client?.Name ?? "Unknown",
                BeneficiaryName = payment.Beneficiary?.Name ?? "Unknown",
                Amount = payment.Amount,
                Status = payment.Status.ToString(),
                ApprovedBy = payment.ApprovedBy,
                ApprovedAt = payment.ApprovedAt
            };
        }
    }
}
