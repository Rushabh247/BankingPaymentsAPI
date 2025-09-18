using BankingPaymentsAPI.DTOs;

public interface IAuthService
{
    Task<AuthResponseDto> Login(LoginRequestDto dto);
    Task<AuthResponseDto> RefreshToken(RefreshRequestDto dto);
    Task Logout(LogoutRequestDto dto);
}
