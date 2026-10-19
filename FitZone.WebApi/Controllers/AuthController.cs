using FitZone.Application.DTOs.Auth;
using FitZone.Application.DTOs.AuthDTOs;
using FitZone.Application.ServiceInterfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FitZone.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authSertvice;
        public AuthController(IAuthService authSertvice)
        {
            _authSertvice = authSertvice;
        }
        [HttpPost("Register")]
        public async Task<ActionResult<RegisterResponseDTO>> Register(RegisterRequestDTO request, CancellationToken ct)
        {
            try
            {
                var result = await _authSertvice.RegisterAsync(request, ct);
                return Ok(result);
            }
            catch (Exception ex)
            {
                {
                    return BadRequest(ex.Message);
                }
            }
        }



    }
}
