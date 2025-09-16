using BankingPaymentsAPI.DTOs;
using BankingPaymentsAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankingPaymentsAPI.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _service;

        public UsersController(IUserService service)
        {
            _service = service;
        }

       
        [HttpPost("register")]
        [AllowAnonymous] 
        public IActionResult Register([FromBody] RegisterUserDto dto)
        {
            var createdUser = _service.Register(dto);
            return CreatedAtAction(nameof(GetById), new { id = createdUser.Id }, createdUser);
        }

      
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,BankUser")]
        public IActionResult GetById(int id)
        {
            var user = _service.GetById(id);
            return user == null ? NotFound($"User with ID {id} not found.") : Ok(user);
        }

        [HttpGet("by-username-or-email/{usernameOrEmail}")]
        [Authorize(Roles = "Admin,BankUser")]
        public IActionResult GetByUsernameOrEmail(string usernameOrEmail)
        {
            var user = _service.GetByUsernameOrEmail(usernameOrEmail);
            return user == null ? NotFound($"User '{usernameOrEmail}' not found.") : Ok(user);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Update(int id, [FromBody] UpdateUserDto dto)
        {
            var updatedUser = _service.UpdateUser(id, dto);
            return updatedUser == null ? NotFound($"User with ID {id} not found.") : Ok(updatedUser);
        }

   
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult SoftDelete(int id)
        {
            var result = _service.SoftDeleteUser(id);
            return result ? NoContent() : NotFound($"User with ID {id} not found.");
        }
    }
}
