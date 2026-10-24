using System.Security.Claims;
using FitZone.Application.DTOs.VisitDTOs;
using FitZone.Application.ServiceInterfaces;
using Microsoft.AspNetCore.Authorization;
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

        [HttpPost("check-in")]
        public async Task<ActionResult<CheckInResponseDTO>> CheckIn(Guid UserId, CancellationToken ct)
        {
            var result = await _visitService.CheckInAsync(GetUserId(), ct);
            return Ok(result);
        }

        [HttpGet("my")]
        public async Task<ActionResult<ICollection<VisitDTO>>> GetMy(CancellationToken ct)

        { return Ok(await _visitService.GetMyVisitsAsync(GetUserId(), ct));}

        private Guid GetUserId()
        {
            return Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }
    }
}