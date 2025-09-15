using BankingPaymentsAPI.Models;

namespace BankingPaymentsAPI.Repository
{
    public interface IPaymentRepository
    {
        Payment Add(Payment payment);
        Payment? GetById(int id);
        IEnumerable<Payment> GetAll();
        void Update(Payment payment);
        void Delete(Payment payment);
    }
}
