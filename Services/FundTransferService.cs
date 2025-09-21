using BankingPaymentsAPI.Data;
using BankingPaymentsAPI.Enums;
using Microsoft.EntityFrameworkCore;
using System;

namespace BankingPaymentsAPI.Services
{
    public class FundTransferService : IFundTransferService
    {
        private readonly AppDbContext _context;

        public FundTransferService(AppDbContext context)
        {
            _context = context;
        }

        public bool TransferFunds(int fromId, AccountHolderType fromType, int toId, AccountHolderType toType, decimal amount)
        {
            using var transaction = _context.Database.BeginTransaction();

            try
            {
                // Debit
                if (!AdjustBalance(fromId, fromType, -amount))
                    throw new Exception("Insufficient funds or entity not found");

                // Credit
                if (!AdjustBalance(toId, toType, amount))
                    throw new Exception("Recipient not found");

                _context.SaveChanges();
                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                return false;
            }
        }

        private bool AdjustBalance(int entityId, AccountHolderType type, decimal amount)
        {
            switch (type)
            {
                case AccountHolderType.Client:
                    var client = _context.Clients.Find(entityId);
                    if (client == null) return false;
                    if (client.Balance + amount < 0) return false;
                    client.Balance += amount;
                    _context.Clients.Update(client);
                    break;

                case AccountHolderType.Beneficiary:
                    var beneficiary = _context.Beneficiaries.Find(entityId);
                    if (beneficiary == null) return false;
                    if (beneficiary.Balance + amount < 0) return false;
                    beneficiary.Balance += amount;
                    _context.Beneficiaries.Update(beneficiary);
                    break;

                case AccountHolderType.Employee:
                    var employee = _context.Employees.Find(entityId);
                    if (employee == null) return false;
                    if (employee.Balance + amount < 0) return false;
                    employee.Balance += amount;
                    _context.Employees.Update(employee);
                    break;

                default:
                    return false;
            }

            return true;
        }
    }
}
