using BankingPaymentsAPI.DTOs;
using BankingPaymentsAPI.Enums;
using BankingPaymentsAPI.Models;
using BankingPaymentsAPI.Repository;

namespace BankingPaymentsAPI.Services
{
    public class SalaryService : ISalaryService
    {
        private readonly ISalaryRepository _salaryRepo;
        private readonly IPaymentService _paymentService;

        public SalaryService(ISalaryRepository salaryRepo, IPaymentService paymentService)
        {
            _salaryRepo = salaryRepo;
            _paymentService = paymentService;
        }

        public SalaryBatchDto CreateBatch(SalaryBatchRequestDto request, int createdBy)
        {
            var batch = new SalaryBatch
            {
                ClientId = request.ClientId,
                BatchCode = request.BatchCode,
                TotalAmount = request.Items.Sum(i => i.Amount),
                Status = BatchStatus.Created,
                Items = request.Items.Select(i => new SalaryPayment
                {
                    EmployeeId = i.EmployeeId,
                    Amount = i.Amount,
                    Status = PaymentStatus.Draft,
                    Method = request.Method
                }).ToList()
            };

            _salaryRepo.AddBatch(batch);

            return MapBatchToDto(batch);
        }

        public SalaryBatchDto? GetBatchById(int id)
        {
            var batch = _salaryRepo.GetBatchById(id);
            if (batch == null) return null;

            return MapBatchToDto(batch);
        }

        public IEnumerable<SalaryBatchDto> GetBatchesByClient(int clientId)
        {
            var batches = _salaryRepo.GetBatchesByClient(clientId);
            return batches.Select(MapBatchToDto);
        }

        public SalaryBatchDto? SubmitBatch(int id, int submittedBy)
        {
            var batch = _salaryRepo.GetBatchById(id);
            if (batch == null) return null;

            batch.Status = BatchStatus.Submitted;
            foreach (var item in batch.Items)
            {
                item.Status = PaymentStatus.PendingApproval;
            }

            _salaryRepo.UpdateBatch(batch);
            return MapBatchToDto(batch);
        }

        public bool DeleteBatch(int id)
        {
            var batch = _salaryRepo.GetBatchById(id);
            if (batch == null) return false;

            _salaryRepo.DeleteBatch(batch);
            return true;
        }

        public SalaryPaymentDto? ProcessPayment(int paymentId)
        {
            var payment = _salaryRepo.GetPaymentById(paymentId);
            if (payment == null) return null;

            // Example: internal transfer
            payment.Status = PaymentStatus.Processed;
            _salaryRepo.UpdatePayment(payment);

            return MapPaymentToDto(payment);
        }

        public SalaryPaymentDto? ProcessStripePayment(int paymentId)
        {
            var payment = _salaryRepo.GetPaymentById(paymentId);
            if (payment == null) return null;

            // create Stripe payment intent here
            payment.Method = PaymentMethod.Stripe;
            payment.Status = PaymentStatus.PendingApproval;

            _salaryRepo.UpdatePayment(payment);
            return MapPaymentToDto(payment);
        }

        public SalaryPaymentDto? ConfirmStripeSalaryPayment(string paymentIntentId, int approverId)
        {
            // Find the payment by Stripe intent
            var payment = _salaryRepo.GetBatchesByClient(0) // replace with actual method to get payment by Stripe ID
                .SelectMany(b => b.Items)
                .FirstOrDefault(p => p.StripePaymentIntentId == paymentIntentId);

            if (payment == null) return null;

            payment.Status = PaymentStatus.Processed;
            _salaryRepo.UpdatePayment(payment);
            return MapPaymentToDto(payment);
        }

        // ---------------- Helpers ----------------

        private SalaryBatchDto MapBatchToDto(SalaryBatch batch)
        {
            return new SalaryBatchDto
            {
                Id = batch.Id,
                ClientId = batch.ClientId,
                BatchCode = batch.BatchCode,
                TotalAmount = batch.TotalAmount,
                Status = ConvertBatchStatusToPaymentStatus(batch.Status),
                Items = batch.Items.Select(MapPaymentToDto).ToList(),
                Method = batch.Items.FirstOrDefault()?.Method ?? PaymentMethod.Internal
            };
        }

        private SalaryPaymentDto MapPaymentToDto(SalaryPayment payment)
        {
            return new SalaryPaymentDto
            {
                Id = payment.Id,
                EmployeeId = payment.EmployeeId,
                EmployeeName = payment.Employee?.FullName ?? "",
                Amount = payment.Amount,
                Status = payment.Status,
                TxnRef = payment.TxnRef,
                FailureReason = payment.FailureReason,
                SalaryBatchId = null, // optional, if needed add reference
                Method = payment.Method,
                StripePaymentIntentId = payment.StripePaymentIntentId
            };
        }

        private PaymentStatus ConvertBatchStatusToPaymentStatus(BatchStatus status)
        {
            return status switch
            {
                BatchStatus.Created => PaymentStatus.Draft,
                BatchStatus.Submitted => PaymentStatus.PendingApproval,
                BatchStatus.Approved => PaymentStatus.Approved,
                BatchStatus.Processed => PaymentStatus.Processed,
                BatchStatus.Failed => PaymentStatus.Failed,
                _ => PaymentStatus.Draft
            };
        }
    }
}
