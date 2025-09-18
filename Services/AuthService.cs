using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BankingPaymentsAPI.Data;
using BankingPaymentsAPI.DTOs;
using BankingPaymentsAPI.Helpers; // <-- include PasswordHelper
using BankingPaymentsAPI.Helpers.BankingPaymentsAPI.Helpers;
using BankingPaymentsAPI.Models;
using BankingPaymentsAPI.Repository;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace BankingPaymentsAPI.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IRefreshTokenRepository _refreshTokenRepo;
        private readonly IConfiguration _config;

        public AuthService(AppDbContext context, IRefreshTokenRepository refreshTokenRepo, IConfiguration config)
        {
            _context = context;
            _refreshTokenRepo = refreshTokenRepo;
            _config = config;
        }

        // ✅ Login
        public async Task<AuthResponseDto> Login(LoginRequestDto dto)
        {
            // 1. Find user by email/username
            var user = _context.Users.FirstOrDefault(u => u.Email == dto.UsernameOrEmail);
            if (user == null) throw new UnauthorizedAccessException("Invalid credentials");

            // 2. Verify password against stored SHA256 hash
            if (!PasswordHelper.Verify(dto.Password, user.PasswordHash))
                throw new UnauthorizedAccessException("Invalid credentials");

            // 3. Generate access token
            var accessToken = GenerateJwtToken(user);

            // 4. Generate refresh token
            var refreshToken = new RefreshToken
            {
                UserId = user.Id,
                Token = Guid.NewGuid().ToString(),
                ExpiresAt = DateTimeOffset.UtcNow.AddDays(7),
                IsRevoked = false
            };
            await _refreshTokenRepo.AddAsync(refreshToken);

            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token,
                ExpiresAt = DateTime.UtcNow.AddMinutes(30)
            };
        }

        // ✅ Refresh Token
        public async Task<AuthResponseDto> RefreshToken(RefreshRequestDto dto)
        {
            var storedToken = await _refreshTokenRepo.GetByTokenAsync(dto.RefreshToken);
            if (storedToken == null || storedToken.IsRevoked || storedToken.ExpiresAt < DateTimeOffset.UtcNow)
                throw new UnauthorizedAccessException("Invalid refresh token");

            var user = await _context.Users.FindAsync(storedToken.UserId);
            if (user == null) throw new UnauthorizedAccessException("User not found");

            // Issue new access token
            var newAccessToken = GenerateJwtToken(user);

            return new AuthResponseDto
            {
                AccessToken = newAccessToken,
                RefreshToken = dto.RefreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(30)
            };
        }

        // ✅ Logout
        public async Task Logout(LogoutRequestDto dto)
        {
            var storedToken = await _refreshTokenRepo.GetByTokenAsync(dto.RefreshToken);
            if (storedToken != null)
            {
                storedToken.IsRevoked = true;
                await _refreshTokenRepo.UpdateAsync(storedToken);
            }
        }

        // 🔑 Generate JWT
        private string GenerateJwtToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(30),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
