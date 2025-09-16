using BankingPaymentsAPI.DTOs;
using BankingPaymentsAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
       // [Authorize(Roles = "Admin,BankUser")]
        public IActionResult Create([FromBody] ClientRequestDto dto, [FromQuery] int createdByUserId)
        {
            var client = _service.CreateClient(dto, createdByUserId);
            return CreatedAtAction(nameof(GetById), new { id = client.Id }, client);
        }

        // Get client by ID
        [HttpGet("{id}")]
       // [Authorize(Roles = "Admin,BankUser")]
        public IActionResult GetById(int id)
        {
            var client = _service.GetClientById(id);
            return client == null ? NotFound($"Client with ID {id} not found.") : Ok(client);
        }

        // Get all clients
        [HttpGet]
//        [Authorize(Roles = "Admin,BankUser")]
        public IActionResult GetAll()
        {
            var clients = _service.GetAllClients();
            return Ok(clients);
        }

        // Update client
        [HttpPut("{id}")]
      //  [Authorize(Roles = "Admin")]
        public IActionResult Update(int id, [FromBody] ClientUpdateDto dto)
        {
            var updatedClient = _service.UpdateClient(id, dto);
            return updatedClient == null ? NotFound($"Client with ID {id} not found.") : Ok(updatedClient);
        }

        // Delete client
        [HttpDelete("{id}")]
       // [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            var result = _service.DeleteClient(id);
            return result ? NoContent() : NotFound($"Client with ID {id} not found.");
        }
    }
}
