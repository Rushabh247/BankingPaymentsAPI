using BankingPaymentsAPI.DTOs;

namespace BankingPaymentsAPI.Services
{
    public interface IClientService
    {
        ClientDto CreateClient(ClientRequestDto request, int createdByUserId);
        ClientDto? GetClientById(int id);
        IEnumerable<ClientDto> GetAllClients();
        ClientDto? UpdateClient(int id, ClientUpdateDto request);
        bool DeleteClient(int id);
    }
}
