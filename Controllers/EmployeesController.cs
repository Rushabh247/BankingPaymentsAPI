using BankingPaymentsAPI.DTOs;
using BankingPaymentsAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankingPaymentsAPI.Controllers
{
    [ApiController]
    [Route("api/employees")]
    public class EmployeesController : ControllerBase
    {
        private readonly IEmployeeService _service;

        public EmployeesController(IEmployeeService service)
        {
            _service = service;
        }

        // ✅ Create new Employee
        [HttpPost]
       // [Authorize(Roles = "Admin,BankUser")]
        public IActionResult Create([FromBody] EmployeeRequestDto dto)
        {
            // Assuming current user ID comes from token/claims, 
            // for now pass createdBy = 1 (hardcoded).
            var createdEmployee = _service.CreateEmployee(dto, createdBy: 1);
            return CreatedAtAction(nameof(GetById), new { id = createdEmployee.Id }, createdEmployee);
        }

        // ✅ Get Employee by Id
        [HttpGet("{id}")]
      //  [Authorize(Roles = "Admin,BankUser")]
        public IActionResult GetById(int id)
        {
            var employee = _service.GetById(id);
            return employee == null ? NotFound($"Employee with ID {id} not found.") : Ok(employee);
        }

        // ✅ Get Employees by ClientId
        [HttpGet("by-client/{clientId}")]
      //  [Authorize(Roles = "Admin,BankUser")]
        public IActionResult GetByClient(int clientId)
        {
            var employees = _service.GetByClient(clientId);
            return employees == null || !employees.Any()
                ? NotFound($"No employees found for Client ID {clientId}.")
                : Ok(employees);
        }

        // ✅ Update Employee
        [HttpPut("{id}")]
      //  [Authorize(Roles = "Admin")]
        public IActionResult Update(int id, [FromBody] EmployeeRequestDto dto)
        {
            var updatedEmployee = _service.Update(id, dto);
            return updatedEmployee == null
                ? NotFound($"Employee with ID {id} not found.")
                : Ok(updatedEmployee);
        }

        // ✅ Delete Employee (hard delete as per service)
        [HttpDelete("{id}")]
       // [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            var result = _service.Delete(id);
            return result ? NoContent() : NotFound($"Employee with ID {id} not found.");
        }
    }
}
