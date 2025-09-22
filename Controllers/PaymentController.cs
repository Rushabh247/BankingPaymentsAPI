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
        [Authorize(Roles = "Admin,ClientUser")]
        public IActionResult CreatePayment([FromBody] PaymentRequestDto dto, [FromQuery] int createdByUserId)
        {
            var payment = _service.CreatePayment(dto, createdByUserId);
            return CreatedAtAction(nameof(GetById), new { id = payment.Id }, payment);
        }

        // Get a payment by ID
        [HttpGet("{id}")]
          [Authorize(Roles = "Admin,BankUser,ClientUser")]
        public IActionResult GetById(int id)
        {
            var payment = _service.GetPaymentById(id);
            return payment == null ? NotFound($"Payment with ID {id} not found.") : Ok(payment);
        }

        // Get all payments
        [HttpGet]
         [Authorize(Roles = "Admin,BankUser")]
        public IActionResult GetAll()
        {
            var payments = _service.GetAllPayments();
            return Ok(payments);
        }

        // Approve a payment
        [HttpPut("approve/{id}")]
          [Authorize(Roles = "SuperAdmin,BankUser")]
        public IActionResult ApprovePayment(int id, [FromQuery] int approverId, [FromQuery] string remarks)
        {
            var payment = _service.ApprovePayment(id, approverId, remarks);
            return payment == null ? NotFound($"Payment with ID {id} not found.") : Ok(payment);
        }

        // Delete a payment
        [HttpDelete("{id}")]
          [Authorize(Roles = "SuperAdmin")]
        public IActionResult DeletePayment(int id)
        {
            var result = _service.DeletePayment(id);
            return result ? NoContent() : NotFound($"Payment with ID {id} not found.");
        }

        // Create Stripe payment
        [HttpPost("stripe")]
         [Authorize(Roles = "SuperAdmin,BankUser,ClientUser")]
        public IActionResult CreateStripePayment([FromBody] PaymentRequestDto dto, [FromQuery] int createdByUserId)
        {
            var payment = _service.CreateStripePayment(dto, createdByUserId);
            return CreatedAtAction(nameof(GetById), new { id = payment.Id }, payment);
        }

        // Confirm Stripe payment
        [HttpPost("stripe/confirm/{paymentIntentId}")]
          [Authorize(Roles = "SuperAdmin,BankUser")]
        public IActionResult ConfirmStripePayment(string paymentIntentId, [FromQuery] int approverId, [FromQuery] string remarks)
        {
            var payment = _service.ConfirmStripePayment(paymentIntentId, approverId, remarks);
            return payment == null ? NotFound($"Payment with PaymentIntent ID {paymentIntentId} not found.") : Ok(payment);
        }
    }
}
