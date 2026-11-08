using System.Security.Claims;
using FitZone.Application.DTOs.VisitDTOs;
using FitZone.Application.ServiceInterfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FitZone.WebApi.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/visit")]
    public class VisitsController : ControllerBase
    {
        private readonly IVisitService _visitService;

        public VisitsController(IVisitService visitService)
        {
            _visitService = visitService;
        }

        /// <summary>Records a gym visit for the current user and uses one visit from a limited plan.</summary>
        /// <response code="200">Check-in recorded.</response>
        /// <response code="401">Not authenticated.</response>
        /// <response code="403">No active membership or no visits left.</response>
        [HttpPost("check-in")]
        [ProducesResponseType(typeof(CheckInResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<CheckInResponseDTO>> CheckIn(CancellationToken ct)
        {
            var result = await _visitService.CheckInAsync(GetUserId(), ct);
            return Ok(result);
        }

        /// <summary>Returns the current user's visit history.</summary>
        /// <response code="200">The list of visits.</response>
        /// <response code="401">Not authenticated.</response>
        [HttpGet("my")]
        [ProducesResponseType(typeof(ICollection<VisitDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ICollection<VisitDTO>>> GetMy(CancellationToken ct)
        {
            return Ok(await _visitService.GetMyVisitsAsync(GetUserId(), ct));
        }

        /// <summary>Admin only: corrects the check-in time of a visit.</summary>
        /// <response code="200">The updated visit.</response>
        /// <response code="400">The time is in the future or outside the membership period.</response>
        /// <response code="401">Not authenticated.</response>
        /// <response code="403">The user is not an Admin.</response>
        /// <response code="404">The visit does not exist.</response>
        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(VisitDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<VisitDTO>> Update(Guid id, UpdateVisitRequestDTO request, CancellationToken ct)
        {
            return Ok(await _visitService.UpdateAsync(id, request, ct));
        }

        /// <summary>Admin only: deletes a visit. On a limited, still active plan the visit is given back.</summary>
        /// <response code="204">Visit deleted.</response>
        /// <response code="401">Not authenticated.</response>
        /// <response code="403">The user is not an Admin.</response>
        /// <response code="404">The visit does not exist.</response>
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            await _visitService.DeleteAsync(id, ct);
            return NoContent();
        }

        private Guid GetUserId()
        {
            return Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }
    }
}