using FitZone.Application.DTOs;
using FitZone.Application.DTOs.BookingDTOs;
using FitZone.Application.ServiceInterfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FitZone.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingServiceController : ControllerBase
    {
        private readonly IBookingService _service;

        public BookingServiceController(IBookingService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<ActionResult<BookingDTO>> Create(
           [FromBody] CreateBookingRequestDTO request, CancellationToken ct)
        {
            var created = await _service.CreateAsync(GetUserId(), request, ct);
            return StatusCode(StatusCodes.Status201Created, created);
        }


        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Cancel(Guid id, CancellationToken ct)
        {
            await _service.CancelAsync(GetUserId(), id, ct);
            return NoContent();
        }

        [HttpGet("my")]
        public async Task<ActionResult<PagedResponseDTO<BookingDTO>>> GetMy(
            [FromQuery] PaginationQueryDTO paging, CancellationToken ct)
        {
            var result = await _service.GetMyAsync(GetUserId(), paging, ct);
            return Ok(result);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
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
