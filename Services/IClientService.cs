using BankingPaymentsAPI.DTOs;
using Stripe;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IClientService
{
    Task<ClientDto> CreateClientAsync(ClientRequestDto request, int createdByUserId);
    Task<ClientDto?> GetClientByIdAsync(int id);
    Task<IEnumerable<ClientDto>> GetAllClientsAsync();
    Task<ClientDto?> UpdateClientAsync(int id, ClientUpdateDto request);
    Task<bool> DeleteClientAsync(int id);

    Task<bool> AddMoneyAsync(int clientId, decimal amount);
    Task<PaymentIntent> TopUpViaStripeAsync(int clientId, decimal amount);
    Task<bool> ConfirmStripeTopUpAsync(string paymentIntentId);
    Task<ClientDto?> GetClientByStripePaymentIntentIdAsync(string paymentIntentId);
}
