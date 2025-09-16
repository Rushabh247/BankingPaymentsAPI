using BankingPaymentsAPI.DTOs;
using BankingPaymentsAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankingPaymentsAPI.Controllers
{
    [ApiController]
    [Route("api/payments")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _service;

        public PaymentController(IPaymentService service)
        {
            _service = service;
        }

        // Create a new payment
        [HttpPost]
        //[Authorize(Roles = "Admin,BankUser")]
        public IActionResult CreatePayment([FromBody] PaymentRequestDto dto, [FromQuery] int createdByUserId)
        {
            var payment = _service.CreatePayment(dto, createdByUserId);
            return CreatedAtAction(nameof(GetById), new { id = payment.Id }, payment);
        }

        // Get a payment by ID
        [HttpGet("{id}")]
      //  [Authorize(Roles = "Admin,BankUser")]
        public IActionResult GetById(int id)
        {
            var payment = _service.GetPaymentById(id);
            return payment == null ? NotFound($"Payment with ID {id} not found.") : Ok(payment);
        }

        // Get all payments
        [HttpGet]
       // [Authorize(Roles = "Admin,BankUser")]
        public IActionResult GetAll()
        {
            var payments = _service.GetAllPayments();
            return Ok(payments);
        }

        // Approve a payment
        [HttpPut("approve/{id}")]
      //  [Authorize(Roles = "Admin")]
        public IActionResult ApprovePayment(int id, [FromQuery] int approverId, [FromQuery] string remarks)
        {
            var payment = _service.ApprovePayment(id, approverId, remarks);
            return payment == null ? NotFound($"Payment with ID {id} not found.") : Ok(payment);
        }

        // Delete a payment
        [HttpDelete("{id}")]
      //  [Authorize(Roles = "Admin")]
        public IActionResult DeletePayment(int id)
        {
            var result = _service.DeletePayment(id);
            return result ? NoContent() : NotFound($"Payment with ID {id} not found.");
        }
    }
}
