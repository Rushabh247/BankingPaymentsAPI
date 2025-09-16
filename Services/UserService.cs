using BankingPaymentsAPI.DTOs;
using BankingPaymentsAPI.Enums;
using BankingPaymentsAPI.Helpers.BankingPaymentsAPI.Helpers;
using BankingPaymentsAPI.Models;
using BankingPaymentsAPI.Repository;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace BankingPaymentsAPI.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repo;
        private readonly IAuditService _audit;
        private readonly IHttpContextAccessor _httpContext;

        public UserService(IUserRepository repo, IAuditService audit, IHttpContextAccessor httpContext)
        {
            _repo = repo;
            _audit = audit;
            _httpContext = httpContext;
        }

        public UserDto Register(RegisterUserDto dto)
        {
            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = PasswordHelper.Hash(dto.Password),
                Role = Enum.Parse<UserRole>(dto.Role, true),
                IsActive = true,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _repo.Add(user);

            //  Log CREATE
            _audit.Log(new CreateAuditLogDto
            {
                UserId = GetCurrentUserId(),
                Action = "CREATE_USER",
                EntityName = nameof(User),
                EntityId = user.Id,
                OldValueJson = null,
                NewValueJson = JsonSerializer.Serialize(user),
                IpAddress = GetClientIp()
            });

            return MapToDto(user);
        }

        public UserDto? UpdateUser(int id, UpdateUserDto dto)
        {
            var user = _repo.GetById(id);
            if (user == null) return null;

            var oldValue = JsonSerializer.Serialize(user);

            user.Username = dto.Username ?? user.Username;
            user.Email = dto.Email ?? user.Email;

            if (!string.IsNullOrEmpty(dto.Password))
                user.PasswordHash = PasswordHelper.Hash(dto.Password);

            if (!string.IsNullOrEmpty(dto.Role))
                user.Role = Enum.Parse<UserRole>(dto.Role, true);

            _repo.Update(user);

            //  Log UPDATE
            _audit.Log(new CreateAuditLogDto
            {
                UserId = GetCurrentUserId(),
                Action = "UPDATE_USER",
                EntityName = nameof(User),
                EntityId = user.Id,
                OldValueJson = oldValue,
                NewValueJson = JsonSerializer.Serialize(user),
                IpAddress = GetClientIp()
            });

            return MapToDto(user);
        }

        public bool SoftDeleteUser(int id)
        {
            var user = _repo.GetById(id);
            if (user == null) return false;

            var oldValue = JsonSerializer.Serialize(user);

            user.IsActive = false;
            _repo.Update(user);

            // Log DELETE (soft)
            _audit.Log(new CreateAuditLogDto
            {
                UserId = GetCurrentUserId(),
                Action = "SOFT_DELETE_USER",
                EntityName = nameof(User),
                EntityId = user.Id,
                OldValueJson = oldValue,
                NewValueJson = JsonSerializer.Serialize(user),
                IpAddress = GetClientIp()
            });

            return true;
        }

        public UserDto? GetById(int id)
        {
            var user = _repo.GetById(id);
            return user == null ? null : MapToDto(user);
        }

        public UserDto? GetByUsernameOrEmail(string usernameOrEmail)
        {
            var user = _repo.GetByUsernameOrEmail(usernameOrEmail);
            return user == null ? null : MapToDto(user);
        }

        private UserDto MapToDto(User user) =>
            new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role.ToString()
            };

        //  Helpers
        private int GetCurrentUserId()
        {
            var userIdClaim = _httpContext.HttpContext?.User?.FindFirst("userId")?.Value;
            return int.TryParse(userIdClaim, out var id) ? id : 0;
        }

        private string GetClientIp()
        {
            return _httpContext.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "unknown";
        }
    }
}
