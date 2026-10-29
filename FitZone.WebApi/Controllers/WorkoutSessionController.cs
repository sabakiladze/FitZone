using FitZone.Application.DTOs;
using FitZone.Application.DTOs.WorkOutSessionDTOs;
using FitZone.Application.ServiceInterfaces;
using Microsoft.AspNetCore.Authorization;
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

        [HttpGet]
        public async Task<ActionResult<PagedResponseDTO<WorkoutSessionDTO>>> Get(
            [FromQuery] WorkoutSessionFilterDTO filter,
            [FromQuery] PaginationQueryDTO paging,
            CancellationToken ct)
        {
            var result = await _service.GetAsync(filter, paging, ct);
            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<WorkoutSessionDTO>> GetById(Guid id, CancellationToken ct)
        {
            var result = await _service.GetByIdAsync(id, ct);
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<WorkoutSessionDTO>> Create(
            [FromBody] CreateWorkoutSessionRequestDTO request,
            CancellationToken ct)
        {
            var created = await _service.AddAsync(request, ct);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] UpdateWorkoutSessionRequestDTO request,
            CancellationToken ct)
        {
            await _service.UpdateAsync(id, request, ct);
            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            await _service.RemoveAsync(id, ct);
            return NoContent();
        }
    }
}