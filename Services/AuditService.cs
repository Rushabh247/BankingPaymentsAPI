using BankingPaymentsAPI.DTOs;
using BankingPaymentsAPI.Models;
using BankingPaymentsAPI.Repository;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace BankingPaymentsAPI.Services
{
    public class AuditService : IAuditService
    {
        private readonly IAuditLogRepository _repo;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditService(IAuditLogRepository repo, IHttpContextAccessor httpContextAccessor)
        {
            _repo = repo;
            _httpContextAccessor = httpContextAccessor;
        }

        public AuditLogDto Log(CreateAuditLogDto dto)
        {
            var httpContext = _httpContextAccessor.HttpContext;

            //  Get UserId from JWT claims
            var userIdClaim = httpContext?.User.FindFirstValue("userId");
            int.TryParse(userIdClaim, out var userId);

            //  Get IP address (real client IP if behind proxy)
            var ipAddress = httpContext?.Request.Headers["X-Forwarded-For"].FirstOrDefault()
                            ?? httpContext?.Connection.RemoteIpAddress?.ToString();

            var log = new AuditLog
            {
                UserId = userId != 0 ? userId : dto.UserId, // fallback to dto
                Action = dto.Action,
                EntityName = dto.EntityName,
                EntityId = dto.EntityId,
                OldValueJson = dto.OldValueJson,
                NewValueJson = dto.NewValueJson,
                IpAddress = ipAddress ?? dto.IpAddress,
                Timestamp = DateTimeOffset.UtcNow
            };

            _repo.Add(log);
            return MapToDto(log);
        }

        public IEnumerable<AuditLogDto> GetByEntity(string entityName, int entityId)
        {
            return _repo.GetByEntity(entityName, entityId).Select(MapToDto);
        }

        public IEnumerable<AuditLogDto> GetByUser(int userId)
        {
            return _repo.GetByUser(userId).Select(MapToDto);
        }

        // new method
        public IEnumerable<AuditLogDto> GetAll()
        {
            return _repo.GetAll().Select(MapToDto);
        }

        private AuditLogDto MapToDto(AuditLog a) =>
            new AuditLogDto
            {
                Id = a.Id,
                UserId = a.UserId,
                Action = a.Action,
                EntityName = a.EntityName,
                EntityId = a.EntityId,
                OldValueJson = a.OldValueJson,
                NewValueJson = a.NewValueJson,
                Timestamp = a.Timestamp,
                IpAddress = a.IpAddress
            };
    }
}
