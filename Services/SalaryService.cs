using BankingPaymentsAPI.DTOs;
using BankingPaymentsAPI.Enums;
using BankingPaymentsAPI.Models;
using BankingPaymentsAPI.Repository;
using BankingPaymentsAPI.Services.PaymentProcessing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BankingPaymentsAPI.Services
{
    public class SalaryService : ISalaryService
    {
        private readonly ISalaryRepository _salaryRepo;
        private readonly IFundTransferService _fundTransfer;
        private readonly IStripePaymentService _stripe;
        private readonly ITransactionService _txnService;

        public SalaryService(
            ISalaryRepository salaryRepo,
            IFundTransferService fundTransfer,
            IStripePaymentService stripe,
            ITransactionService txnService)
        {
            _salaryRepo = salaryRepo;
            _fundTransfer = fundTransfer;
            _stripe = stripe;
            _txnService = txnService;
        }

        #region Batch

        public SalaryBatchDto CreateBatch(SalaryBatchRequestDto request, int createdByUserId)
        {
            // Step 1: Create batch with empty items
            var batch = new SalaryBatch
            {
                ClientId = request.ClientId,
                BatchCode = request.BatchCode,
                TotalAmount = request.Items.Sum(i => i.Amount),
                Status = BatchStatus.Created,
                Items = new List<SalaryPayment>()
            };

            batch = _salaryRepo.AddBatch(batch); // Save batch first to generate Id

            // Step 2: Add payments now that batch Id exists
            foreach (var i in request.Items)
            {
                var payment = new SalaryPayment
                {
                    EmployeeId = i.EmployeeId,
                    Amount = i.Amount,
                    Status = PaymentStatus.PendingApproval,
                    Method = request.Method,
                    TxnRef = request.Method == PaymentMethod.Internal ? $"TEMP-{Guid.NewGuid()}" : null,
                    SalaryBatchId = batch.Id
                };

                _salaryRepo.UpdatePayment(payment); // Save payment individually
            }

            // Reload batch with items for DTO
            batch = _salaryRepo.GetBatchById(batch.Id);
            return MapBatchToDto(batch);
        }

        public SalaryBatchDto? ProcessBatch(int batchId, int approverId, string remarks)
        {
            var batch = _salaryRepo.GetBatchById(batchId);
            if (batch == null) return null;

            foreach (var payment in batch.Items.ToList()) // ToList avoids modification issues
            {
                if (payment.Status == PaymentStatus.Processed) continue;

                try
                {
                    if (payment.Method == PaymentMethod.Internal)
                    {
                        _fundTransfer.TransferFunds(
                            batch.ClientId, AccountHolderType.Client,
                            payment.EmployeeId, AccountHolderType.Employee,
                            payment.Amount
                        );

                        payment.Status = PaymentStatus.Processed;
                        payment.TxnRef = $"TXN-{Guid.NewGuid()}";

                        _txnService.RecordTransaction(new TransactionDto
                        {
                            PaymentId = null,
                            SalaryPaymentId = payment.Id,
                            Amount = payment.Amount,
                            DebitAccountMasked = MaskAccount(batch.Client.Beneficiaries.FirstOrDefault()?.AccountNumber ?? ""),
                            CreditAccountMasked = MaskAccount(payment.Employee.AccountNumber),
                            TransactionDate = DateTimeOffset.UtcNow,
                            Status = "SUCCESS",
                            ExternalTxnRef = payment.TxnRef
                        });
                    }
                    else if (payment.Method == PaymentMethod.Stripe)
                    {
                        var intent = _stripe.CreatePaymentIntent(payment.Amount, "INR", $"salary_{payment.Id}");
                        payment.StripePaymentIntentId = intent.Id;
                        payment.Status = PaymentStatus.PendingApproval;
                    }
                }
                catch (Exception ex)
                {
                    payment.Status = PaymentStatus.Failed;
                    payment.FailureReason = ex.Message;
                }

                _salaryRepo.UpdatePayment(payment);
            }

            // ✅ Update batch status correctly with BatchStatus enum
            batch.Status = batch.Items.All(p => p.Status == PaymentStatus.Processed) ? BatchStatus.Processed :
                           batch.Items.All(p => p.Status == PaymentStatus.Failed) ? BatchStatus.Failed :
                           BatchStatus.PartiallyProcessed;

            _salaryRepo.UpdateBatch(batch);
            return MapBatchToDto(batch);
        }

        public SalaryPaymentDto? RetryFailedPayment(int paymentId, int approverId)
        {
            var payment = _salaryRepo.GetPaymentById(paymentId);
            if (payment == null || payment.Status != PaymentStatus.Failed) return null;

            try
            {
                if (payment.Method == PaymentMethod.Internal)
                {
                    _fundTransfer.TransferFunds(
                        payment.SalaryBatch.ClientId, AccountHolderType.Client,
                        payment.EmployeeId, AccountHolderType.Employee, payment.Amount
                    );

                    payment.Status = PaymentStatus.Processed;
                    payment.TxnRef = $"RETRY-{Guid.NewGuid()}";
                    payment.FailureReason = null;

                    _txnService.RecordTransaction(new TransactionDto
                    {
                        PaymentId = null,
                        SalaryPaymentId = payment.Id,
                        Amount = payment.Amount,
                        DebitAccountMasked = MaskAccount(payment.SalaryBatch.Client.Beneficiaries.FirstOrDefault()?.AccountNumber ?? ""),
                        CreditAccountMasked = MaskAccount(payment.Employee.AccountNumber),
                        TransactionDate = DateTimeOffset.UtcNow,
                        Status = "SUCCESS",
                        ExternalTxnRef = payment.TxnRef
                    });
                }
                else if (payment.Method == PaymentMethod.Stripe)
                {
                    var intent = _stripe.CreatePaymentIntent(payment.Amount, "INR", $"salary_{payment.Id}");
                    payment.StripePaymentIntentId = intent.Id;
                    payment.Status = PaymentStatus.PendingApproval;
                    payment.FailureReason = null;
                }

                _salaryRepo.UpdatePayment(payment);
            }
            catch (Exception ex)
            {
                payment.Status = PaymentStatus.Failed;
                payment.FailureReason = ex.Message;
                _salaryRepo.UpdatePayment(payment);
            }

            return MapPaymentToDto(payment);
        }

        #endregion

        #region Stripe

        public SalaryPaymentDto CreateStripePayment(int employeeId, decimal amount, int batchId, int createdByUserId)
        {
            var batch = _salaryRepo.GetBatchById(batchId) ?? throw new Exception("Batch not found");

            var payment = new SalaryPayment
            {
                EmployeeId = employeeId,
                Amount = amount,
                Method = PaymentMethod.Stripe,
                Status = PaymentStatus.PendingApproval,
                SalaryBatchId = batchId
            };

            var intent = _stripe.CreatePaymentIntent(amount, "INR", $"salary_{payment.Id}");
            payment.StripePaymentIntentId = intent.Id;

            _salaryRepo.UpdatePayment(payment); // Save payment

            batch = _salaryRepo.GetBatchById(batchId);
            return MapPaymentToDto(payment);
        }

        public SalaryPaymentDto? ConfirmStripeSalaryPayment(string paymentIntentId, int approverId)
        {
            var payment = _salaryRepo.GetPaymentByStripeId(paymentIntentId);
            if (payment == null) return null;

            var intent = _stripe.GetPaymentIntent(paymentIntentId);

            if (intent.Status == "succeeded")
            {
                _fundTransfer.TransferFunds(
                    payment.SalaryBatch.ClientId, AccountHolderType.Client,
                    payment.EmployeeId, AccountHolderType.Employee, payment.Amount
                );

                payment.Status = PaymentStatus.Processed;
                payment.TxnRef = $"TXN-{Guid.NewGuid()}";

                _txnService.RecordTransaction(new TransactionDto
                {
                    PaymentId = null,
                    SalaryPaymentId = payment.Id,
                    Amount = payment.Amount,
                    DebitAccountMasked = MaskAccount(payment.SalaryBatch.Client.Beneficiaries.FirstOrDefault()?.AccountNumber ?? ""),
                    CreditAccountMasked = MaskAccount(payment.Employee.AccountNumber),
                    TransactionDate = DateTimeOffset.UtcNow,
                    Status = "SUCCESS",
                    ExternalTxnRef = payment.TxnRef
                });
            }
            else
            {
                payment.Status = PaymentStatus.Failed;
                payment.FailureReason = $"Stripe status: {intent.Status}";
            }

            _salaryRepo.UpdatePayment(payment);

            var batch = _salaryRepo.GetBatchById(payment.SalaryBatchId);
            if (batch != null)
            {
                batch.Status = batch.Items.All(p => p.Status == PaymentStatus.Processed) ? BatchStatus.Processed :
                               batch.Items.All(p => p.Status == PaymentStatus.Failed) ? BatchStatus.Failed :
                               BatchStatus.PartiallyProcessed;
                _salaryRepo.UpdateBatch(batch);
            }

            return MapPaymentToDto(payment);
        }

        #endregion

        #region Queries / Delete

        public SalaryBatchDto? GetBatchById(int id) =>
            _salaryRepo.GetBatchById(id) is SalaryBatch b ? MapBatchToDto(b) : null;

        public IEnumerable<SalaryBatchDto> GetBatchesByClient(int clientId) =>
            _salaryRepo.GetBatchesByClient(clientId).Select(MapBatchToDto);

        public SalaryPaymentDto? GetPaymentById(int id) =>
            _salaryRepo.GetPaymentById(id) is SalaryPayment p ? MapPaymentToDto(p) : null;

        public bool DeleteBatch(int id)
        {
            var batch = _salaryRepo.GetBatchById(id);
            if (batch == null) return false;
            _salaryRepo.DeleteBatch(batch);
            return true;
        }

        #endregion

        #region Helpers

        private SalaryBatchDto MapBatchToDto(SalaryBatch b) => new SalaryBatchDto
        {
            Id = b.Id,
            ClientId = b.ClientId,
            BatchCode = b.BatchCode,
            TotalAmount = b.TotalAmount,
            Status = b.Status, // ✅ Now uses BatchStatus
            Items = b.Items.Select(MapPaymentToDto).ToList(),
            Method = b.Items.FirstOrDefault()?.Method ?? PaymentMethod.Internal
        };

        private SalaryPaymentDto MapPaymentToDto(SalaryPayment p) => new SalaryPaymentDto
        {
            Id = p.Id,
            EmployeeId = p.EmployeeId,
            EmployeeName = p.Employee?.FullName ?? "Unknown",
            Amount = p.Amount,
            Status = p.Status,
            TxnRef = p.TxnRef,
            FailureReason = p.FailureReason,
            SalaryBatchId = p.SalaryBatchId,
            Method = p.Method,
            StripePaymentIntentId = p.StripePaymentIntentId
        };

        private string MaskAccount(string accountNumber)
        {
            if (string.IsNullOrEmpty(accountNumber) || accountNumber.Length <= 4) return "****";
            return new string('*', accountNumber.Length - 4) + accountNumber[^4..];
        }

        #endregion
    }
}
