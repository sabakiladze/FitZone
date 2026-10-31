using FitZone.Application.DTOs.BookingDTOs;
using FitZone.Application.DTOs.PaginationDTOs;
using FitZone.Application.ServiceInterfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FitZone.WebApi.Controllers
{
    [Route("api/bookings")]
    [ApiController]
    [Authorize]
    public class BookingsController : ControllerBase
    {
        private readonly IBookingService _service;

        public BookingsController(IBookingService service)
        {
            _service = service;
        }

        /// <summary>Books a workout session for the current user.</summary>
        /// <param name="request">The session to book.</param>
        /// <response code="201">Booking created.</response>
        /// <response code="400">Validation failed or the session has already started.</response>
        /// <response code="401">Not authenticated.</response>
        /// <response code="403">No active membership or no visits left.</response>
        /// <response code="404">Session or user not found.</response>
        /// <response code="409">Session is full, already booked, or time conflicts with another booking.</response>
        [HttpPost]
        [ProducesResponseType(typeof(BookingDTO), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<BookingDTO>> Create(
           [FromBody] CreateBookingRequestDTO request, CancellationToken ct)
        {
            var created = await _service.CreateAsync(GetUserId(), request, ct);
            return StatusCode(StatusCodes.Status201Created, created);
        }

        /// <summary>Cancels the current user's booking.</summary>
        /// <param name="id">The booking id.</param>
        /// <response code="204">Booking cancelled.</response>
        /// <response code="401">Not authenticated.</response>
        /// <response code="403">The booking belongs to another user.</response>
        /// <response code="404">Booking not found.</response>
        /// <response code="409">Booking is already cancelled.</response>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Cancel(Guid id, CancellationToken ct)
        {
            await _service.CancelAsync(GetUserId(), id, ct);
            return NoContent();
        }

        /// <summary>Returns the current user's bookings, newest first.</summary>
        /// <param name="paging">Page number and page size.</param>
        /// <response code="200">A page of bookings.</response>
        /// <response code="400">Invalid paging values.</response>
        /// <response code="401">Not authenticated.</response>
        [HttpGet("my")]
        [ProducesResponseType(typeof(PagedResponseDTO<BookingDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<PagedResponseDTO<BookingDTO>>> GetMy(
            [FromQuery] PaginationQueryDTO paging, CancellationToken ct)
        {
            var result = await _service.GetMyAsync(GetUserId(), paging, ct);
            return Ok(result);
        }

        /// <summary>Returns all bookings with optional filters. Admin only.</summary>
        /// <param name="filter">Optional filters: session, status, trainer name, date.</param>
        /// <param name="paging">Page number and page size.</param>
        /// <response code="200">A page of bookings.</response>
        /// <response code="400">Invalid filter or paging values.</response>
        /// <response code="401">Not authenticated.</response>
        /// <response code="403">The user is not an Admin.</response>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(PagedResponseDTO<BookingDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<PagedResponseDTO<BookingDTO>>> GetAll(
            [FromQuery] BookingFilterDTO filter,
            [FromQuery] PaginationQueryDTO paging,
            CancellationToken ct)
        {
            var result = await _service.GetAllAsync(filter, paging, ct);
            return Ok(result);
        }

        private Guid GetUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }
    }
}