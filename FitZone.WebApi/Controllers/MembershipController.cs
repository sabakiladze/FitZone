using System.Security.Claims;
using FitZone.Application.DTOs.MembershipDTOs;
using FitZone.Application.ServiceInterfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FitZone.WebApi.Controllers
{
    [ApiController]
    [Route("api/memberships")]
    public class MembershipsController : ControllerBase
    {
        private readonly IMembershipService _membershipService;

        public MembershipsController(IMembershipService membershipService)
        {
            _membershipService = membershipService;
        }

        /// <summary>Returns all available membership plans. Public.</summary>
        /// <response code="200">The list of plans.</response>
        [HttpGet("plans")]
        [ProducesResponseType(typeof(ICollection<MembershipPlanDTO>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ICollection<MembershipPlanDTO>>> GetPlans(CancellationToken ct)
        {
            return Ok(await _membershipService.GetPlansAsync(ct));
        }

        /// <summary>Activates a membership plan for the current user.</summary>
        /// <param name="request">The plan to activate.</param>
        /// <response code="200">Membership activated.</response>
        /// <response code="400">Validation failed.</response>
        /// <response code="401">Not authenticated.</response>
        /// <response code="404">Plan not found.</response>
        /// <response code="409">The user already has an active plan.</response>
        [Authorize]
        [HttpPost("activate")]
        [ProducesResponseType(typeof(UserMembershipDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<UserMembershipDTO>> Activate(
            ActivateMembershipRequestDTO request, CancellationToken ct)
        {
            return Ok(await _membershipService.ActivateAsync(GetUserId(), request, ct));
        }

        /// <summary>Returns the current user's active membership.</summary>
        /// <response code="200">The active membership.</response>
        /// <response code="401">Not authenticated.</response>
        /// <response code="404">The user has no active membership.</response>
        [Authorize]
        [HttpGet("my")]
        [ProducesResponseType(typeof(UserMembershipDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserMembershipDTO>> GetMy(CancellationToken ct)
        {
            var result = await _membershipService.GetMyActiveAsync(GetUserId(), ct);
            return result is null ? NotFound() : Ok(result);
        }

        private Guid GetUserId()
        {
            return Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }
    }
}