using FitZone.Application.DTOs.Auth;
using FitZone.Application.ServiceInterfaces;
using FitZone.Domain.Entities.Models;
using FitZone.Domain.Interfaces;


namespace FitZone.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        public AuthService(IUserRepository userRepository, IRoleRepository roleRepository)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
        }
        public Task<bool> DeleteUserAsync(Guid userId, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<AuthResponseDTO> LoginAsync(LoginRequestDTO request, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task LogOutAsync(string token, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<AuthResponseDTO> RefreshTokenAsync(string token, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public async Task<AuthResponseDTO> RegisterAsync(RegisterRequestDTO request, CancellationToken ct = default)
        {
            if (request is null) throw new ArgumentNullException(nameof(request));

            var role = await _roleRepository.GetByNameAsync("User", ct);
            if (role == null) throw new Exception(""); // ეს არასწორი მესიჯი იქნება იმიტომ რომ მომხარებელს რაში აინტერესებს როლი რომ არ მოიძებნა მაგრამ რა დავწერო??

                var newUser = new User {
                UserName = request.UserName,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                PasswordHash = ....,
                RoleId = role.Id,
                Role = role,
                IsActive = true,
                IsEmailVerified = false,
                VerificationCode = null,
                VerificationCodeExpiresAt = null,
                CreatedAt = DateTime.Now,
                LastLoginAt =null
            };
            await _userRepository.AddAsync(newUser, ct);
            return new AuthResponseDTO
            {

            };
        }

        public Task<bool> VerifyEmailAsync(string email, string code, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }
    }
}
