using FitZone.Application.DTOs.TrainerDTOs;
using FitZone.Application.ServiceInterfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FitZone.WebApi.Controllers
{
    [ApiController]
    [Route("api/trainers")]
    public class TrainersController : ControllerBase
    {
        private readonly ITrainerService _trainerService;

        public TrainersController(ITrainerService trainerService)
        {
            _trainerService = trainerService;
        }

        /// <summary>Returns all trainers. Public.</summary>
        /// <response code="200">The list of trainers.</response>
        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyList<TrainerDTO>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<TrainerDTO>>> GetAll(CancellationToken ct)
            => Ok(await _trainerService.GetAllAsync(ct));

        /// <summary>Returns a trainer by id. Public.</summary>
        /// <param name="id">The trainer id.</param>
        /// <response code="200">The trainer.</response>
        /// <response code="404">Trainer not found.</response>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(TrainerDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<TrainerDTO>> GetById(Guid id, CancellationToken ct)
            => Ok(await _trainerService.GetByIdAsync(id, ct));

        /// <summary>Searches trainers by name and/or specialization. Public.</summary>
        /// <param name="name">Part of the trainer's full name.</param>
        /// <param name="specialization">Part of the specialization description.</param>
        /// <response code="200">Matching trainers; empty if none.</response>
        [HttpGet("search")]
        [ProducesResponseType(typeof(ICollection<TrainerDTO>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ICollection<TrainerDTO>>> Search(
            [FromQuery] string? name, [FromQuery] string? specialization, CancellationToken ct)
        {
            return Ok(await _trainerService.SearchAsync(name, specialization, ct));
        }

        /// <summary>Creates a trainer. Admin only.</summary>
        /// <param name="request">Trainer data.</param>
        /// <response code="201">Trainer created.</response>
        /// <response code="400">Validation failed.</response>
        /// <response code="401">Not authenticated.</response>
        /// <response code="403">The user is not an Admin.</response>
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ProducesResponseType(typeof(TrainerDTO), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<TrainerDTO>> Create(CreateTrainerRequestDTO request, CancellationToken ct)
        {
            var created = await _trainerService.AddAsync(request, GetAdminEmail(), ct);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        /// <summary>Updates a trainer. Admin only.</summary>
        /// <param name="id">The trainer id.</param>
        /// <param name="request">Updated trainer data.</param>
        /// <response code="204">Trainer updated.</response>
        /// <response code="400">Validation failed.</response>
        /// <response code="401">Not authenticated.</response>
        /// <response code="403">The user is not an Admin.</response>
        /// <response code="404">Trainer not found.</response>
        [Authorize(Roles = "Admin")]
        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(Guid id, UpdateTrainerRequestDTO request, CancellationToken ct)
        {
            await _trainerService.UpdateAsync(id, request, GetAdminEmail(), ct);
            return NoContent();
        }

        /// <summary>Deletes a trainer. Admin only.</summary>
        /// <param name="id">The trainer id.</param>
        /// <response code="204">Trainer deleted.</response>
        /// <response code="401">Not authenticated.</response>
        /// <response code="403">The user is not an Admin.</response>
        /// <response code="404">Trainer not found.</response>
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            await _trainerService.RemoveAsync(id, GetAdminEmail(), ct);
            return NoContent();
        }

        private string GetAdminEmail()
        {
            return User.FindFirstValue(ClaimTypes.Email)!;

            // update, add, remoce ისთვის ემაილს გავატან ტოკენიდან რა.
            // იმ შემთხვევაში თუ ისეთი ემაილი გადაეცა რომელიც არაა ადმინი,
            // ამ შემთხვევაში user ის ემაილი გადაეცემა, მაგრამ არ ამოაგდებს შეცდომას
            // რადგან authorize role==admin ჩააგდებს სანამ მივა საერთოდ სერვისამდე.


        }
    }

}
     

    
