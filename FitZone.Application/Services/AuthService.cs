using FitZone.Application.DTOs.Auth;
using FitZone.Application.DTOs.AuthDTOs;
using FitZone.Application.ServiceInterfaces;
using FitZone.Domain.CustomExceptions;
using FitZone.Domain.Entities.Models;
using FitZone.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System.Security.Authentication;


namespace FitZone.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AuthService> _logger;


        public AuthService(IUserRepository userRepository, IRoleRepository roleRepository, IPasswordHasher passwordHasher, IUnitOfWork unitOfWork, ILogger<AuthService> logger)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _passwordHasher = passwordHasher;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public Task<bool> DeleteUserAsync(Guid userId, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public async Task<AuthResponseDTO> LoginAsync(LoginRequestDTO request, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            
            var user = await _userRepository.GetByEmailAsync(request.Email, ct);
            if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
                throw new Exception("Email or password is incorrect"); //?????????

            return new AuthResponseDTO { };  // ამას დავამატო
        }

        public Task LogOutAsync(string token, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<AuthResponseDTO> RefreshTokenAsync(string token, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public async Task<RegisterResponseDTO> RegisterAsync(RegisterRequestDTO request, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(request);// ეს კარგი რაღაცაა დაიმახსოვრე


            var email = request.Email.Trim().ToLowerInvariant();

            bool user_email = await _userRepository.ExistsByEmailAsync(email, ct);
            if (user_email == true) throw new EmailIsAlreadyRegisteredException();

            bool user_username = await _userRepository.ExistsByUserNameAsync(request.UserName, ct);
            if (user_username == true) throw new UserNameIsAlreadyTaken();


            var role = await _roleRepository.GetByNameAsync("User", ct);

            if (role is null)
            {
                _logger.LogError("Default role 'User' was not found.");
                throw new RoleNotFoundException();
            }


            var newUser = new User
            {
                UserName = request.UserName,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                PasswordHash = _passwordHasher.Hash(request.Password),
                RoleId = role.Id,
                IsActive = true,
                IsEmailVerified = false,
                VerificationCode = null,
                VerificationCodeExpiresAt = null,
                CreatedAt = DateTime.Now,
                LastLoginAt = null
            };


            await _userRepository.AddAsync(newUser, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            _logger.LogInformation("User {UserId} registered.", newUser.Id);

            return new RegisterResponseDTO { UserId = newUser.Id, Email = newUser.Email };

        }

        public async Task<bool> VerifyEmailAsync(string email, string code, CancellationToken ct = default)
        {


            if(string.IsNullOrEmpty(email)) { throw new ArgumentException("Email filed is empty");}
            if (string.IsNullOrEmpty(code)) { throw new ArgumentException("Code is empty");}

            var email_check = email.Trim().ToLowerInvariant();
            var user = await _userRepository.GetByEmailAsync(email_check, ct) ?? throw new ArgumentException("User by this email could not found");

           
            if(user.VerificationCode==code)
            {
                user.VerifyEmail();
            }
            return true;


        }
    }
}
