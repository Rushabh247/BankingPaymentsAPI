using BankingPaymentsAPI.DTOs;
using BankingPaymentsAPI.Models;
using BankingPaymentsAPI.Repository;


namespace BankingPaymentsAPI.Services
{
    public class ClientService : IClientService
    {
        private readonly IClientRepository _clientRepo;

        public ClientService(IClientRepository clientRepo)
        {
            _clientRepo = clientRepo;
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

            client.Name = request.Name;
            client.ContactEmail = request.ContactEmail;
            client.ContactPhone = request.ContactPhone;

            _clientRepo.Update(client);
            return MapToDto(client);
        }

        public bool DeleteClient(int id)
        {
            var client = _clientRepo.GetById(id);
            if (client == null) return false;

            _clientRepo.Delete(client);
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
    }
}
