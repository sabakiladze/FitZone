using FitZone.Application.DTOs.TrainerDTOs;
using FitZone.Application.ServiceInterfaces;
using Microsoft.AspNetCore.Authorization;
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

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<TrainerDTO>>> GetAll(CancellationToken ct)
            => Ok(await _trainerService.GetAllAsync(ct));

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<TrainerDTO>> GetById(Guid id, CancellationToken ct)
            => Ok(await _trainerService.GetByIdAsync(id, ct));

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<TrainerDTO>> Create(CreateTrainerRequestDTO request, string adminEmail, CancellationToken ct)
        {
            var created = await _trainerService.AddAsync(request,adminEmail, ct);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);

        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, string adminEmail, UpdateTrainerRequestDTO request, CancellationToken ct)
        {
            await _trainerService.UpdateAsync(id, request, adminEmail, ct);
            return NoContent();
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, string adminEmail, CancellationToken ct)
        {
            await _trainerService.RemoveAsync(id,adminEmail, ct);
            return NoContent();
        }

        [HttpGet]
        public async Task<ActionResult<ICollection<TrainerDTO>>> Search([FromQuery] string? name, [FromQuery] string? specialization, CancellationToken ct)
        {
            return Ok(await _trainerService.SearchAsync(name, specialization, ct));
        }

        private Guid GetAdminId()
        {
            return Guid.Parse(User.FindFirstValue(ClaimTypes.Email)!);

        }

        // update, add, remoce ისთვის ემაილს გავატან ტოკენიდან რა.
        // იმ შემთხვევაში თუ ისეთი ემაილი გადაეცა რომელიც არაა ადმინი,
        // ამ შემთხვევაში user ის ემაილი გადაეცემა, მაგრამ არ ამოაგდებს შეცდომას
        // რადგან authorize role==admin ჩააგდებს სანამ მივა საერთოდ სერვისამდე.
    }
}