using BankingPaymentsAPI.DTOs;
using BankingPaymentsAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankingPaymentsAPI.Controllers
{
    [ApiController]
    [Route("api/salary-batches")]
    public class SalaryBatchController : ControllerBase
    {
        private readonly ISalaryService _service;

        public SalaryBatchController(ISalaryService service)
        {
            _service = service;
        }

        // Create new batch
        [HttpPost]
       // [Authorize(Roles = "Admin,BankUser")]
        public IActionResult Create([FromBody] SalaryBatchRequestDto dto)
        {
            var createdBy = GetCurrentUserId();
            var createdBatch = _service.CreateBatch(dto, createdBy);

            return CreatedAtAction(nameof(GetById), new { id = createdBatch.Id }, createdBatch);
        }

        // Get batch by Id
        [HttpGet("{id}")]
       // [Authorize(Roles = "Admin,BankUser")]
        public IActionResult GetById(int id)
        {
            var batch = _service.GetBatchById(id);
            return batch == null ? NotFound($"SalaryBatch with ID {id} not found.") : Ok(batch);
        }

        // Get all batches for a client
        [HttpGet("by-client/{clientId}")]
       // [Authorize(Roles = "Admin,BankUser")]
        public IActionResult GetByClient(int clientId)
        {
            var batches = _service.GetBatchesByClient(clientId);
            return Ok(batches);
        }

        // Submit batch
        [HttpPut("submit/{id}")]
       // [Authorize(Roles = "Admin,BankUser")]
        public IActionResult Submit(int id)
        {
            var submittedBy = GetCurrentUserId();
            var batch = _service.SubmitBatch(id, submittedBy);

            return batch == null ? NotFound($"SalaryBatch with ID {id} not found.") : Ok(batch);
        }

        // Delete batch
        [HttpDelete("{id}")]
       // [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            var result = _service.DeleteBatch(id);
            return result ? NoContent() : NotFound($"SalaryBatch with ID {id} not found.");
        }

        // helper: extract current userId from claims
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("userId")?.Value;
            return int.TryParse(userIdClaim, out var id) ? id : 0;
        }
    }
}
