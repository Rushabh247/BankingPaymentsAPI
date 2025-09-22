using BankingPaymentsAPI.DTOs;
using BankingPaymentsAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankingPaymentsAPI.Controllers
{
    [ApiController]
    [Route("api/banks")]
    public class BankController : ControllerBase
    {
        private readonly IBankService _service;

        public BankController(IBankService service)
        {
            _service = service;
        }

        // Create a new bank
        [HttpPost]
       [Authorize(Roles = "SuperAdmin")] // only Admin should create banks
        public IActionResult Create([FromBody] BankRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var createdBank = _service.CreateBank(dto);
            return CreatedAtAction(nameof(GetById), new { id = createdBank.Id }, createdBank);
        }

        // Get bank by Id
        [HttpGet("{id}")]
        [Authorize(Roles = "SuperAdmin")]
        public IActionResult GetById(int id)
        {
            var bank = _service.GetBankById(id);
            return bank == null ? NotFound($"Bank with ID {id} not found.") : Ok(bank);
        }

        // Get all banks
        [HttpGet]
       [Authorize(Roles = "SuperAdmin")]
        public IActionResult GetAll()
        {
            var banks = _service.GetAllBanks();
            return Ok(banks);
        }

        // Update a bank
        [HttpPut("{id}")]
        [Authorize(Roles = "SuperAdmin")]
        public IActionResult Update(int id, [FromBody] BankUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updatedBank = _service.UpdateBank(id, dto);
            return updatedBank == null ? NotFound($"Bank with ID {id} not found.") : Ok(updatedBank);
        }

        // Delete a bank
        [HttpDelete("{id}")]
        [Authorize(Roles = "SuperAdmin")]
        public IActionResult Delete(int id)
        {
            var result = _service.DeleteBank(id);
            return result ? NoContent() : NotFound($"Bank with ID {id} not found.");
        }
    }
}
