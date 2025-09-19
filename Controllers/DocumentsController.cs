using BankingPaymentsAPI.DTOs;
using BankingPaymentsAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BankingPaymentsAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentsController : ControllerBase
    {
        private readonly IDocumentService _service;

        public DocumentsController(IDocumentService service)
        {
            _service = service;
        }

       
        [HttpPost("{clientId}/upload")]
      //  [Authorize] // require authentication
        public async Task<IActionResult> Upload(
            int clientId,
            IFormFile file,
            [FromQuery] string documentType,
            [FromQuery] int uploadedBy)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded");

            var result = await _service.UploadDocumentAsync(clientId, file, documentType, uploadedBy);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

     
        [HttpGet("{id}")]
     //   [Authorize]
        public async Task<IActionResult> GetById(int id)
        {
            var doc = await _service.GetByIdAsync(id);
            return doc == null ? NotFound() : Ok(doc);
        }

       
        [HttpGet("client/{clientId}")]
     //   [Authorize]
        public async Task<IActionResult> GetByClient(int clientId)
        {
            var docs = await _service.GetByClientAsync(clientId);
            return Ok(docs);
        }

        [HttpPut("status")]
        public async Task<IActionResult> UpdateStatus([FromBody] UpdateDocumentStatusDto dto)
        {
            var updatedDoc = await _service.UpdateStatusAsync(dto.DocumentId, dto.Status);
            return updatedDoc == null ? NotFound(new { message = "Invalid document or status" }) : Ok(updatedDoc);
        }


        [HttpDelete("{id}")]
       // [Authorize(Roles = "ADMIN")] // only admins can delete
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _service.DeleteAsync(id);
            return success ? NoContent() : NotFound();
        }
    }
}
