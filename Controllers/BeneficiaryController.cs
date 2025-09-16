using BankingPaymentsAPI.DTOs;
using BankingPaymentsAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankingPaymentsAPI.Controllers
{
    [ApiController]
    [Route("api/beneficiaries")]
    public class BeneficiaryController : ControllerBase
    {
        private readonly IBeneficiaryService _service;

        public BeneficiaryController(IBeneficiaryService service)
        {
            _service = service;
        }

        // Create a new beneficiary
        [HttpPost]
       // [Authorize(Roles = "Admin,BankUser")]
        public IActionResult Create([FromBody] BeneficiaryRequestDto dto, [FromQuery] int createdBy)
        {
            var createdBeneficiary = _service.CreateBeneficiary(dto, createdBy);
            return CreatedAtAction(nameof(GetById), new { id = createdBeneficiary.Id }, createdBeneficiary);
        }

        // Get beneficiary by ID
        [HttpGet("{id}")]
      //  [Authorize(Roles = "Admin,BankUser")]
        public IActionResult GetById(int id)
        {
            var beneficiary = _service.GetById(id);
            return beneficiary == null ? NotFound($"Beneficiary with ID {id} not found.") : Ok(beneficiary);
        }

        // Get all beneficiaries for a client
        [HttpGet("by-client/{clientId}")]
       // [Authorize(Roles = "Admin,BankUser")]
        public IActionResult GetByClient(int clientId)
        {
            var beneficiaries = _service.GetByClient(clientId);
            return Ok(beneficiaries);
        }

        // Update beneficiary
        [HttpPut("{id}")]
      //  [Authorize(Roles = "Admin,BankUser")]
        public IActionResult Update(int id, [FromBody] BeneficiaryRequestDto dto)
        {
            var updatedBeneficiary = _service.Update(id, dto);
            return updatedBeneficiary == null ? NotFound($"Beneficiary with ID {id} not found.") : Ok(updatedBeneficiary);
        }

        // Soft delete beneficiary
        [HttpDelete("{id}")]
       // [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            var result = _service.Delete(id);
            return result ? NoContent() : NotFound($"Beneficiary with ID {id} not found.");
        }
    }
}
