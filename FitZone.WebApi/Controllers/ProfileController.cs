using System.Security.Claims;
using FitZone.Application.DTOs.ProfileDTOs;
using FitZone.Application.ServiceInterfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FitZone.WebApi.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/profile")]
    public class ProfileController : ControllerBase
    {
        private readonly IProfileService _profileService;

        public ProfileController(IProfileService profileService)
        {
            _profileService = profileService;
        }

        /// <summary>Returns the profile of the current user (name, e-mail, role, registration date).</summary>
        /// <response code="200">The profile.</response>
        /// <response code="401">Not authenticated.</response>
        /// <response code="404">The user no longer exists.</response>
        [HttpGet]
        [ProducesResponseType(typeof(ProfileDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProfileDTO>> Get(CancellationToken ct)
        {
            return Ok(await _profileService.GetAsync(GetUserId(), ct));
        }

        private Guid GetUserId()
        {
            return Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }
    }
}
