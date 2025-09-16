using BankingPaymentsAPI.DTOs;
using BankingPaymentsAPI.Models;
using BankingPaymentsAPI.Repository;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace BankingPaymentsAPI.Services
{
    public class ClientService : IClientService
    {
        private readonly IClientRepository _clientRepo;
        private readonly IAuditService _audit;
        private readonly IHttpContextAccessor _httpContext;

        public ClientService(
            IClientRepository clientRepo,
            IAuditService audit,
            IHttpContextAccessor httpContext)
        {
            _clientRepo = clientRepo;
            _audit = audit;
            _httpContext = httpContext;
        }

        public ClientDto CreateClient(ClientRequestDto request, int createdByUserId)
        {
            var client = new Client
            {
                BankId = request.BankId,
                ClientCode = request.ClientCode,
                Name = request.Name,
                ContactEmail = request.ContactEmail,
                ContactPhone = request.ContactPhone,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = createdByUserId
            };

            _clientRepo.Add(client);

            // Log CREATE
            _audit.Log(new CreateAuditLogDto
            {
                UserId = GetCurrentUserId(),
                Action = "CREATE",
                EntityName = nameof(Client),
                EntityId = client.Id,
                OldValueJson = null,
                NewValueJson = JsonSerializer.Serialize(client),
                IpAddress = GetClientIp()
            });

            return MapToDto(client);
        }

        public ClientDto? GetClientById(int id)
        {
            var client = _clientRepo.GetById(id);
            return client == null ? null : MapToDto(client);
        }

        public IEnumerable<ClientDto> GetAllClients()
        {
            return _clientRepo.GetAll().Select(MapToDto);
        }

        public ClientDto? UpdateClient(int id, ClientUpdateDto request)
        {
            var client = _clientRepo.GetById(id);
            if (client == null) return null;

            var oldValue = JsonSerializer.Serialize(client);

            client.Name = request.Name;
            client.ContactEmail = request.ContactEmail;
            client.ContactPhone = request.ContactPhone;

            _clientRepo.Update(client);

            //  Log UPDATE
            _audit.Log(new CreateAuditLogDto
            {
                UserId = GetCurrentUserId(),
                Action = "UPDATE",
                EntityName = nameof(Client),
                EntityId = client.Id,
                OldValueJson = oldValue,
                NewValueJson = JsonSerializer.Serialize(client),
                IpAddress = GetClientIp()
            });

            return MapToDto(client);
        }

        public bool DeleteClient(int id)
        {
            var client = _clientRepo.GetById(id);
            if (client == null) return false;

            var oldValue = JsonSerializer.Serialize(client);

            _clientRepo.Delete(client);

            //  Log DELETE
            _audit.Log(new CreateAuditLogDto
            {
                UserId = GetCurrentUserId(),
                Action = "DELETE",
                EntityName = nameof(Client),
                EntityId = client.Id,
                OldValueJson = oldValue,
                NewValueJson = null,
                IpAddress = GetClientIp()
            });

            return true;
        }

        private ClientDto MapToDto(Client client)
        {
            return new ClientDto
            {
                Id = client.Id,
                BankName = client.Bank?.BankName ?? "Unknown",
                Name = client.Name,
                ClientCode = client.ClientCode,
                ContactEmail = client.ContactEmail,
                ContactPhone = client.ContactPhone,
                OnboardingStatus = client.OnboardingStatus.ToString(),
                IsVerified = client.IsVerified
            };
        }

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
