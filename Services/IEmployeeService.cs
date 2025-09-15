using BankingPaymentsAPI.DTOs;
using System.Collections.Generic;

namespace BankingPaymentsAPI.Services
{
    public interface IEmployeeService
    {
        EmployeeDto CreateEmployee(EmployeeRequestDto dto, int createdBy);
        EmployeeDto? GetById(int id);
        IEnumerable<EmployeeDto> GetByClient(int clientId);
        EmployeeDto? Update(int id, EmployeeRequestDto dto);
        bool Delete(int id);
    }
}
