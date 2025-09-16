using BankingPaymentsAPI.DTOs;

namespace BankingPaymentsAPI.Services
{
    public interface IUserService
    {
       
        UserDto Register(RegisterUserDto dto);
        UserDto? GetById(int id);
        UserDto? GetByUsernameOrEmail(string usernameOrEmail);

       
        UserDto? UpdateUser(int id, UpdateUserDto dto);
        bool SoftDeleteUser(int id);
    }
}
