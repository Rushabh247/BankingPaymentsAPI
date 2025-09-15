using BankingPaymentsAPI.Models;

using System.Collections.Generic;

namespace BankingPaymentsAPI.Repository
{
    public interface IEmployeeRepository
    {
        Employee Add(Employee employee);
        Employee? GetById(int id);
        IEnumerable<Employee> GetByClientId(int clientId);
        IEnumerable<Employee> GetAll();
        void Update(Employee employee);
        void Delete(Employee employee);
    }
}
