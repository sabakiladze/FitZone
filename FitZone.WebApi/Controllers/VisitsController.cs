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
    [Route("api/visits")]
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

        private Guid GetUserId()
        {
            return Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }
    }
}