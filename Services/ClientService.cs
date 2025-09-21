using BankingPaymentsAPI.DTOs;
using BankingPaymentsAPI.Models;
using BankingPaymentsAPI.Repository;
using BankingPaymentsAPI.Services.PaymentProcessing;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Stripe;

namespace BankingPaymentsAPI.Services
{
    public class ClientService : IClientService
    {
        private readonly IClientRepository _clientRepo;
        private readonly IAuditService _audit;
        private readonly IHttpContextAccessor _httpContext;
        private readonly IStripePaymentService _stripeService;

        public ClientService(
            IClientRepository clientRepo,
            IAuditService audit,
            IHttpContextAccessor httpContext,
            IStripePaymentService stripeService)
        {
            _clientRepo = clientRepo;
            _audit = audit;
            _httpContext = httpContext;
            _stripeService = stripeService;
        }

        #region CRUD Methods

        public async Task<ClientDto> CreateClientAsync(ClientRequestDto request, int createdByUserId)
        {
            var client = new Client
            {
                BankId = request.BankId,
                ClientCode = request.ClientCode,
                Name = request.Name,
                ContactEmail = request.ContactEmail,
                ContactPhone = request.ContactPhone,
                Balance = 0m,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = createdByUserId
            };

            await _clientRepo.AddAsync(client);

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

        public async Task<ClientDto?> GetClientByIdAsync(int id)
        {
            var client = await _clientRepo.GetByIdAsync(id);
            return client == null ? null : MapToDto(client);
        }

        public async Task<IEnumerable<ClientDto>> GetAllClientsAsync()
        {
            var clients = await _clientRepo.GetAllAsync();
            return clients.Select(MapToDto);
        }

        public async Task<ClientDto?> UpdateClientAsync(int id, ClientUpdateDto request)
        {
            var client = await _clientRepo.GetByIdAsync(id);
            if (client == null) return null;

            var oldValue = JsonSerializer.Serialize(client);

            client.Name = request.Name;
            client.ContactEmail = request.ContactEmail;
            client.ContactPhone = request.ContactPhone;

            await _clientRepo.UpdateAsync(client);

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

        public async Task<bool> DeleteClientAsync(int id)
        {
            var client = await _clientRepo.GetByIdAsync(id);
            if (client == null) return false;

            var oldValue = JsonSerializer.Serialize(client);
            await _clientRepo.DeleteAsync(client);

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

        #endregion

        #region Internal & Stripe Top-up

        public async Task<bool> AddMoneyAsync(int clientId, decimal amount)
        {
            if (amount <= 0) return false;

            var client = await _clientRepo.GetByIdAsync(clientId);
            if (client == null) return false;

            client.Balance += amount;
            await _clientRepo.UpdateAsync(client);

            return true;
        }

        public async Task<PaymentIntent> TopUpViaStripeAsync(int clientId, decimal amount)
        {
            var client = await _clientRepo.GetByIdAsync(clientId);
            if (client == null) return null;

            var receipt = $"Client-{clientId}-TopUp-{DateTimeOffset.UtcNow.Ticks}";
            var paymentIntent = _stripeService.CreatePaymentIntent(amount, "INR", receipt);

            client.StripePaymentIntentId = paymentIntent.Id;
            await _clientRepo.UpdateAsync(client);

            return paymentIntent;
        }

        public async Task<bool> ConfirmStripeTopUpAsync(string paymentIntentId)
        {
            var client = await _clientRepo.GetByStripeIdAsync(paymentIntentId);
            if (client == null) return false;

            var intent = _stripeService.GetPaymentIntent(paymentIntentId);
            if (intent.Status == "succeeded")
            {
                client.Balance += intent.Amount / 100m;
                client.StripePaymentIntentId = paymentIntentId;
                await _clientRepo.UpdateAsync(client);
                return true;
            }

            return false;
        }

        #endregion

        #region Helpers

        private ClientDto MapToDto(Client client)
        {
            return new ClientDto
            {
                Id = client.Id,
                BankId = client.BankId,
                BankName = client.Bank?.BankName ?? "Unknown",
                ClientCode = client.ClientCode,
                Name = client.Name,
                ContactEmail = client.ContactEmail,
                ContactPhone = client.ContactPhone,
                OnboardingStatus = client.OnboardingStatus.ToString(),
                IsVerified = client.IsVerified,
                Balance = client.Balance
            };
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = _httpContext.HttpContext?.User?.FindFirst("userId")?.Value;
            return int.TryParse(userIdClaim, out var id) ? id : 0;
        }

        private string GetClientIp()
        {
            return _httpContext.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "unknown";
        }

        public async Task<ClientDto?> GetClientByStripePaymentIntentIdAsync(string paymentIntentId)
        {
            var client = await _clientRepo.GetByStripePaymentIntentIdAsync(paymentIntentId);
            return client == null ? null : MapToDto(client);
        }

        #endregion
    }
}
