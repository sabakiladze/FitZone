using FitZone.Application.DTOs.BookingDTOs;
using FitZone.Application.DTOs.MembershipDTOs;
using FitZone.Application.DTOs.PaginationDTOs;
using FitZone.Application.DTOs.ProfileDTOs;
using FitZone.Application.DTOs.UserDTOs;
using FitZone.Application.DTOs.VisitDTOs;
using FitZone.Application.ServiceInterfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FitZone.WebApi.Controllers
{
    /// <summary>Admin-only: browse users and see their membership, bookings and visits.</summary>
    [ApiController]
    [Authorize(Roles = "Admin")]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IProfileService _profileService;
        private readonly IMembershipService _membershipService;
        private readonly IBookingService _bookingService;
        private readonly IVisitService _visitService;

        public UsersController(IUserService userService, IProfileService profileService,
            IMembershipService membershipService, IBookingService bookingService, IVisitService visitService)
        {
            _userService = userService;
            _profileService = profileService;
            _membershipService = membershipService;
            _bookingService = bookingService;
            _visitService = visitService;
        }

        /// <summary>Returns a page of users (deleted users are not listed). Admin only.</summary>
        /// <param name="search">Optional text searched in user name, first name, last name and e-mail.</param>
        /// <param name="paging">Page and page size (1-50).</param>
        /// <response code="200">The page of users.</response>
        /// <response code="400">Invalid page or page size.</response>
        /// <response code="401">Not authenticated.</response>
        /// <response code="403">The user is not an Admin.</response>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResponseDTO<UserListItemDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<PagedResponseDTO<UserListItemDTO>>> GetAll(
            [FromQuery] string? search, [FromQuery] PaginationQueryDTO paging, CancellationToken ct)
        {
            return Ok(await _userService.GetAllAsync(search, paging, ct));
        }

        /// <summary>Returns the profile of one user. Admin only.</summary>
        /// <response code="200">The profile.</response>
        /// <response code="401">Not authenticated.</response>
        /// <response code="403">The user is not an Admin.</response>
        /// <response code="404">The user was not found.</response>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ProfileDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProfileDTO>> Get(Guid id, CancellationToken ct)
        {
            return Ok(await _profileService.GetAsync(id, ct));
        }

        /// <summary>Deletes a user (soft delete: personal data is wiped, the account can no longer log in). Admin only.</summary>
        /// <response code="204">User deleted.</response>
        /// <response code="401">Not authenticated.</response>
        /// <response code="403">The user is not an Admin, or the target is an Admin.</response>
        /// <response code="404">The user was not found.</response>
        /// <response code="409">The user has upcoming bookings.</response>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            await _userService.DeleteAsync(id, ct);
            return NoContent();
        }

        /// <summary>Returns the active membership of a user. Admin only.</summary>
        /// <response code="200">The active membership.</response>
        /// <response code="401">Not authenticated.</response>
        /// <response code="403">The user is not an Admin.</response>
        /// <response code="404">The user has no active membership.</response>
        [HttpGet("{id:guid}/membership")]
        [ProducesResponseType(typeof(UserMembershipDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserMembershipDTO>> GetMembership(Guid id, CancellationToken ct)
        {
            var result = await _membershipService.GetMyActiveAsync(id, ct);
            return result is null ? NotFound() : Ok(result);
        }

        /// <summary>Returns a page of a user's bookings. Admin only.</summary>
        /// <response code="200">The page of bookings.</response>
        /// <response code="400">Invalid page or page size.</response>
        /// <response code="401">Not authenticated.</response>
        /// <response code="403">The user is not an Admin.</response>
        [HttpGet("{id:guid}/bookings")]
        [ProducesResponseType(typeof(PagedResponseDTO<BookingDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<PagedResponseDTO<BookingDTO>>> GetBookings(
            Guid id, [FromQuery] PaginationQueryDTO paging, CancellationToken ct)
        {
            return Ok(await _bookingService.GetMyAsync(id, paging, ct));
        }

        /// <summary>Returns all visits of a user, newest first (with ids for PUT/DELETE visit/{id}). Admin only.</summary>
        /// <response code="200">The visits.</response>
        /// <response code="401">Not authenticated.</response>
        /// <response code="403">The user is not an Admin.</response>
        [HttpGet("{id:guid}/visits")]
        [ProducesResponseType(typeof(ICollection<VisitDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ICollection<VisitDTO>>> GetVisits(Guid id, CancellationToken ct)
        {
            return Ok(await _visitService.GetMyVisitsAsync(id, ct));
        }
    }
}
