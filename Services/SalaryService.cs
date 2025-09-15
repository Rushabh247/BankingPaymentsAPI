using BankingPaymentsAPI.DTOs;
using BankingPaymentsAPI.Enums;
using BankingPaymentsAPI.Models;
using BankingPaymentsAPI.Repository;

using System;
using System.Collections.Generic;
using System.Linq;

namespace BankingPaymentsAPI.Services
{
    public class SalaryService : ISalaryService
    {
        private readonly ISalaryRepository _repo;
        private readonly IEmployeeRepository _employeeRepo;

        public SalaryService(ISalaryRepository repo, IEmployeeRepository employeeRepo)
        {
            _repo = repo;
            _employeeRepo = employeeRepo;
        }

        public SalaryBatchDto CreateBatch(SalaryBatchRequestDto request, int createdBy)
        {
            var batch = new SalaryBatch
            {
                ClientId = request.ClientId,
                BatchCode = request.BatchCode,
                Status = BatchStatus.Created,
                TotalAmount = request.Items.Sum(i => i.Amount),
                Items = request.Items.Select(i => new SalaryPayment
                {
                    EmployeeId = i.EmployeeId,
                    Amount = i.Amount,
                    Status = PaymentStatus.Draft
                }).ToList()
            };

            _repo.AddBatch(batch);
            return MapToDto(batch);
        }

        public SalaryBatchDto? GetBatchById(int id)
        {
            var b = _repo.GetBatchById(id);
            return b == null ? null : MapToDto(b);
        }

        public IEnumerable<SalaryBatchDto> GetBatchesByClient(int clientId)
        {
            return _repo.GetBatchesByClient(clientId).Select(MapToDto);
        }

        public SalaryBatchDto? SubmitBatch(int id, int submittedBy)
        {
            var b = _repo.GetBatchById(id);
            if (b == null) return null;
            b.Status = BatchStatus.Submitted;
            _repo.UpdateBatch(b);
            return MapToDto(b);
        }

        public bool DeleteBatch(int id)
        {
            var b = _repo.GetBatchById(id);
            if (b == null) return false;
            _repo.DeleteBatch(b);
            return true;
        }

        private SalaryBatchDto MapToDto(SalaryBatch b) =>
            new SalaryBatchDto
            {
                Id = b.Id,
                ClientId = b.ClientId,
                BatchCode = b.BatchCode,
                TotalAmount = b.TotalAmount,
                Status = b.Status.ToString(),
                Items = b.Items?.Select(i => new SalaryPaymentDto
                {
                    Id = i.Id,
                    EmployeeId = i.EmployeeId,
                    EmployeeName = i.Employee?.FullName ?? "Unknown",
                    Amount = i.Amount,
                    Status = i.Status.ToString(),
                    TxnRef = i.TxnRef
                }).ToList() ?? new List<SalaryPaymentDto>()
            };
    }
}
