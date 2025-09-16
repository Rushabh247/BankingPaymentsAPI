using BankingPaymentsAPI.DTOs;
using BankingPaymentsAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankingPaymentsAPI.Controllers
{
    [ApiController]
    [Route("api/reports")]
    public class ReportController : ControllerBase
    {
        private readonly IReportService _service;

        public ReportController(IReportService service)
        {
            _service = service;
        }

        // Request a new report
        [HttpPost]
       // [Authorize(Roles = "Admin,BankUser")]
        public IActionResult RequestReport([FromBody] ReportRequestCreateDto dto, [FromQuery] int requestedBy)
        {
            var report = _service.RequestReport(dto, requestedBy);
            return CreatedAtAction(nameof(GetById), new { id = report.Id }, report);
        }

        // Get report by ID
        [HttpGet("{id}")]
      //  [Authorize(Roles = "Admin,BankUser")]
        public IActionResult GetById(int id)
        {
            var report = _service.GetById(id);
            return report == null ? NotFound($"Report with ID {id} not found.") : Ok(report);
        }

        // Get all reports
        [HttpGet]
      //  [Authorize(Roles = "Admin,BankUser")]
        public IActionResult GetAll()
        {
            var reports = _service.GetAll();
            return Ok(reports);
        }

        // Mark report as completed
        [HttpPut("complete/{id}")]
       // [Authorize(Roles = "Admin")]
        public IActionResult MarkCompleted(int id, [FromQuery] string resultUrl)
        {
            _service.MarkCompleted(id, resultUrl);
            return NoContent();
        }

        // Mark report as failed
        [HttpPut("fail/{id}")]
      //  [Authorize(Roles = "Admin")]
        public IActionResult MarkFailed(int id, [FromQuery] string reason)
        {
            _service.MarkFailed(id, reason);
            return NoContent();
        }
    }
}
