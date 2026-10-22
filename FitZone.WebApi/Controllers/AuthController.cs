using FitZone.Application.DTOs.Auth;
using FitZone.Application.DTOs.AuthDTOs;
using FitZone.Application.ServiceInterfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FitZone.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<RegisterResponseDTO>> Register(RegisterRequestDTO request, CancellationToken ct)
        {
            return Ok(await _authService.RegisterAsync(request, ct));
        }

        
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDTO>> Login(LoginRequestDTO request, CancellationToken ct)
        { 
            return Ok(await _authService.LoginAsync(request, ct)); 
        }

        [HttpPost("send-verification-code")]
        public async Task<ActionResult>SendVerificationCode(SentVerificationCodeDTO request, CancellationToken ct)
        {
            await _authService.SentVerifivationCode(request, ct);
            return NoContent();
        }

        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail(VerificationEmailRequestDTO request, CancellationToken ct)
        {
            await _authService.VerifyEmailAsync(request, ct);
            return NoContent();
        }

        [HttpPost("refresh")]
        public async Task<ActionResult<AuthResponseDTO>> Refresh(RefreshTokenRequestDTO request, CancellationToken ct)
        {
            return Ok(await _authService.RefreshTokenAsync(request, ct));
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout(LogOutRequestDTO request, CancellationToken ct)
        {
            await _authService.LogOutAsync(request, ct);
            return NoContent();
        }
    }
}
