using BankingPaymentsAPI.Data;
using BankingPaymentsAPI.DTOs;
using BankingPaymentsAPI.Models;
using BankingPaymentsAPI.Repository;
using BankingPaymentsAPI.Helpers.BankingPaymentsAPI.Helpers;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace BankingPaymentsAPI.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IRefreshTokenRepository _refreshRepo;
        private readonly IUserRepository _userRepo; 

        // In prod, inject settings and proper key management
        private readonly string _jwtSecret = "change_this_to_strong_secret";
        private readonly TimeSpan _accessTokenLifetime = TimeSpan.FromMinutes(30);
        private readonly TimeSpan _refreshTokenLifetime = TimeSpan.FromDays(7);

        public AuthService(AppDbContext context, IRefreshTokenRepository refreshRepo, IUserRepository userRepo)
        {
            _context = context;
            _refreshRepo = refreshRepo;
            _userRepo = userRepo;
        }

        public AuthResponseDto Login(LoginRequestDto dto)
        {
            var user = _userRepo.GetByUsernameOrEmail(dto.UsernameOrEmail);
            if (user == null) throw new Exception("Invalid credentials");
           
            if (!PasswordHelper.Verify(dto.Password, user.PasswordHash)) throw new Exception("Invalid credentials");

            user.LastLogin = DateTimeOffset.UtcNow;
            _userRepo.Update(user);

            var accessToken = GenerateJwt(user);
            var refreshToken = CreateRefreshToken(user);

            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token,
                ExpiresAt = DateTimeOffset.UtcNow.Add(_accessTokenLifetime)
            };
        }

        public AuthResponseDto RefreshToken(RefreshRequestDto dto)
        {
            var stored = _refreshRepo.GetByToken(dto.RefreshToken);
            if (stored == null || stored.IsRevoked || stored.ExpiresAt < DateTimeOffset.UtcNow) throw new Exception("Invalid refresh token");

            var user = _userRepo.GetById(stored.UserId);
            if (user == null) throw new Exception("Invalid token user");

            // optionally revoke old token, create new one
            stored.IsRevoked = true;
            _refreshRepo.Update(stored);

            var access = GenerateJwt(user);
            var rt = CreateRefreshToken(user);

            return new AuthResponseDto
            {
                AccessToken = access,
                RefreshToken = rt.Token,
                ExpiresAt = DateTimeOffset.UtcNow.Add(_accessTokenLifetime)
            };
        }

        public void Logout(LogoutRequestDto dto)
        {
            var stored = _refreshRepo.GetByToken(dto.RefreshToken);
            if (stored == null) return;
            stored.IsRevoked = true;
            _refreshRepo.Update(stored);
        }

        private string GenerateJwt(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                new Claim("userId", user.Id.ToString()),
                new Claim("role", user.Role.ToString())
            };

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.Add(_accessTokenLifetime),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private RefreshToken CreateRefreshToken(User user)
        {
            var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray()) + Guid.NewGuid().ToString("N");
            var rt = new RefreshToken
            {
                UserId = user.Id,
                Token = token,
                ExpiresAt = DateTimeOffset.UtcNow.Add(_refreshTokenLifetime),
                IsRevoked = false
            };
            _refreshRepo.Add(rt);
            return rt;
        }
    }
}
