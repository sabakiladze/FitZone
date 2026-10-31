using FitZone.Application.DTOs.PaginationDTOs;
using FitZone.Application.DTOs.WorkOutSessionDTOs;
using FitZone.Application.ServiceInterfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FitZone.WebApi.Controllers
{
    [ApiController]
    [Route("api/training-sessions")]
    [Authorize]
    public class WorkoutSessionController : ControllerBase
    {
        private readonly IWorkOutSessionService _service;

        public WorkoutSessionController(IWorkOutSessionService service)
        {
            _service = service;
        }

        /// <summary>Returns workout sessions with optional filters, ordered by start time.</summary>
        /// <param name="filter">Optional filters (trainer, date).</param>
        /// <param name="paging">Page number and page size.</param>
        /// <response code="200">A page of sessions, including the number of free spots.</response>
        /// <response code="400">Invalid filter or paging values.</response>
        /// <response code="401">Not authenticated.</response>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResponseDTO<WorkoutSessionDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<PagedResponseDTO<WorkoutSessionDTO>>> Get(
            [FromQuery] WorkoutSessionFilterDTO filter,
            [FromQuery] PaginationQueryDTO paging,
            CancellationToken ct)
        {
            var result = await _service.GetAsync(filter, paging, ct);
            return Ok(result);
        }

        /// <summary>Returns a workout session by id.</summary>
        /// <param name="id">The session id.</param>
        /// <response code="200">The session.</response>
        /// <response code="401">Not authenticated.</response>
        /// <response code="404">Session not found.</response>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(WorkoutSessionDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<WorkoutSessionDTO>> GetById(Guid id, CancellationToken ct)
        {
            var result = await _service.GetByIdAsync(id, ct);
            return Ok(result);
        }

        /// <summary>Creates a workout session. Admin only.</summary>
        /// <param name="request">Trainer, start and end time (UTC), capacity.</param>
        /// <response code="201">Session created.</response>
        /// <response code="400">Validation failed (past start, end before start, capacity below 1).</response>
        /// <response code="401">Not authenticated.</response>
        /// <response code="403">The user is not an Admin.</response>
        /// <response code="404">Trainer not found.</response>
        /// <response code="409">The trainer already has a session in this time range.</response>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(WorkoutSessionDTO), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<WorkoutSessionDTO>> Create(
            [FromBody] CreateWorkoutSessionRequestDTO request,
            CancellationToken ct)
        {
            var created = await _service.AddAsync(request, ct);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        /// <summary>Updates a workout session. Admin only.</summary>
        /// <param name="id">The session id.</param>
        /// <param name="request">New start and end time (UTC) and capacity.</param>
        /// <response code="204">Session updated.</response>
        /// <response code="400">Validation failed.</response>
        /// <response code="401">Not authenticated.</response>
        /// <response code="403">The user is not an Admin.</response>
        /// <response code="404">Session not found.</response>
        /// <response code="400">The trainer has another session in this time range, or capacity is below the number of active bookings.</response>
        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] UpdateWorkoutSessionRequestDTO request,
            CancellationToken ct)
        {
            await _service.UpdateAsync(id, request, ct);
            return NoContent();
        }

        /// <summary>Deletes a workout session. Admin only.</summary>
        /// <param name="id">The session id.</param>
        /// <response code="204">Session deleted.</response>
        /// <response code="401">Not authenticated.</response>
        /// <response code="403">The user is not an Admin.</response>
        /// <response code="404">Session not found.</response>
        /// <response code="409">The session has bookings and cannot be deleted.</response>
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            await _service.RemoveAsync(id, ct);
            return NoContent();
        }
    }
}