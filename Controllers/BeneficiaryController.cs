using BankingPaymentsAPI.DTOs;
using BankingPaymentsAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

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
        [Authorize(Roles = "SuperAdmin,ClientUser")]
        public async Task<IActionResult> Create([FromBody] BeneficiaryRequestDto dto, [FromQuery] int createdBy)
        {
            var createdBeneficiary = await _service.CreateBeneficiaryAsync(dto, createdBy);
            return CreatedAtAction(nameof(GetById), new { id = createdBeneficiary.Id }, createdBeneficiary);
        }

        // Get beneficiary by ID
        [HttpGet("{id}")]
         [Authorize(Roles = "SuperAdmin,BankUser,ClientUser")]
        public async Task<IActionResult> GetById(int id)
        {
            var beneficiary = await _service.GetByIdAsync(id);
            return beneficiary == null ? NotFound($"Beneficiary with ID {id} not found.") : Ok(beneficiary);
        }

        // Get all beneficiaries for a client
        [HttpGet("by-client/{clientId}")]
        [Authorize(Roles = "SuperAdmin,ClientUser")]
        public async Task<IActionResult> GetByClient(int clientId)
        {
            var beneficiaries = await _service.GetByClientAsync(clientId);
            return Ok(beneficiaries);
        }

        // Update beneficiary
        [HttpPut("{id}")]
         [Authorize(Roles = "SuperAdmin,ClientUser")]
        public async Task<IActionResult> Update(int id, [FromBody] BeneficiaryRequestDto dto)
        {
            var updatedBeneficiary = await _service.UpdateAsync(id, dto);
            return updatedBeneficiary == null ? NotFound($"Beneficiary with ID {id} not found.") : Ok(updatedBeneficiary);
        }

        // Soft delete beneficiary
        [HttpDelete("{id}")]
         [Authorize(Roles = "SuperAdmin,ClientUser")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.DeleteAsync(id);
            return result ? NoContent() : NotFound($"Beneficiary with ID {id} not found.");
        }
    }
}
