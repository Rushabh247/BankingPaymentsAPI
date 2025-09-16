using BankingPaymentsAPI.DTOs;
using BankingPaymentsAPI.Enums;

using BankingPaymentsAPI.Helpers.BankingPaymentsAPI.Helpers;
using BankingPaymentsAPI.Models;
using BankingPaymentsAPI.Repository;

namespace BankingPaymentsAPI.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repo;

        public UserService(IUserRepository repo)
        {
            _repo = repo;
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

            return MapToDto(user);
        }

       

      
        public UserDto? UpdateUser(int id, UpdateUserDto dto)
        {
            var user = _repo.GetById(id);
            if (user == null) return null;

            user.Username = dto.Username ?? user.Username;
            user.Email = dto.Email ?? user.Email;

            if (!string.IsNullOrEmpty(dto.Password))
                user.PasswordHash = PasswordHelper.Hash(dto.Password);

            if (!string.IsNullOrEmpty(dto.Role))
                user.Role = Enum.Parse<UserRole>(dto.Role, true);

            _repo.Update(user);

            return MapToDto(user);
        }

      
        public bool SoftDeleteUser(int id)
        {
            var user = _repo.GetById(id);
            if (user == null) return false;

            user.IsActive = false;
            _repo.Update(user);
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
    }
}
