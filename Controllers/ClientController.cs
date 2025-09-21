using BankingPaymentsAPI.DTOs;
using BankingPaymentsAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using System.Threading.Tasks;

namespace BankingPaymentsAPI.Controllers
{
    [ApiController]
    [Route("api/clients")]
    public class ClientController : ControllerBase
    {
        private readonly IClientService _service;

        public ClientController(IClientService service)
        {
            _service = service;
        }

        // Create client
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ClientRequestDto dto, [FromQuery] int createdByUserId)
        {
            var client = await _service.CreateClientAsync(dto, createdByUserId);
            return CreatedAtAction(nameof(GetById), new { id = client.Id }, client);
        }

        // Get client by ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var client = await _service.GetClientByIdAsync(id);
            return client == null ? NotFound($"Client with ID {id} not found.") : Ok(client);
        }

        // Get all clients
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var clients = await _service.GetAllClientsAsync();
            return Ok(clients);
        }

        // Update client
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ClientUpdateDto dto)
        {
            var updatedClient = await _service.UpdateClientAsync(id, dto);
            return updatedClient == null ? NotFound($"Client with ID {id} not found.") : Ok(updatedClient);
        }

        // Delete client
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.DeleteClientAsync(id);
            return result ? NoContent() : NotFound($"Client with ID {id} not found.");
        }

        // Add internal money to client balance
        [HttpPost("{id}/add-money")]
        public async Task<IActionResult> AddMoney(int id, [FromQuery] decimal amount)
        {
            var success = await _service.AddMoneyAsync(id, amount);
            return success ? Ok($"Added ₹ {amount} to client {id} balance.")
                           : BadRequest("Failed to add money. Amount must be positive and client must exist.");
        }

        // Create Stripe top-up
        [HttpPost("{id}/stripe-topup")]
        public async Task<IActionResult> StripeTopUp(int id, [FromQuery] decimal amount)
        {
            var paymentIntent = await _service.TopUpViaStripeAsync(id, amount);
            if (paymentIntent == null)
                return BadRequest("Failed to create Stripe payment intent.");

            return Ok(paymentIntent);
        }

        // Confirm Stripe top-up
        [HttpPost("stripe-topup/confirm")]
        public async Task<IActionResult> ConfirmStripeTopUp([FromQuery] string paymentIntentId)
        {
            var success = await _service.ConfirmStripeTopUpAsync(paymentIntentId);
            if (!success)
                return BadRequest("Stripe payment not completed or failed.");

            var client = await _service.GetClientByStripePaymentIntentIdAsync(paymentIntentId);
            if (client == null)
                return BadRequest("Client not found after top-up.");

            return Ok(new
            {
                Message = "Stripe top-up successful.",
                CurrentBalance = $"₹{client.Balance}"
            });
        }
    }
}
