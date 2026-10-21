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
    [Route("api/auth")]
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

        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail(VerificationEmailRequestDTO request, CancellationToken ct)
        {
            await _authService.VerifyEmailAsync(request, ct);
            return NoContent();
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout(LogOutRequestDTO request, CancellationToken ct)
        {
            await _authService.LogOutAsync(request.RefreshToken, ct);
            return NoContent();
        }

        [Authorize]
        [HttpDelete("me")]
        public async Task<IActionResult> DeleteAccount(DeleteAccountRequestDTO request, CancellationToken ct)
        {
            await _authService.DeleteUserAsync(GetUserId(), request, ct);
            return NoContent();
        }

        [Authorize]
        [HttpGet("me")]
        public IActionResult Me() => Ok(new { UserId = GetUserId() });

        private Guid GetUserId() =>
            Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }
}
