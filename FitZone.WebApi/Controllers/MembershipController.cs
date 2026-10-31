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

        /// <summary>Creates a membership plan. Admin only.</summary>
        /// <param name="request">Name, price, duration and optional visit limit.</param>
        /// <response code="201">Plan created.</response>
        /// <response code="400">Validation failed.</response>
        /// <response code="401">Not authenticated.</response>
        /// <response code="403">The user is not an Admin.</response>
        /// <response code="409">A plan with this name already exists.</response>
        [Authorize(Roles = "Admin")]
        [HttpPost("plans")]
        [ProducesResponseType(typeof(MembershipPlanDTO), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<MembershipPlanDTO>> CreatePlan(
            CreateMembershipPlanRequestDTO request, CancellationToken ct)
        {
            var created = await _membershipService.CreatePlanAsync(request, ct);
            return StatusCode(StatusCodes.Status201Created, created);
        }

        /// <summary>Updates a membership plan. Admin only.</summary>
        /// <param name="id">The plan id.</param>
        /// <param name="request">New plan data.</param>
        /// <response code="204">Plan updated.</response>
        /// <response code="400">Validation failed.</response>
        /// <response code="401">Not authenticated.</response>
        /// <response code="403">The user is not an Admin.</response>
        /// <response code="404">Plan not found.</response>
        /// <response code="409">Another plan already has this name.</response>
        [Authorize(Roles = "Admin")]
        [HttpPut("plans/{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> UpdatePlan(
            Guid id, UpdateMembershipPlanRequestDTO request, CancellationToken ct)
        {
            await _membershipService.UpdatePlanAsync(id, request, ct);
            return NoContent();
        }

        /// <summary>Deletes a membership plan. Admin only.</summary>
        /// <param name="id">The plan id.</param>
        /// <response code="204">Plan deleted.</response>
        /// <response code="401">Not authenticated.</response>
        /// <response code="403">The user is not an Admin.</response>
        /// <response code="404">Plan not found.</response>
        /// <response code="409">The plan is used by users and cannot be deleted.</response>
        [Authorize(Roles = "Admin")]
        [HttpDelete("plans/{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> DeletePlan(Guid id, CancellationToken ct)
        {
            await _membershipService.DeletePlanAsync(id, ct);
            return NoContent();
        }
        private Guid GetUserId()
        {
            return Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }
    }
}