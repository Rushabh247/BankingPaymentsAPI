using BankingPaymentsAPI.DTOs;
using BankingPaymentsAPI.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

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

        [HttpPost]
        public async Task<IActionResult> RequestReport([FromBody] ReportRequestCreateDto dto, [FromQuery] int requestedBy)
        {
            var report = await _service.RequestReportAsync(dto, requestedBy);
            return CreatedAtAction(nameof(GetById), new { id = report.Id }, report);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var report = await _service.GetByIdAsync(id);
            return report == null ? NotFound($"Report with ID {id} not found.") : Ok(report);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? clientId, [FromQuery] string fromDate, [FromQuery] string toDate)
        {
            var reports = await _service.GetReportsAsync(clientId, fromDate, toDate);
            return Ok(reports);
        }

        [HttpGet("type/{reportType}")]
        public async Task<IActionResult> GetByType(string reportType, [FromQuery] int? clientId)
        {
            var data = await _service.GetReportDataByTypeAsync(reportType, clientId);
            return Ok(data);
        }

        [HttpPut("generate/{id}")]
        public async Task<IActionResult> GenerateReport(int id)
        {
            await _service.GenerateAndCompleteReportAsync(id);
            return NoContent();
        }

        [HttpPut("fail/{id}")]
        public async Task<IActionResult> MarkFailed(int id, [FromQuery] string reason)
        {
            await _service.MarkFailedAsync(id, reason);
            return NoContent();
        }
    }
}
