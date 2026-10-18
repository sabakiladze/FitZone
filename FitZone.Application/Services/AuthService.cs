using FitZone.Application.DTOs.Auth;
using FitZone.Application.ServiceInterfaces;
using FitZone.Domain.CustomExceptions;
using FitZone.Domain.Entities.Models;
using FitZone.Domain.Interfaces;


namespace FitZone.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IUnitOfWork _unitOfWork;
        public AuthService(IUserRepository userRepository, IRoleRepository roleRepository, IPasswordHasher passwordHasher, IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _passwordHasher = passwordHasher;
            _unitOfWork=unitOfWork;
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
            ArgumentNullException.ThrowIfNull(request);// ეს კარგი რაღაცაა დაიმახსოვრე



            bool user_email = await _userRepository.ExistsByEmailAsync(request.Email.Trim().ToLowerInvariant(), ct);
            if (user_email == true) throw new EmailIsAlreadyRegisteredException();

            bool user_username = await _userRepository.ExistsByEmailAsync(request.UserName, ct);
            if (user_username == true) throw new UserNameIsAlreadyTaken();


            var role = await _roleRepository.GetByNameAsync("User", ct) ?? throw new RoleNotFoundException();


            var newUser = new User {
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
