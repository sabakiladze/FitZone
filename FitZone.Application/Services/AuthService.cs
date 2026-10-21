using FitZone.Application.DTOs.Auth;
using FitZone.Application.DTOs.AuthDTOs;
using FitZone.Application.Helpers;
using FitZone.Application.ServiceInterfaces;
using FitZone.Domain.CustomExceptions;
using FitZone.Domain.Entities.Models;
using FitZone.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Text;

namespace FitZone.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AuthService> _logger;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IEmailSender _emailSender;



        public AuthService(IUserRepository userRepository,
            IRoleRepository roleRepository,
            IPasswordHasher passwordHasher,
            IUnitOfWork unitOfWork,
            ILogger<AuthService> logger,
            IJwtTokenService jwtTokenService,
            IEmailSender emailsender
            )
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _passwordHasher = passwordHasher;
            _unitOfWork = unitOfWork;
            _jwtTokenService = jwtTokenService;
            _logger = logger;
            _emailSender = emailsender;
        }

        public async Task DeleteUserAsync(Guid userId, DeleteAccountRequestDTO request, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(request);
            var user = await _userRepository.GetByIdAsync(userId, ct);
            if (user is null || user.DeletedAt is not null)
                throw new UserNotFoundException();

            if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
                throw new System.Security.Authentication.InvalidCredentialException();

            user.MarkAsDeleted();

            // TODO: მომავალი ჩაწერების გაუქმება (Booking-ის ლოგიკის შემდეგ)

            await _unitOfWork.SaveChangesAsync(ct);

            _logger.LogInformation("User {UserId} deleted account.", userId);

        }

        public async Task<AuthResponseDTO> LoginAsync(LoginRequestDTO request, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            var email = request.Email.Trim().ToLowerInvariant();

            var user = await _userRepository.GetByEmailAsync(email, ct);
            
            if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
                throw new EmailOrPasswordIsIncorrectException();

            if(user.IsEmailVerified ==false) throw new EmailIsNotVerifiedException();

            if (!user.IsActive)
                throw new AccountIsDeactivatedException();

            var (accessToken, accessExpiresAt) = _jwtTokenService.GenerateAccessToken(user);


            var(refreshToken, tokenHash,refreshexpiresAt)= _jwtTokenService.GenerateRefreshToken();

            user.SetRefreshToken(tokenHash, refreshexpiresAt);

            await _unitOfWork.SaveChangesAsync(ct);

            return new AuthResponseDTO {
                UserId = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Role = user.Role.RoleName,
                AccessToken = accessToken,
                AccessTokenExpiresAt = accessExpiresAt,
                RefreshToken = refreshToken
            };  
        }

        public async Task LogOutAsync(string hashToken, CancellationToken ct = default)
        {
            var user=await _userRepository.GetUserByHashedToken(hashToken, ct);
            user?.ClearRefreshToken();
            await _unitOfWork.SaveChangesAsync(ct);
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
                Email = email,
                PasswordHash = _passwordHasher.Hash(request.Password),
                RoleId = role.Id,
                
            }; 

            const int codeLifespanMinutes = 15;
            var code = VerificationCodeHelper.Generate();
            var hash = VerificationCodeHelper.Hash(code);

            newUser.SetVerificationCode(hash,
                DateTime.UtcNow.AddMinutes(codeLifespanMinutes));


            await _userRepository.AddAsync(newUser, ct);
            await _unitOfWork.SaveChangesAsync(ct);


            var body = VerificationEmailTemplate.GenerateVerificationBody(newUser.FirstName, code, codeLifespanMinutes);
            await _emailSender.SendAsync(newUser.Email, "Account Verification", body, ct); 

            _logger.LogInformation("User {UserId} registered.", newUser.Id);


            return new RegisterResponseDTO { UserId = newUser.Id, Email = newUser.Email };

        }

        public async Task<bool> VerifyEmailAsync(VerificationEmailRequestDTO request, CancellationToken ct = default)
        {

            ArgumentNullException.ThrowIfNull(request);

            var email = request.Email.Trim().ToLowerInvariant();
            var user = await _userRepository.GetByEmailAsync(email, ct);

             if (user is null || user.IsEmailVerified == true || user.VerificationCode is null)
                    throw new InvalidVerificationCodeException();

            if (user.VerificationCodeExpiresAt is null || user.VerificationCodeExpiresAt < DateTime.UtcNow)
                throw new VerificationCodeExpiredException();

            var provided = Encoding.UTF8.GetBytes(VerificationCodeHelper.Hash(request.Code));
            var stored = Encoding.UTF8.GetBytes(user.VerificationCode!);

            if (!CryptographicOperations.FixedTimeEquals(provided, stored))
                throw new InvalidVerificationCodeException();


            user.VerifyEmail();
            await _unitOfWork.SaveChangesAsync(ct);
            _logger.LogInformation("User {UserId} verified email.", user.Id);

            return true;


        }
    }
}
