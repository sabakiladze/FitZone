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


        /// <summary>Registers a new user and sends an email verification code.</summary>
        /// <param name="request">Email, password and profile data.</param>
        /// <response code="200">User registered; verification code sent.</response>
        /// <response code="400">Validation failed.</response>
        /// <response code="409">Email is already registered.</response>
        
        [HttpPost("register")]
        [ProducesResponseType(typeof(RegisterResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<RegisterResponseDTO>> Register(RegisterRequestDTO request, CancellationToken ct)
        {
            return Ok(await _authService.RegisterAsync(request, ct));
        }



        /// <summary>Authenticates a user and returns access and refresh tokens.</summary>
        /// <param name="request">Email and password.</param>
        /// <response code="200">Login successful.</response>
        /// <response code="400">Validation failed.</response>
        /// <response code="401">Wrong credentials or email not verified.</response>
        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<AuthResponseDTO>> Login(LoginRequestDTO request, CancellationToken ct)
        { 
            return Ok(await _authService.LoginAsync(request, ct)); 
        }

        /// <summary>Sends a new email verification code to the user.</summary>
        /// <param name="request">The email to send the code to.</param>
        /// <response code="204">Code sent.</response>
        /// <response code="400">Validation failed or email already verified.</response>
        /// <response code="404">No user with this email.</response>
        [HttpPost("send-verification-code")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult>SendVerificationCode(SentVerificationCodeDTO request, CancellationToken ct)
        {
            await _authService.SentVerifivationCode(request, ct);
            return NoContent();
        }



        /// <summary>Verifies the user's email with the code they received.</summary>
        /// <param name="request">Email and verification code.</param>
        /// <response code="204">Email verified.</response>
        /// <response code="400">Wrong or expired code.</response>
        [HttpPost("verify-email")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> VerifyEmail(VerificationEmailRequestDTO request, CancellationToken ct)
        {
            await _authService.VerifyEmailAsync(request, ct);
            return NoContent();
        }



        /// <summary>Issues a new access token using a valid refresh token.</summary>
        /// <param name="request">The refresh token.</param>
        /// <response code="200">New tokens issued.</response>
        /// <response code="401">Refresh token is invalid, expired or revoked.</response>
        [HttpPost("refresh")]
        [ProducesResponseType(typeof(AuthResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<AuthResponseDTO>> Refresh(RefreshTokenRequestDTO request, CancellationToken ct)
        {
            return Ok(await _authService.RefreshTokenAsync(request, ct));
        }


        /// <summary>Logs the current user out by revoking the refresh token.</summary>
        /// <param name="request">The refresh token to revoke.</param>
        /// <response code="204">Logged out.</response>
        /// <response code="401">Not authenticated.</response>
        [Authorize]
        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Logout(LogOutRequestDTO request, CancellationToken ct)
        {
            await _authService.LogOutAsync(request, ct);
            return NoContent();
        }
    }
}
