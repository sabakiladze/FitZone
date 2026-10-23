using FitZone.Application.DTOs.Auth;
using FitZone.Application.DTOs.AuthDTOs;
using FitZone.Application.Helpers;
using FitZone.Application.Implementations;
using FitZone.Application.ServiceInterfaces;
using FitZone.Domain.CustomExceptions;
using FitZone.Domain.Entities.Models;
using FitZone.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Testing.ServicesTesting
{
    public class AuthSericeTest
    {
        private readonly Mock<IUserRepository> _userRepository = new();
        private readonly Mock<IRoleRepository> _roleRepository = new();
        private readonly Mock<IPasswordHasher> _passwordHasher = new();
        private readonly Mock<IUnitOfWork> _unitOfWork = new();
        private readonly Mock<ILogger<AuthService>> _logger = new();
        private readonly Mock<IJwtTokenService> _jwtTokenService = new();
        private readonly Mock<IEmailSender> _emailSender = new();

        private readonly AuthService _sut;

        public AuthSericeTest()
        {
            _sut = new AuthService(
                _userRepository.Object,
                _roleRepository.Object,
                _passwordHasher.Object,
                _unitOfWork.Object,
                _logger.Object,
                _jwtTokenService.Object,
                _emailSender.Object);
        }
        private static User CreateUser()
        {
            return new User
            {
                Id = Guid.NewGuid(),
                UserName = "testuser",
                FirstName = "Test",
                LastName = "User",
                Email = "test@example.com",
                PasswordHash = "hashed-password",
                //IsEmailVerified = true,
                IsActive = true,
                Role = new Role
                {
                    Id = Guid.NewGuid(),
                    RoleName = "User"
                }
            };
        }
        [Fact]
        public async Task LoginAsync_ValidCredentials_ReturnsAuthResponse()
        {
            // Arrange
            var user = CreateUser();
            _userRepository.Setup(r => r.GetByEmailAsync(
           "test@example.com",
           It.IsAny<CancellationToken>())).ReturnsAsync(user);

            _passwordHasher
                .Setup(p => p.Verify("correct-password", user.PasswordHash))
                .Returns(true);

            var accessExpiry = DateTime.UtcNow.AddMinutes(15);
            var refreshExpiry = DateTime.UtcNow.AddDays(7);

            _jwtTokenService
                .Setup(j => j.GenerateAccessToken(user))
                .Returns(("access-token", accessExpiry));

            _jwtTokenService
                .Setup(j => j.GenerateRefreshToken())
                .Returns(("refresh-token", "refresh-hash", refreshExpiry));

            var request = new LoginRequestDTO
            {
                Email = " TEST@EXAMPLE.COM ",
                Password = "correct-password"
            };

            // Act
            var result = await _sut.LoginAsync(request);

            // Assert
            Assert.Equal(user.Id, result.UserId);
            Assert.Equal("testuser", result.UserName);
            Assert.Equal("test@example.com", result.Email);
            Assert.Equal("User", result.Role);
            Assert.Equal("access-token", result.AccessToken);
            Assert.Equal("refresh-token", result.RefreshToken);
            Assert.Equal(accessExpiry, result.AccessTokenExpiresAt);

            _unitOfWork.Verify(
                u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task LoginAsync_WrongPassword_ThrowsEmailOrPasswordException()
        {
            // Arrange
            var user = CreateUser();

            _userRepository
                .Setup(r => r.GetByEmailAsync(
                    user.Email,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            _passwordHasher
                .Setup(p => p.Verify("wrong-password", user.PasswordHash))
                .Returns(false);

            var request = new LoginRequestDTO
            {
                Email = user.Email,
                Password = "wrong-password"
            };

            // Act & Assert
            await Assert.ThrowsAsync<EmailOrPasswordIsIncorrectException>(
                () => _sut.LoginAsync(request));

            _unitOfWork.Verify(
                u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task LoginAsync_UserNotFound_ThrowsEmailOrPasswordException()
        {
            _userRepository
                .Setup(r => r.GetByEmailAsync(
                    "missing@example.com",
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);

            var request = new LoginRequestDTO
            {
                Email = "missing@example.com",
                Password = "some-password"
            };

            await Assert.ThrowsAsync<EmailOrPasswordIsIncorrectException>(
                () => _sut.LoginAsync(request));
        }

        [Fact]
        public async Task LoginAsync_EmailNotVerified_ThrowsException()
        {
            var user = CreateUser();
            //user.IsEmailVerified = false;

            _userRepository
                .Setup(r => r.GetByEmailAsync(
                    user.Email,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            _passwordHasher
                .Setup(p => p.Verify("correct-password", user.PasswordHash))
                .Returns(true);

            var request = new LoginRequestDTO
            {
                Email = user.Email,
                Password = "correct-password"
            };

            await Assert.ThrowsAsync<EmailIsNotVerifiedException>(
                () => _sut.LoginAsync(request));
        }

        [Fact]
        public async Task LoginAsync_InactiveUser_ThrowsException()
        {
            var user = CreateUser();
            user.IsActive = false;

            _userRepository
                .Setup(r => r.GetByEmailAsync(
                    user.Email,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            _passwordHasher
                .Setup(p => p.Verify("correct-password", user.PasswordHash))
                .Returns(true);

            var request = new LoginRequestDTO
            {
                Email = user.Email,
                Password = "correct-password"
            };

            await Assert.ThrowsAsync<AccountIsDeactivatedException>(
                () => _sut.LoginAsync(request));
        }

        [Fact]
        public async Task RegisterAsync_ValidRequest_CreatesUser()
        {
            // Arrange
            var role = new Role
            {
                Id = Guid.NewGuid(),
                RoleName = "User"
            };

            _userRepository
                .Setup(r => r.ExistsByEmailAsync(
                    "test@example.com",
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _userRepository
                .Setup(r => r.ExistsByUserNameAsync(
                    "testuser",
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _roleRepository
                .Setup(r => r.GetByNameAsync(
                    "User",
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(role);

            _passwordHasher
                .Setup(p => p.Hash("password123"))
                .Returns("hashed-password");

            User? savedUser = null;

            _userRepository
                .Setup(r => r.AddAsync(
                    It.IsAny<User>(),
                    It.IsAny<CancellationToken>()))
                .Callback<User, CancellationToken>((u, _) => savedUser = u)
                .Returns((User u, CancellationToken _) => Task.FromResult(u));

            var request = new RegisterRequestDTO
            {
                UserName = "testuser",
                FirstName = "Test",
                LastName = "User",
                Email = " TEST@EXAMPLE.COM ",
                Password = "password123"
            };

            // Act
            var result = await _sut.RegisterAsync(request);

            // Assert
            Assert.NotNull(savedUser);
            Assert.Equal("test@example.com", savedUser.Email);
            Assert.Equal("hashed-password", savedUser.PasswordHash);
            Assert.Equal(role.Id, savedUser.RoleId);

            Assert.Equal(savedUser.Id, result.UserId);
            Assert.Equal(savedUser.Email, result.Email);

            _userRepository.Setup(r => r.AddAsync(
             It.IsAny<User>(),
             It.IsAny<CancellationToken>()))
            .Callback<User, CancellationToken>((u, _) => savedUser = u)
.ReturnsAsync((User u, CancellationToken _) => u);
            _unitOfWork.Verify(
                u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_EmailAlreadyExists_ThrowsException()
        {
            _userRepository
                .Setup(r => r.ExistsByEmailAsync(
                    "test@example.com",
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var request = new RegisterRequestDTO
            {
                UserName = "testuser",
                FirstName = "Test",
                LastName = "User",
                Email = "test@example.com",
                Password = "password123"
            };

            await Assert.ThrowsAsync<EmailIsAlreadyRegisteredException>(
                () => _sut.RegisterAsync(request));

            _userRepository.Verify(
                r => r.AddAsync(
                    It.IsAny<User>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task VerifyEmailAsync_ValidCode_VerifiesEmail()
        {
            // Arrange
            var user = CreateUser();

            var code = "123456";
            var codeHash = VerificationCodeHelper.Hash(code);

            user.SetVerificationCode(
                codeHash,
                DateTime.UtcNow.AddMinutes(10));

            //user.IsEmailVerified = false;

            _userRepository
                .Setup(r => r.GetByEmailAsync(
                    user.Email,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            var request = new VerificationEmailRequestDTO
            {
                Email = user.Email,
                Code = code
            };

            // Act
            var result = await _sut.VerifyEmailAsync(request);

            // Assert
            Assert.True(result);
            Assert.True(user.IsEmailVerified);

            _unitOfWork.Verify(
                u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task VerifyEmailAsync_WrongCode_ThrowsException()
        {
            var user = CreateUser();

            //user.IsEmailVerified = false;
            user.SetVerificationCode(
                VerificationCodeHelper.Hash("123456"),
                DateTime.UtcNow.AddMinutes(10));

            _userRepository
                .Setup(r => r.GetByEmailAsync(
                    user.Email,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            var request = new VerificationEmailRequestDTO
            {
                Email = user.Email,
                Code = "999999"
            };

            await Assert.ThrowsAsync<InvalidVerificationCodeException>(
                () => _sut.VerifyEmailAsync(request));

            _unitOfWork.Verify(
                u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task RefreshTokenAsync_ValidToken_RotatesRefreshToken()
        {
            // Arrange
            var user = CreateUser();

            var oldRawToken = "old-refresh-token";
            var oldHash = RefreshTokenHasher.Hash(oldRawToken);

            user.SetRefreshToken(
                oldHash,
                DateTime.UtcNow.AddDays(5));

            _userRepository
                .Setup(r => r.GetUserByHashedToken(
                    oldHash,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            var newExpiry = DateTime.UtcNow.AddDays(7);

            _jwtTokenService
                .Setup(j => j.GenerateAccessToken(user))
                .Returns(("new-access-token", DateTime.UtcNow.AddMinutes(15)));

            _jwtTokenService
                .Setup(j => j.GenerateRefreshToken())
                .Returns(("new-raw-token", "new-hash", newExpiry));

            // Act
            var result = await _sut.RefreshTokenAsync(
                new RefreshTokenRequestDTO
                {
                    RefreshToken = oldRawToken
                });

            // Assert
            Assert.Equal("new-raw-token", result.RefreshToken);
            Assert.Equal("new-access-token", result.AccessToken);

            // Refresh token-ის raw მნიშვნელობა DB-ში არ უნდა იცოდესსს.
            Assert.Equal("new-hash", user.RefreshTokenHash);

            _unitOfWork.Verify(
                u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
