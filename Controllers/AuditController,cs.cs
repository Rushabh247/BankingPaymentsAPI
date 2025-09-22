using BankingPaymentsAPI.DTOs;
using BankingPaymentsAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankingPaymentsAPI.Controllers
{
    [ApiController]
    [Route("api/audit-logs")]
    public class AuditLogsController : ControllerBase
    {
        private readonly IAuditService _service;

        public AuditLogsController(IAuditService service)
        {
            _service = service;
        }

 
      
      
        [HttpPost]
        [Authorize(Roles = "SuperAdmin")]
        public IActionResult Create([FromBody] CreateAuditLogDto dto)
        {
            var createdLog = _service.Log(dto);
            return Ok(createdLog); // returning Ok since no GetById
        }

    
        /// Get all audit logs
  
        [HttpGet]
        [Authorize(Roles = "SuperAdmin")]
        public IActionResult GetAll()
        {
            var logs = _service.GetAll();
            return Ok(logs);
        }


        /// Get all logs for a specific entity
   
        [HttpGet("entity/{entityName}/{entityId}")]
        [Authorize(Roles = "SuperAdmin")]
        public IActionResult GetByEntity(string entityName, int entityId)
        {
            var logs = _service.GetByEntity(entityName, entityId);
            return Ok(logs);
        }

  
        /// Get all logs for a specific user
  
        [HttpGet("user/{userId}")]
       [Authorize(Roles = "SuperAdmin")]
        public IActionResult GetByUser(int userId)
        {
            var logs = _service.GetByUser(userId);
            return Ok(logs);
        }
    }
}
