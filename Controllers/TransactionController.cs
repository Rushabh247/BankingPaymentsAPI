using BankingPaymentsAPI.DTOs;
using BankingPaymentsAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace BankingPaymentsAPI.Controllers
{
    [ApiController]
    [Route("api/transactions")]
    public class TransactionController : ControllerBase
    {
        private readonly ITransactionService _service;

        public TransactionController(ITransactionService service)
        {
            _service = service;
        }

        // Record a new transaction
        [HttpPost]
      [Authorize(Roles = "Admin,BankUser")]
        public IActionResult RecordTransaction([FromBody] TransactionDto dto)
        {
            var recorded = _service.RecordTransaction(dto);
            return CreatedAtAction(nameof(GetById), new { id = recorded.Id }, recorded);
        }

        // Get transaction by ID
        [HttpGet("{id}")]
       [Authorize(Roles = "Admin,BankUser,ClientUser")]
        public IActionResult GetById(int id)
        {
            var txn = _service.GetById(id);
            return txn == null ? NotFound($"Transaction with ID {id} not found.") : Ok(txn);
        }

        // Get transactions by PaymentId
        [HttpGet("by-payment/{paymentId}")]
        [Authorize(Roles = "Admin,BankUser,ClientUser")]
        public ActionResult<IEnumerable<TransactionDto>> GetByPaymentId(int paymentId)
        {
            var txns = _service.GetByPaymentId(paymentId);
            return Ok(txns);
        }

        // Get all transactions
        [HttpGet]
       [Authorize(Roles = "Admin")]
        public ActionResult<IEnumerable<TransactionDto>> GetAll()
        {
            var txns = _service.GetAll();
            return Ok(txns);
        }
    }
}
