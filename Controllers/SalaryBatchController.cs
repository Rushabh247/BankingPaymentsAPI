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
        [Authorize(Roles = "Admin,ClientUser")]
        public IActionResult Create([FromBody] SalaryBatchRequestDto dto)
        {
            var createdBy = GetCurrentUserId();
            var createdBatch = _service.CreateBatch(dto, createdBy);

            return CreatedAtAction(nameof(GetById), new { id = createdBatch.Id }, createdBatch);
        }

        // Get batch by Id
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,BankUser,ClientUser")]
        public IActionResult GetById(int id)
        {
            var batch = _service.GetBatchById(id);
            return batch == null ? NotFound($"SalaryBatch with ID {id} not found.") : Ok(batch);
        }

        // Get all batches for a client
        [HttpGet("by-client/{clientId}")]
        [Authorize(Roles = "Admin,BankUser,ClientUser")]
        public IActionResult GetByClient(int clientId)
        {
            var batches = _service.GetBatchesByClient(clientId);
            return Ok(batches);
        }

        // Submit batch for processing
        [HttpPut("submit/{id}")]
        [Authorize(Roles = "Admin,BankUser")]
        public IActionResult Submit(int id)
        {
            var submittedBy = GetCurrentUserId();
            var batch = _service.ProcessBatch(id, submittedBy, "Submitted for processing");

            return batch == null ? NotFound($"SalaryBatch with ID {id} not found.") : Ok(batch);
        }

        // Delete batch
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,BankUser")]
        public IActionResult Delete(int id)
        {
            var result = _service.DeleteBatch(id);
            return result ? NoContent() : NotFound($"SalaryBatch with ID {id} not found.");
        }

        // Confirm Stripe payment (called from webhook or after approval)
        [HttpPut("confirm-stripe/{paymentIntentId}")]
        [Authorize(Roles = "Admin,BankUser")]
        public IActionResult ConfirmStripe(string paymentIntentId)
        {
            var approverId = GetCurrentUserId();
            var payment = _service.ConfirmStripeSalaryPayment(paymentIntentId, approverId);

            return payment == null ? NotFound($"No payment found for Stripe Intent ID {paymentIntentId}.") : Ok(payment);
        }

        // Get payments for a batch
        [HttpGet("{batchId}/payments")]
        [Authorize(Roles = "Admin,BankUser,ClientUser")]
        public IActionResult GetPaymentsByBatch(int batchId)
        {
            var payments = _service.GetBatchById(batchId)?.Items;
            return payments == null ? NotFound($"No payments found for batch {batchId}.") : Ok(payments);
        }

        // Retry failed payments in a batch
        [HttpPut("{batchId}/retry-failed")]
        [Authorize(Roles = "Admin,BankUser,ClientUser")]
        public IActionResult RetryFailedPayments(int batchId)
        {
            var batch = _service.GetBatchesByClient(batchId)
                .FirstOrDefault(b => b.Id == batchId);

            if (batch == null) return NotFound($"Batch {batchId} not found.");

            var retried = batch.Items
                .Where(p => p.Status == Enums.PaymentStatus.Failed)
                .Select(p => _service.RetryFailedPayment(p.Id, GetCurrentUserId()))
                .ToList();

            return Ok(retried);
        }

        // helper: extract current userId from claims
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("userId")?.Value;
            return int.TryParse(userIdClaim, out var id) ? id : 0;
        }
    }
}
