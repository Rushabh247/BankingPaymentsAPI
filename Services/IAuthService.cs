using BankingPaymentsAPI.DTOs;

namespace BankingPaymentsAPI.Services
{
    public interface IAuthService
    {
        AuthResponseDto Login(LoginRequestDto dto);
        AuthResponseDto RefreshToken(RefreshRequestDto dto);
        void Logout(LogoutRequestDto dto);
    }
}
