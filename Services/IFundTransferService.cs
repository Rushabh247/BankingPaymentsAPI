using BankingPaymentsAPI.Enums;

namespace BankingPaymentsAPI.Services
{
    public interface IFundTransferService
    {
       
        bool TransferFunds(int fromId, AccountHolderType fromType, int toId, AccountHolderType toType, decimal amount);
    }
}
