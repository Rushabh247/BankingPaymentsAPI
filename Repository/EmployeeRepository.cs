using BankingPaymentsAPI.Data;
using BankingPaymentsAPI.Models;

using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace BankingPaymentsAPI.Repository
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly AppDbContext _context;
        public EmployeeRepository(AppDbContext context) => _context = context;

        public Employee Add(Employee employee)
        {
            _context.Employees.Add(employee);
            _context.SaveChanges();
            return employee;
        }

        public Employee? GetById(int id)
        {
            return _context.Employees
                .Include(e => e.Client)
                .FirstOrDefault(e => e.Id == id);
        }

        public IEnumerable<Employee> GetByClientId(int clientId)
        {
            return _context.Employees.Where(e => e.ClientId == clientId).ToList();
        }

        public IEnumerable<Employee> GetAll()
        {
            return _context.Employees.Include(e => e.Client).ToList();
        }

        public void Update(Employee employee)
        {
            _context.Employees.Update(employee);
            _context.SaveChanges();
        }

        public void Delete(Employee employee)
        {
            _context.Employees.Remove(employee);
            _context.SaveChanges();
        }
    }
}
